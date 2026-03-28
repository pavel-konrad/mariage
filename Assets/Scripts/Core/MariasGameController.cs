using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MariasGame.Core.Events;
using MariasGame.Core.Interfaces;
using MariasGame.ScriptableObjects;

namespace MariasGame.Core
{
    /// <summary>
    /// Řídí průběh jednoho kola Mariáše.
    /// Pure C# třída – žádné Unity závislosti mimo event manažery.
    /// Komunikuje výhradně přes ISubject event managery.
    /// </summary>
    public class MariasGameController
    {
        private readonly IDeckFactory _deckFactory;
        private readonly ISubject<GameEvent> _gameEvents;
        private readonly ISubject<CardEvent> _cardEvents;
        private readonly ISubject<ScoreEvent> _scoreEvents;
        private GameModeConfig _gameMode;

        private MariasGameState _state;
        private IDeck _deck;

        // Stav dražby (platný jen v GamePhase.Bidding)
        private readonly HashSet<int> _passedBidders = new();
        private MariasGameRules.BidOption _currentHighBid;
        private int _currentHighBidder;

        public MariasGameState State => _state;

        public MariasGameController(
            IDeckFactory deckFactory,
            ISubject<GameEvent> gameEvents,
            ISubject<CardEvent> cardEvents,
            ISubject<ScoreEvent> scoreEvents)
        {
            _deckFactory = deckFactory ?? throw new ArgumentNullException(nameof(deckFactory));
            _gameEvents  = gameEvents  ?? throw new ArgumentNullException(nameof(gameEvents));
            _cardEvents  = cardEvents  ?? throw new ArgumentNullException(nameof(cardEvents));
            _scoreEvents = scoreEvents ?? throw new ArgumentNullException(nameof(scoreEvents));
        }

        public void Initialize(GameModeConfig gameMode)
        {
            _gameMode = gameMode ?? throw new ArgumentNullException(nameof(gameMode));
        }

        // ─── Public API ───────────────────────────────────────────────────────────

        public void StartNewGame(List<string> playerNames)
        {
            _state = MariasGameState.CreateNew(playerNames);
            _deck  = _deckFactory.CreateStandardDeck();

            _gameEvents.NotifyObservers(new GameEvent { Type = GameEventType.GameStarted });

            ShuffleAndDeal();
        }

        /// <summary>
        /// Hráč učiní nabídku v dražbě.
        /// V Easy módu (HumanAlwaysDeclarer) hráč volí přímo typ hry – bidding fáze je přeskočena.
        /// </summary>
        public void MakeBid(int playerIndex, MariasGameRules.BidOption bid)
        {
            if (_state.Phase != GamePhase.Bidding)
            {
                Debug.LogWarning("[MariasGameController] MakeBid voláno mimo Bidding fázi.");
                return;
            }

            if (playerIndex != _state.CurrentPlayerIndex)
            {
                Debug.LogWarning("[MariasGameController] MakeBid: není tah tohoto hráče.");
                return;
            }

            switch (bid)
            {
                case MariasGameRules.BidOption.Pass:
                    _passedBidders.Add(playerIndex);
                    break;

                case MariasGameRules.BidOption.Double:
                case MariasGameRules.BidOption.Redouble:
                case MariasGameRules.BidOption.Tutti:
                    if (!_gameMode.HasFlekRe) return;
                    _state.BetMultiplier *= 2;
                    _gameEvents.NotifyObservers(new GameEvent
                    {
                        Type = GameEventType.BetDoubled,
                        BetMultiplier = _state.BetMultiplier
                    });
                    AdvanceToNextBidder();
                    return;

                default:
                    // Game, Seven, Hundred, Bettel, Durch
                    if (!MariasGameRules.IsValidBid(_currentHighBid, bid))
                    {
                        Debug.LogWarning($"[MariasGameController] Neplatná nabídka {bid} přes {_currentHighBid}.");
                        return;
                    }
                    if (!_gameMode.HasBettelDurch &&
                        (bid == MariasGameRules.BidOption.Bettel || bid == MariasGameRules.BidOption.Durch))
                    {
                        Debug.LogWarning("[MariasGameController] Betl/Durch není povolen v tomto módu.");
                        return;
                    }

                    _currentHighBid    = bid;
                    _currentHighBidder = playerIndex;
                    _state.GameType    = BidToGameType(bid);
                    _state.DeclarerIndex = playerIndex;

                    NotifyGameBidDeclared(bid);
                    break;
            }

            ResolveBiddingState();
        }

        /// <summary>
        /// Forhont odhodí 2 karty do talonu.
        /// </summary>
        public void DiscardToTalon(List<Card> cardsToDiscard)
        {
            if (_state.Phase != GamePhase.DiscardingTalon)
            {
                Debug.LogWarning("[MariasGameController] DiscardToTalon voláno mimo DiscardingTalon fázi.");
                return;
            }

            if (cardsToDiscard.Count != 2)
            {
                Debug.LogWarning("[MariasGameController] Musí být odhozeny právě 2 karty.");
                return;
            }

            var hand = _state.PlayerHands[_state.DeclarerIndex];
            foreach (var card in cardsToDiscard)
            {
                if (!hand.Contains(card))
                {
                    Debug.LogWarning($"[MariasGameController] Karta {card} není v ruce forhonta.");
                    return;
                }
            }

            foreach (var card in cardsToDiscard)
                hand.Remove(card);

            _state.DiscardedTalon = cardsToDiscard;
            _state.PlayerTrickPoints[_state.DeclarerIndex] += MariasGameRules.CalculatePoints(cardsToDiscard);

            _cardEvents.NotifyObservers(new CardEvent
            {
                Type        = CardEventType.TalonDiscarded,
                PlayerIndex = _state.DeclarerIndex,
                Cards       = cardsToDiscard.AsReadOnly()
            });

            StartDeclaring();
        }

        /// <summary>
        /// Forhont zvolí trumfovou barvu.
        /// </summary>
        public void DeclareTrump(CardSuit trumpSuit)
        {
            if (_state.Phase != GamePhase.Declaring)
            {
                Debug.LogWarning("[MariasGameController] DeclareTrump voláno mimo Declaring fázi.");
                return;
            }

            if (!MariasGameRules.GameHasTrumps(_state.GameType))
            {
                Debug.LogWarning("[MariasGameController] Tento typ hry nemá trumfy.");
                return;
            }

            _state.TrumpSuit = trumpSuit;

            _gameEvents.NotifyObservers(new GameEvent
            {
                Type      = GameEventType.TrumpDeclared,
                TrumpSuit = trumpSuit
            });

            StartPlaying();
        }

        /// <summary>
        /// Hráč hlásí pár (Král + Dáma stejné barvy).
        /// Lze volat pouze při vynášení (CurrentTrick prázdný).
        /// </summary>
        public void DeclareMarriage(CardSuit suit)
        {
            if (_state.Phase != GamePhase.Playing)
            {
                Debug.LogWarning("[MariasGameController] DeclareMarriage voláno mimo Playing fázi.");
                return;
            }

            if (_state.CurrentTrick.Count > 0)
            {
                Debug.LogWarning("[MariasGameController] Hláška lze ohlásit pouze při vynášení.");
                return;
            }

            var hand = _state.GetCurrentPlayerHand();
            if (!MariasGameRules.CanDeclareMarriage(hand, suit))
            {
                Debug.LogWarning("[MariasGameController] Hráč nemá tento pár.");
                return;
            }

            if (_state.DeclaredMarriages.Contains(suit))
            {
                Debug.LogWarning("[MariasGameController] Tento pár již byl hlášen.");
                return;
            }

            _state.DeclaredMarriages.Add(suit);
            int points = MariasGameRules.GetMarriageValue(suit, _state.TrumpSuit);
            _state.PlayerMarriagePoints[_state.CurrentPlayerIndex] += points;

            _gameEvents.NotifyObservers(new GameEvent
            {
                Type          = GameEventType.MarriageDeclared,
                PlayerIndex   = _state.CurrentPlayerIndex,
                TrumpSuit     = suit,
                MarriagePoints = points
            });

            _scoreEvents.NotifyObservers(new ScoreEvent
            {
                Type       = ScoreEventType.ScoreChanged,
                PlayerIndex = _state.CurrentPlayerIndex,
                NewScore   = _state.PlayerTrickPoints[_state.CurrentPlayerIndex]
                           + _state.PlayerMarriagePoints[_state.CurrentPlayerIndex]
            });
        }

        /// <summary>
        /// Zahraje kartu. Vrací false pokud je tah neplatný.
        /// </summary>
        public bool PlayCard(int playerIndex, Card card)
        {
            if (_state.Phase != GamePhase.Playing)
            {
                Debug.LogWarning("[MariasGameController] PlayCard voláno mimo Playing fázi.");
                return false;
            }

            if (playerIndex != _state.CurrentPlayerIndex)
            {
                Debug.LogWarning("[MariasGameController] PlayCard: není tah tohoto hráče.");
                return false;
            }

            var hand = _state.PlayerHands[playerIndex];
            if (!hand.Contains(card))
            {
                Debug.LogWarning($"[MariasGameController] Karta {card} není v ruce hráče {playerIndex}.");
                return false;
            }

            var legalPlays = MariasGameRules.GetLegalPlays(hand, _state.CurrentTrick, _state.TrumpSuit);
            if (!legalPlays.Contains(card))
            {
                Debug.LogWarning($"[MariasGameController] Karta {card} není legální tah.");
                return false;
            }

            hand.Remove(card);
            _state.CurrentTrick.Add(card);

            _cardEvents.NotifyObservers(new CardEvent
            {
                Type        = CardEventType.CardPlayed,
                PlayerIndex = playerIndex,
                Card        = card
            });

            if (_state.IsTrickComplete())
                ResolveTrick();
            else
                AdvancePlayTurn();

            return true;
        }

        public List<Card> GetLegalPlays()
        {
            if (_state.Phase != GamePhase.Playing) return new List<Card>();
            return MariasGameRules.GetLegalPlays(
                _state.GetCurrentPlayerHand(), _state.CurrentTrick, _state.TrumpSuit);
        }

        public List<CardSuit> GetPossibleMarriages()
        {
            if (_state.Phase != GamePhase.Playing || _state.CurrentTrick.Count > 0)
                return new List<CardSuit>();

            return MariasGameRules.GetPossibleMarriages(_state.GetCurrentPlayerHand())
                .Where(s => !_state.DeclaredMarriages.Contains(s))
                .ToList();
        }

        // ─── Private: Dealing ─────────────────────────────────────────────────────

        private void ShuffleAndDeal()
        {
            if (_deck is IShuffleable shuffleable)
                shuffleable.Shuffle();

            _state.Phase = GamePhase.Dealing;
            _gameEvents.NotifyObservers(new GameEvent { Type = GameEventType.PhaseChanged, Phase = _state.Phase });

            for (int i = 0; i < 3; i++)
            {
                var cards = new List<Card>();
                for (int j = 0; j < MariasGameRules.CardsPerPlayer; j++)
                    cards.Add(_deck.DrawCard());

                _state.PlayerHands[i] = cards;

                _cardEvents.NotifyObservers(new CardEvent
                {
                    Type        = CardEventType.CardsDealt,
                    PlayerIndex = i,
                    Cards       = cards.AsReadOnly()
                });
            }

            _state.Talon.Clear();
            _state.Talon.Add(_deck.DrawCard());
            _state.Talon.Add(_deck.DrawCard());

            StartBidding();
        }

        // ─── Private: Bidding ─────────────────────────────────────────────────────

        private void StartBidding()
        {
            if (_gameMode.HumanAlwaysDeclarer)
            {
                // Easy mód: lidský hráč je vždy forhont, dražba se přeskočí
                _state.DeclarerIndex = 0;
                _state.GameType      = MariasGameRules.GameType.Normal;
                StartTakingTalon();
                return;
            }

            _passedBidders.Clear();
            _currentHighBid    = MariasGameRules.BidOption.Pass;
            _currentHighBidder = -1;

            _state.Phase               = GamePhase.Bidding;
            _state.CurrentPlayerIndex  = 0;

            _gameEvents.NotifyObservers(new GameEvent { Type = GameEventType.PhaseChanged, Phase = _state.Phase });
            _gameEvents.NotifyObservers(new GameEvent { Type = GameEventType.PlayerTurnStarted, PlayerIndex = 0 });
        }

        private void ResolveBiddingState()
        {
            int remaining = 3 - _passedBidders.Count;

            if (remaining == 0)
            {
                // Všichni pasovali – lidský hráč hraje Normal
                _state.GameType      = MariasGameRules.GameType.Normal;
                _state.DeclarerIndex = 0;
                StartTakingTalon();
                return;
            }

            if (remaining == 1)
            {
                if (_currentHighBidder >= 0)
                {
                    // Vítěz dražby je znám
                    StartTakingTalon();
                }
                else
                {
                    // Poslední zbývající hráč, nikdo jiný nenabídl
                    int last = FindLastRemainingBidder();
                    _state.GameType      = MariasGameRules.GameType.Normal;
                    _state.DeclarerIndex = last;
                    StartTakingTalon();
                }
                return;
            }

            AdvanceToNextBidder();
        }

        private void AdvanceToNextBidder()
        {
            do
            {
                _state.CurrentPlayerIndex = (_state.CurrentPlayerIndex + 1) % 3;
            }
            while (_passedBidders.Contains(_state.CurrentPlayerIndex));

            _gameEvents.NotifyObservers(new GameEvent
            {
                Type        = GameEventType.PlayerTurnStarted,
                PlayerIndex = _state.CurrentPlayerIndex
            });
        }

        private int FindLastRemainingBidder()
        {
            for (int i = 0; i < 3; i++)
                if (!_passedBidders.Contains(i)) return i;
            return 0;
        }

        private void NotifyGameBidDeclared(MariasGameRules.BidOption bid)
        {
            var type = bid switch
            {
                MariasGameRules.BidOption.Seven   => GameEventType.SevenDeclared,
                MariasGameRules.BidOption.Hundred => GameEventType.HundredDeclared,
                MariasGameRules.BidOption.Bettel  => GameEventType.BettelDeclared,
                MariasGameRules.BidOption.Durch   => GameEventType.DurchDeclared,
                _                                 => GameEventType.PhaseChanged
            };

            _gameEvents.NotifyObservers(new GameEvent { Type = type, PlayerIndex = _state.DeclarerIndex });
        }

        private static MariasGameRules.GameType BidToGameType(MariasGameRules.BidOption bid) => bid switch
        {
            MariasGameRules.BidOption.Seven   => MariasGameRules.GameType.Seven,
            MariasGameRules.BidOption.Hundred => MariasGameRules.GameType.Hundred,
            MariasGameRules.BidOption.Bettel  => MariasGameRules.GameType.Bettel,
            MariasGameRules.BidOption.Durch   => MariasGameRules.GameType.Durch,
            _                                 => MariasGameRules.GameType.Normal
        };

        // ─── Private: Talon ───────────────────────────────────────────────────────

        private void StartTakingTalon()
        {
            _state.Phase              = GamePhase.TakingTalon;
            _state.CurrentPlayerIndex = _state.DeclarerIndex;

            _gameEvents.NotifyObservers(new GameEvent { Type = GameEventType.PhaseChanged, Phase = _state.Phase });

            var talon = _state.Talon.AsReadOnly();
            _state.PlayerHands[_state.DeclarerIndex].AddRange(_state.Talon);

            _cardEvents.NotifyObservers(new CardEvent
            {
                Type        = CardEventType.TalonTaken,
                PlayerIndex = _state.DeclarerIndex,
                Cards       = talon
            });

            _state.Phase = GamePhase.DiscardingTalon;
            _gameEvents.NotifyObservers(new GameEvent { Type = GameEventType.PhaseChanged, Phase = _state.Phase });
            _gameEvents.NotifyObservers(new GameEvent { Type = GameEventType.PlayerTurnStarted, PlayerIndex = _state.DeclarerIndex });
        }

        // ─── Private: Declaring ───────────────────────────────────────────────────

        private void StartDeclaring()
        {
            if (!MariasGameRules.GameHasTrumps(_state.GameType))
            {
                // Betl / Durch – žádný trumf, přejít rovnou na hraní
                _state.TrumpSuit = null;
                StartPlaying();
                return;
            }

            _state.Phase = GamePhase.Declaring;
            _gameEvents.NotifyObservers(new GameEvent { Type = GameEventType.PhaseChanged, Phase = _state.Phase });
            _gameEvents.NotifyObservers(new GameEvent { Type = GameEventType.PlayerTurnStarted, PlayerIndex = _state.DeclarerIndex });
        }

        // ─── Private: Playing ─────────────────────────────────────────────────────

        private void StartPlaying()
        {
            _state.Phase              = GamePhase.Playing;
            _state.TrickNumber        = 1;
            _state.CurrentPlayerIndex = _state.DeclarerIndex;
            _state.TrickLeaderIndex   = _state.DeclarerIndex;
            _state.CurrentTrick.Clear();

            _gameEvents.NotifyObservers(new GameEvent { Type = GameEventType.PhaseChanged, Phase = _state.Phase });

            NotifyLegalPlays();
            _gameEvents.NotifyObservers(new GameEvent { Type = GameEventType.PlayerTurnStarted, PlayerIndex = _state.CurrentPlayerIndex });
        }

        private void AdvancePlayTurn()
        {
            _state.NextPlayer();
            NotifyLegalPlays();
            _gameEvents.NotifyObservers(new GameEvent { Type = GameEventType.PlayerTurnStarted, PlayerIndex = _state.CurrentPlayerIndex });
        }

        private void NotifyLegalPlays()
        {
            var legal = MariasGameRules.GetLegalPlays(
                _state.GetCurrentPlayerHand(), _state.CurrentTrick, _state.TrumpSuit);

            _cardEvents.NotifyObservers(new CardEvent
            {
                Type  = CardEventType.LegalPlaysUpdated,
                Cards = legal.AsReadOnly()
            });
        }

        private void ResolveTrick()
        {
            int relativeWinner = MariasGameRules.DetermineTrickWinner(_state.CurrentTrick, _state.TrumpSuit);
            int absoluteWinner = (_state.TrickLeaderIndex + relativeWinner) % 3;

            int points = MariasGameRules.CalculatePoints(_state.CurrentTrick);
            _state.PlayerTrickPoints[absoluteWinner] += points;
            _state.PlayerTrickCount[absoluteWinner]++;

            _state.TrickHistory.Add(new TrickHistory
            {
                TrickNumber = _state.TrickNumber,
                Cards       = new List<Card>(_state.CurrentTrick),
                WinnerIndex = absoluteWinner,
                Points      = points
            });

            _gameEvents.NotifyObservers(new GameEvent
            {
                Type        = GameEventType.TrickWon,
                PlayerIndex = absoluteWinner
            });

            _scoreEvents.NotifyObservers(new ScoreEvent
            {
                Type       = ScoreEventType.ScoreChanged,
                PlayerIndex = absoluteWinner,
                NewScore   = _state.PlayerTrickPoints[absoluteWinner]
                           + _state.PlayerMarriagePoints[absoluteWinner]
            });

            if (_state.IsLastTrick())
            {
                EndGame();
                return;
            }

            _state.TrickNumber++;
            _state.CurrentTrick.Clear();
            _state.TrickLeaderIndex   = absoluteWinner;
            _state.CurrentPlayerIndex = absoluteWinner;

            NotifyLegalPlays();
            _gameEvents.NotifyObservers(new GameEvent { Type = GameEventType.PlayerTurnStarted, PlayerIndex = absoluteWinner });
        }

        // ─── Private: End game ────────────────────────────────────────────────────

        private void EndGame()
        {
            _state.Phase = GamePhase.Scoring;
            _gameEvents.NotifyObservers(new GameEvent { Type = GameEventType.PhaseChanged, Phase = _state.Phase });

            bool sevenWon  = false;
            bool hundredWon = false;

            if (_state.SevenDeclared && _state.TrumpSuit.HasValue && _state.TrickHistory.Count > 0)
            {
                var lastTrick   = _state.TrickHistory.Last();
                var winningCard = lastTrick.Cards[MariasGameRules.DetermineTrickWinner(lastTrick.Cards, _state.TrumpSuit)];
                sevenWon = MariasGameRules.WonSevenWithTrumpSeven(
                    winningCard, _state.TrumpSuit.Value, lastTrick.WinnerIndex, _state.DeclarerIndex);

                _gameEvents.NotifyObservers(new GameEvent { Type = GameEventType.SevenResolved, SevenWon = sevenWon });
            }

            if (_state.HundredDeclared)
            {
                int total = _state.PlayerTrickPoints[_state.DeclarerIndex]
                          + _state.PlayerMarriagePoints[_state.DeclarerIndex];
                hundredWon = total >= 100;

                _gameEvents.NotifyObservers(new GameEvent { Type = GameEventType.HundredResolved, HundredWon = hundredWon });
            }

            _state.Result = MariasGameRules.CalculateGameResult(
                _state.PlayerTrickPoints,
                _state.PlayerMarriagePoints,
                _state.GameType,
                _state.DeclarerIndex,
                sevenWon,
                !sevenWon && _state.SevenDeclared,
                _state.BetMultiplier);

            int declarerPoints = _state.PlayerTrickPoints[_state.DeclarerIndex]
                               + _state.PlayerMarriagePoints[_state.DeclarerIndex];

            bool declarerWon = _state.GameType switch
            {
                MariasGameRules.GameType.Bettel => _state.PlayerTrickCount[_state.DeclarerIndex] == 0,
                MariasGameRules.GameType.Durch  => _state.PlayerTrickCount[_state.DeclarerIndex] == 10,
                _                               => declarerPoints >= MariasGameRules.WinningPoints
            };

            _state.WinnerIndex = declarerWon ? _state.DeclarerIndex : (int?)null;
            _state.IsGameOver  = true;
            _state.Phase       = GamePhase.GameOver;

            _gameEvents.NotifyObservers(new GameEvent
            {
                Type          = GameEventType.GameEnded,
                PlayerIndex   = _state.WinnerIndex ?? -1,
                DeclarerIndex = _state.DeclarerIndex,
                Result        = _state.Result
            });

            _scoreEvents.NotifyObservers(new ScoreEvent
            {
                Type   = ScoreEventType.RoundEnded,
                Result = _state.Result
            });
        }
    }
}
