using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MariasGame.Core
{
    /// <summary>
    /// Hlavní herní kontrolér pro Mariáš.
    /// Řídí průběh hry a vyvolává eventy pro UI/Audio/VFX.
    /// </summary>
    public class MariasGameController
    {
        private MariasGameState _state;
        private Deck _deck;
        
        #region Events
        
        // Herní eventy pro designéry (Audio, VFX, UI)
        public event Action OnGameStarted;
        public event Action<int, List<Card>> OnCardsDealt; // playerIndex, cards
        public event Action<int, Card> OnCardPlayed; // playerIndex, card
        public event Action<int, List<Card>> OnTrickWon; // winnerIndex, cards
        public event Action<CardSuit, int> OnMarriageDeclared; // suit, points
        public event Action<MariasGameRules.GameType> OnGameTypeDeclared;
        public event Action<CardSuit> OnTrumpDeclared;
        public event Action<int> OnBetDoubled; // newMultiplier
        public event Action<int, MariasGameRules.GameResult> OnGameEnded; // winnerIndex, result
        public event Action<int> OnPlayerTurnStarted;
        public event Action<int, int> OnScoreChanged; // playerIndex, newScore
        public event Action OnSevenDeclared;
        public event Action<bool> OnSevenResolved; // won
        public event Action OnHundredDeclared;
        public event Action<bool> OnHundredResolved; // won
        public event Action OnBettelDeclared;
        public event Action OnDurchDeclared;
        
        // UI eventy
        public event Action<GamePhase> OnPhaseChanged;
        public event Action<List<Card>> OnLegalPlaysCalculated;
        
        #endregion
        
        public MariasGameState State => _state;
        
        /// <summary>
        /// Vytvoří novou hru.
        /// </summary>
        public void StartNewGame(List<string> playerNames)
        {
            _state = MariasGameState.CreateNew(playerNames);
            _deck = new Deck();
            
            OnGameStarted?.Invoke();
            
            // Zamíchat a rozdat karty
            ShuffleAndDeal();
        }
        
        /// <summary>
        /// Zamíchá balíček a rozdá karty.
        /// </summary>
        private void ShuffleAndDeal()
        {
            _deck.Shuffle();
            _state.Phase = GamePhase.Dealing;
            OnPhaseChanged?.Invoke(_state.Phase);
            
            // Rozdat každému hráči 10 karet
            for (int i = 0; i < 3; i++)
            {
                var cards = new List<Card>();
                for (int j = 0; j < MariasGameRules.CardsPerPlayer; j++)
                {
                    cards.Add(_deck.DrawCard());
                }
                _state.PlayerHands[i] = cards;
                OnCardsDealt?.Invoke(i, cards);
            }
            
            // Zbylé 2 karty do talonu
            _state.Talon.Clear();
            _state.Talon.Add(_deck.DrawCard());
            _state.Talon.Add(_deck.DrawCard());
            
            // Přejít na dražbu
            StartBidding();
        }
        
        /// <summary>
        /// Začne fázi dražby.
        /// </summary>
        private void StartBidding()
        {
            _state.Phase = GamePhase.Bidding;
            _state.CurrentPlayerIndex = 0; // Forhont začíná
            OnPhaseChanged?.Invoke(_state.Phase);
            OnPlayerTurnStarted?.Invoke(_state.CurrentPlayerIndex);
        }
        
        /// <summary>
        /// Hráč učiní nabídku v dražbě.
        /// </summary>
        public void MakeBid(int playerIndex, MariasGameRules.BidOption bid)
        {
            if (_state.Phase != GamePhase.Bidding)
            {
                Debug.LogWarning("[MariasGameController] Not in bidding phase!");
                return;
            }
            
            if (playerIndex != _state.CurrentPlayerIndex)
            {
                Debug.LogWarning("[MariasGameController] Not this player's turn!");
                return;
            }
            
            // Zpracovat nabídku
            switch (bid)
            {
                case MariasGameRules.BidOption.Game:
                    _state.GameType = MariasGameRules.GameType.Normal;
                    _state.DeclarerIndex = playerIndex;
                    OnGameTypeDeclared?.Invoke(_state.GameType);
                    StartTakingTalon();
                    break;
                    
                case MariasGameRules.BidOption.Seven:
                    _state.GameType = MariasGameRules.GameType.Seven;
                    _state.DeclarerIndex = playerIndex;
                    _state.SevenDeclared = true;
                    OnSevenDeclared?.Invoke();
                    OnGameTypeDeclared?.Invoke(_state.GameType);
                    StartTakingTalon();
                    break;
                    
                case MariasGameRules.BidOption.Hundred:
                    _state.GameType = MariasGameRules.GameType.Hundred;
                    _state.DeclarerIndex = playerIndex;
                    _state.HundredDeclared = true;
                    OnHundredDeclared?.Invoke();
                    OnGameTypeDeclared?.Invoke(_state.GameType);
                    StartTakingTalon();
                    break;
                    
                case MariasGameRules.BidOption.Bettel:
                    _state.GameType = MariasGameRules.GameType.Bettel;
                    _state.DeclarerIndex = playerIndex;
                    _state.TrumpSuit = null;
                    OnBettelDeclared?.Invoke();
                    OnGameTypeDeclared?.Invoke(_state.GameType);
                    StartTakingTalon();
                    break;
                    
                case MariasGameRules.BidOption.Durch:
                    _state.GameType = MariasGameRules.GameType.Durch;
                    _state.DeclarerIndex = playerIndex;
                    _state.TrumpSuit = null;
                    OnDurchDeclared?.Invoke();
                    OnGameTypeDeclared?.Invoke(_state.GameType);
                    StartTakingTalon();
                    break;
                    
                case MariasGameRules.BidOption.Double:
                    _state.BetMultiplier *= 2;
                    OnBetDoubled?.Invoke(_state.BetMultiplier);
                    _state.NextPlayer();
                    OnPlayerTurnStarted?.Invoke(_state.CurrentPlayerIndex);
                    break;
                    
                case MariasGameRules.BidOption.Pass:
                    _state.NextPlayer();
                    OnPlayerTurnStarted?.Invoke(_state.CurrentPlayerIndex);
                    break;
            }
        }
        
        /// <summary>
        /// Začne fázi braní talonu.
        /// </summary>
        private void StartTakingTalon()
        {
            _state.Phase = GamePhase.TakingTalon;
            _state.CurrentPlayerIndex = _state.DeclarerIndex;
            OnPhaseChanged?.Invoke(_state.Phase);
            
            // Přidat talon do ruky forhonta
            _state.PlayerHands[_state.DeclarerIndex].AddRange(_state.Talon);
            OnCardsDealt?.Invoke(_state.DeclarerIndex, _state.Talon);
            
            // Přejít na odhazování
            _state.Phase = GamePhase.DiscardingTalon;
            OnPhaseChanged?.Invoke(_state.Phase);
            OnPlayerTurnStarted?.Invoke(_state.CurrentPlayerIndex);
        }
        
        /// <summary>
        /// Forhont odhodí 2 karty do talonu.
        /// </summary>
        public void DiscardToTalon(List<Card> cardsToDiscard)
        {
            if (_state.Phase != GamePhase.DiscardingTalon)
            {
                Debug.LogWarning("[MariasGameController] Not in discarding phase!");
                return;
            }
            
            if (cardsToDiscard.Count != 2)
            {
                Debug.LogWarning("[MariasGameController] Must discard exactly 2 cards!");
                return;
            }
            
            var hand = _state.PlayerHands[_state.DeclarerIndex];
            foreach (var card in cardsToDiscard)
            {
                if (!hand.Contains(card))
                {
                    Debug.LogWarning($"[MariasGameController] Card {card} not in hand!");
                    return;
                }
            }
            
            // Odebrat karty z ruky
            foreach (var card in cardsToDiscard)
            {
                hand.Remove(card);
            }
            
            // Uložit odhozené karty (body se přičtou forhontovi)
            _state.DiscardedTalon = cardsToDiscard;
            _state.PlayerTrickPoints[_state.DeclarerIndex] += MariasGameRules.CalculatePoints(cardsToDiscard);
            
            // Přejít na volbu trumfů
            StartDeclaring();
        }
        
        /// <summary>
        /// Začne fázi hlášení.
        /// </summary>
        private void StartDeclaring()
        {
            _state.Phase = GamePhase.Declaring;
            OnPhaseChanged?.Invoke(_state.Phase);
            OnPlayerTurnStarted?.Invoke(_state.DeclarerIndex);
        }
        
        /// <summary>
        /// Forhont zvolí trumfovou barvu.
        /// </summary>
        public void DeclareTrump(CardSuit trumpSuit)
        {
            if (_state.Phase != GamePhase.Declaring)
            {
                Debug.LogWarning("[MariasGameController] Not in declaring phase!");
                return;
            }
            
            if (!MariasGameRules.GameHasTrumps(_state.GameType))
            {
                Debug.LogWarning("[MariasGameController] This game type has no trumps!");
                return;
            }
            
            _state.TrumpSuit = trumpSuit;
            OnTrumpDeclared?.Invoke(trumpSuit);
            
            // Začít hraní
            StartPlaying();
        }
        
        /// <summary>
        /// Hlásí pár (Král + Dáma).
        /// </summary>
        public void DeclareMarriage(CardSuit suit)
        {
            if (_state.Phase != GamePhase.Playing)
            {
                Debug.LogWarning("[MariasGameController] Can only declare marriage during play!");
                return;
            }
            
            // Lze hlásit pouze při vynášení
            if (_state.CurrentTrick.Count > 0)
            {
                Debug.LogWarning("[MariasGameController] Can only declare marriage when leading!");
                return;
            }
            
            var hand = _state.GetCurrentPlayerHand();
            if (!MariasGameRules.CanDeclareMarriage(hand, suit))
            {
                Debug.LogWarning("[MariasGameController] Player doesn't have this marriage!");
                return;
            }
            
            if (_state.DeclaredMarriages.Contains(suit))
            {
                Debug.LogWarning("[MariasGameController] This marriage was already declared!");
                return;
            }
            
            _state.DeclaredMarriages.Add(suit);
            int points = MariasGameRules.GetMarriageValue(suit, _state.TrumpSuit);
            _state.PlayerMarriagePoints[_state.CurrentPlayerIndex] += points;
            
            OnMarriageDeclared?.Invoke(suit, points);
            OnScoreChanged?.Invoke(_state.CurrentPlayerIndex, 
                _state.PlayerTrickPoints[_state.CurrentPlayerIndex] + _state.PlayerMarriagePoints[_state.CurrentPlayerIndex]);
        }
        
        /// <summary>
        /// Začne fázi hraní.
        /// </summary>
        private void StartPlaying()
        {
            _state.Phase = GamePhase.Playing;
            _state.TrickNumber = 1;
            _state.CurrentPlayerIndex = _state.DeclarerIndex; // Forhont vynáší první
            _state.TrickLeaderIndex = _state.DeclarerIndex;
            _state.CurrentTrick.Clear();
            
            OnPhaseChanged?.Invoke(_state.Phase);
            OnPlayerTurnStarted?.Invoke(_state.CurrentPlayerIndex);
            
            // Vypočítat legální tahy
            var legalPlays = MariasGameRules.GetLegalPlays(
                _state.GetCurrentPlayerHand(),
                _state.CurrentTrick,
                _state.TrumpSuit);
            OnLegalPlaysCalculated?.Invoke(legalPlays);
        }
        
        /// <summary>
        /// Zahraje kartu.
        /// </summary>
        public bool PlayCard(int playerIndex, Card card)
        {
            if (_state.Phase != GamePhase.Playing)
            {
                Debug.LogWarning("[MariasGameController] Not in playing phase!");
                return false;
            }
            
            if (playerIndex != _state.CurrentPlayerIndex)
            {
                Debug.LogWarning("[MariasGameController] Not this player's turn!");
                return false;
            }
            
            var hand = _state.PlayerHands[playerIndex];
            if (!hand.Contains(card))
            {
                Debug.LogWarning($"[MariasGameController] Card {card} not in hand!");
                return false;
            }
            
            // Zkontrolovat legálnost tahu
            var legalPlays = MariasGameRules.GetLegalPlays(hand, _state.CurrentTrick, _state.TrumpSuit);
            if (!legalPlays.Contains(card))
            {
                Debug.LogWarning($"[MariasGameController] Card {card} is not a legal play!");
                return false;
            }
            
            // Zahrát kartu
            hand.Remove(card);
            _state.CurrentTrick.Add(card);
            OnCardPlayed?.Invoke(playerIndex, card);
            
            // Zkontrolovat konec štychu
            if (_state.IsTrickComplete())
            {
                ResolveTrick();
            }
            else
            {
                _state.NextPlayer();
                OnPlayerTurnStarted?.Invoke(_state.CurrentPlayerIndex);
                
                var newLegalPlays = MariasGameRules.GetLegalPlays(
                    _state.GetCurrentPlayerHand(),
                    _state.CurrentTrick,
                    _state.TrumpSuit);
                OnLegalPlaysCalculated?.Invoke(newLegalPlays);
            }
            
            return true;
        }
        
        /// <summary>
        /// Vyhodnotí štych.
        /// </summary>
        private void ResolveTrick()
        {
            // Určit vítěze
            int relativeWinner = MariasGameRules.DetermineTrickWinner(_state.CurrentTrick, _state.TrumpSuit);
            int absoluteWinner = (_state.TrickLeaderIndex + relativeWinner) % 3;
            
            // Přičíst body
            int points = MariasGameRules.CalculatePoints(_state.CurrentTrick);
            _state.PlayerTrickPoints[absoluteWinner] += points;
            _state.PlayerTrickCount[absoluteWinner]++;
            
            // Uložit historii
            _state.TrickHistory.Add(new TrickHistory
            {
                TrickNumber = _state.TrickNumber,
                Cards = new List<Card>(_state.CurrentTrick),
                WinnerIndex = absoluteWinner,
                Points = points
            });
            
            OnTrickWon?.Invoke(absoluteWinner, _state.CurrentTrick.ToList());
            OnScoreChanged?.Invoke(absoluteWinner, 
                _state.PlayerTrickPoints[absoluteWinner] + _state.PlayerMarriagePoints[absoluteWinner]);
            
            // Zkontrolovat konec hry
            if (_state.IsLastTrick())
            {
                EndGame();
            }
            else
            {
                // Připravit další štych
                _state.TrickNumber++;
                _state.CurrentTrick.Clear();
                _state.TrickLeaderIndex = absoluteWinner;
                _state.CurrentPlayerIndex = absoluteWinner;
                
                OnPlayerTurnStarted?.Invoke(_state.CurrentPlayerIndex);
                
                var legalPlays = MariasGameRules.GetLegalPlays(
                    _state.GetCurrentPlayerHand(),
                    _state.CurrentTrick,
                    _state.TrumpSuit);
                OnLegalPlaysCalculated?.Invoke(legalPlays);
            }
        }
        
        /// <summary>
        /// Ukončí hru a spočítá výsledek.
        /// </summary>
        private void EndGame()
        {
            _state.Phase = GamePhase.Scoring;
            OnPhaseChanged?.Invoke(_state.Phase);
            
            // Spočítat výsledek
            bool sevenWon = false;
            bool sevenLost = false;
            
            // Zkontrolovat sedmu (pokud byla hlášena)
            if (_state.SevenDeclared && _state.TrickHistory.Count > 0)
            {
                var lastTrick = _state.TrickHistory.Last();
                var winningCard = lastTrick.Cards[MariasGameRules.DetermineTrickWinner(lastTrick.Cards, _state.TrumpSuit)];
                
                if (_state.TrumpSuit.HasValue)
                {
                    sevenWon = MariasGameRules.WonSevenWithTrumpSeven(
                        winningCard, 
                        _state.TrumpSuit.Value, 
                        lastTrick.WinnerIndex, 
                        _state.DeclarerIndex);
                    sevenLost = !sevenWon;
                }
                
                OnSevenResolved?.Invoke(sevenWon);
            }
            
            // Zkontrolovat stovku
            if (_state.HundredDeclared)
            {
                int totalPoints = _state.PlayerTrickPoints[_state.DeclarerIndex] + 
                                  _state.PlayerMarriagePoints[_state.DeclarerIndex];
                OnHundredResolved?.Invoke(totalPoints >= 100);
            }
            
            _state.Result = MariasGameRules.CalculateGameResult(
                _state.PlayerTrickPoints,
                _state.PlayerMarriagePoints,
                _state.GameType,
                _state.DeclarerIndex,
                sevenWon,
                sevenLost,
                _state.BetMultiplier);
            
            // Určit vítěze
            int declarerPoints = _state.PlayerTrickPoints[_state.DeclarerIndex] + 
                                 _state.PlayerMarriagePoints[_state.DeclarerIndex];
            
            bool declarerWon = _state.GameType switch
            {
                MariasGameRules.GameType.Bettel => _state.PlayerTrickCount[_state.DeclarerIndex] == 0,
                MariasGameRules.GameType.Durch => _state.PlayerTrickCount[_state.DeclarerIndex] == 10,
                _ => declarerPoints >= MariasGameRules.WinningPoints
            };
            
            _state.WinnerIndex = declarerWon ? _state.DeclarerIndex : (int?)null;
            _state.IsGameOver = true;
            _state.Phase = GamePhase.GameOver;
            
            OnPhaseChanged?.Invoke(_state.Phase);
            OnGameEnded?.Invoke(_state.WinnerIndex ?? -1, _state.Result);
        }
        
        /// <summary>
        /// Získá legální tahy pro aktuálního hráče.
        /// </summary>
        public List<Card> GetLegalPlays()
        {
            if (_state.Phase != GamePhase.Playing)
                return new List<Card>();
            
            return MariasGameRules.GetLegalPlays(
                _state.GetCurrentPlayerHand(),
                _state.CurrentTrick,
                _state.TrumpSuit);
        }
        
        /// <summary>
        /// Získá možné hlášky pro aktuálního hráče.
        /// </summary>
        public List<CardSuit> GetPossibleMarriages()
        {
            if (_state.Phase != GamePhase.Playing || _state.CurrentTrick.Count > 0)
                return new List<CardSuit>();
            
            return MariasGameRules.GetPossibleMarriages(_state.GetCurrentPlayerHand())
                .Where(s => !_state.DeclaredMarriages.Contains(s))
                .ToList();
        }
    }
}
