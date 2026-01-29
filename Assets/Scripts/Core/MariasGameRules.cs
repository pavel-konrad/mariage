using System;
using System.Collections.Generic;
using System.Linq;
using MariasGame.Core.Interfaces;

namespace MariasGame.Core
{
    /// <summary>
    /// Pravidla hry Mariáš (český mariáš).
    /// Čistá logika bez Unity závislostí.
    /// 
    /// Základní pravidla:
    /// - 32 karet (7-A ve 4 barvách)
    /// - 3 hráči
    /// - Bodované karty: Eso=11, Desítka=10, Král=4, Dáma=3, Spodek=2
    /// - Hlášky: 20 bodů za pár (Král+Dáma), 40 bodů za pár v trumfech
    /// - Sedma: +10 bodů za výhru posledního štychu se sedmou trumfovou
    /// </summary>
    public static class MariasGameRules
    {
        /// <summary>
        /// Počet hráčů v klasickém mariáši.
        /// </summary>
        public const int StandardPlayerCount = 3;
        
        /// <summary>
        /// Počet karet v balíčku.
        /// </summary>
        public const int DeckSize = 32;
        
        /// <summary>
        /// Počet karet, které se rozdávají každému hráči.
        /// </summary>
        public const int CardsPerPlayer = 10;
        
        /// <summary>
        /// Počet karet v talonu.
        /// </summary>
        public const int TalonSize = 2;
        
        /// <summary>
        /// Celkový počet bodů ve hře (bez hlášek).
        /// </summary>
        public const int TotalPoints = 90;
        
        /// <summary>
        /// Minimum bodů pro výhru normální hry (nadpoloviční většina).
        /// </summary>
        public const int WinningPoints = 46;
        
        #region Card Values
        
        /// <summary>
        /// Získá bodovou hodnotu karty pro počítání bodů.
        /// </summary>
        public static int GetCardPoints(CardRank rank)
        {
            return rank switch
            {
                CardRank.Ace => 11,
                CardRank.Ten => 10,
                CardRank.King => 4,
                CardRank.Queen => 3,
                CardRank.Jack => 2,
                CardRank.Nine => 0,
                CardRank.Eight => 0,
                CardRank.Seven => 0,
                _ => 0
            };
        }
        
        /// <summary>
        /// Získá sílu karty pro porovnání (vyšší = silnější).
        /// Pořadí: 7 < 8 < 9 < J < Q < K < 10 < A
        /// </summary>
        public static int GetCardStrength(CardRank rank)
        {
            return rank switch
            {
                CardRank.Seven => 1,
                CardRank.Eight => 2,
                CardRank.Nine => 3,
                CardRank.Jack => 4,
                CardRank.Queen => 5,
                CardRank.King => 6,
                CardRank.Ten => 7,
                CardRank.Ace => 8,
                _ => 0
            };
        }
        
        /// <summary>
        /// Získá celkový počet bodů z karet.
        /// </summary>
        public static int CalculatePoints(IEnumerable<Card> cards)
        {
            return cards.Sum(c => GetCardPoints(c.Rank));
        }
        
        #endregion
        
        #region Trick Rules
        
        /// <summary>
        /// Určí vítěze štychu.
        /// </summary>
        /// <param name="trick">Karty ve štychu (v pořadí hraní)</param>
        /// <param name="trumpSuit">Trumfová barva (null = bez trumfů)</param>
        /// <returns>Index vítěze (0-based)</returns>
        public static int DetermineTrickWinner(List<Card> trick, CardSuit? trumpSuit)
        {
            if (trick == null || trick.Count == 0)
                throw new ArgumentException("Trick cannot be empty");
            
            var leadSuit = trick[0].Suit;
            int winnerIndex = 0;
            Card winningCard = trick[0];
            
            for (int i = 1; i < trick.Count; i++)
            {
                var card = trick[i];
                
                if (BeatsCard(card, winningCard, leadSuit, trumpSuit))
                {
                    winnerIndex = i;
                    winningCard = card;
                }
            }
            
            return winnerIndex;
        }
        
        /// <summary>
        /// Zkontroluje, zda karta poráží jinou kartu.
        /// </summary>
        public static bool BeatsCard(Card attacker, Card defender, CardSuit leadSuit, CardSuit? trumpSuit)
        {
            // Trumf vždy bije ne-trumf
            if (trumpSuit.HasValue)
            {
                bool attackerIsTrump = attacker.Suit == trumpSuit.Value;
                bool defenderIsTrump = defender.Suit == trumpSuit.Value;
                
                if (attackerIsTrump && !defenderIsTrump)
                    return true;
                if (!attackerIsTrump && defenderIsTrump)
                    return false;
            }
            
            // Pokud útočník nehraje barvu ani trumf, nemůže vyhrát
            if (attacker.Suit != leadSuit && (!trumpSuit.HasValue || attacker.Suit != trumpSuit.Value))
                return false;
            
            // Obě karty jsou stejné barvy - porovnat sílu
            if (attacker.Suit == defender.Suit)
            {
                return GetCardStrength(attacker.Rank) > GetCardStrength(defender.Rank);
            }
            
            return false;
        }
        
        /// <summary>
        /// Získá karty, které může hráč legálně zahrát.
        /// </summary>
        public static List<Card> GetLegalPlays(IReadOnlyList<Card> hand, List<Card> currentTrick, CardSuit? trumpSuit)
        {
            if (currentTrick == null || currentTrick.Count == 0)
            {
                // Hráč vynáší - může hrát cokoliv
                return hand.ToList();
            }
            
            var leadSuit = currentTrick[0].Suit;
            var legalPlays = new List<Card>();
            
            // Musíme přiznat barvu
            var suitCards = hand.Where(c => c.Suit == leadSuit).ToList();
            if (suitCards.Count > 0)
            {
                // Máme barvu - můžeme hrát pouze karty této barvy
                // Navíc musíme přebít, pokud můžeme
                var currentWinner = GetCurrentTrickWinner(currentTrick, trumpSuit);
                var winningCard = currentTrick[currentWinner];
                
                var beatingCards = suitCards
                    .Where(c => BeatsCard(c, winningCard, leadSuit, trumpSuit))
                    .ToList();
                
                if (beatingCards.Count > 0)
                {
                    return beatingCards;
                }
                
                return suitCards;
            }
            
            // Nemáme barvu - musíme trumfnout, pokud máme trumfy
            if (trumpSuit.HasValue)
            {
                var trumpCards = hand.Where(c => c.Suit == trumpSuit.Value).ToList();
                if (trumpCards.Count > 0)
                {
                    // Máme trumfy - musíme trumfnout
                    // A pokud můžeme přebít stávající trumf, musíme
                    var currentWinner = GetCurrentTrickWinner(currentTrick, trumpSuit);
                    var winningCard = currentTrick[currentWinner];
                    
                    if (winningCard.Suit == trumpSuit.Value)
                    {
                        // Stávající vítěz je trumf - musíme přebít vyšším trumfem
                        var higherTrumps = trumpCards
                            .Where(c => GetCardStrength(c.Rank) > GetCardStrength(winningCard.Rank))
                            .ToList();
                        
                        if (higherTrumps.Count > 0)
                            return higherTrumps;
                    }
                    
                    return trumpCards;
                }
            }
            
            // Nemáme barvu ani trumfy - můžeme hrát cokoliv
            return hand.ToList();
        }
        
        /// <summary>
        /// Získá aktuálního vítěze štychu (během hraní).
        /// </summary>
        private static int GetCurrentTrickWinner(List<Card> trick, CardSuit? trumpSuit)
        {
            if (trick.Count == 0) return 0;
            return DetermineTrickWinner(trick, trumpSuit);
        }
        
        #endregion
        
        #region Marriage (Hláška)
        
        /// <summary>
        /// Zkontroluje, zda hráč může hlásit (má pár Král+Dáma).
        /// Hlásit lze pouze při vynášení a musí vynést jednu z karet páru.
        /// </summary>
        public static bool CanDeclareMarriage(IReadOnlyList<Card> hand, CardSuit suit)
        {
            return hand.Any(c => c.Suit == suit && c.Rank == CardRank.King) &&
                   hand.Any(c => c.Suit == suit && c.Rank == CardRank.Queen);
        }
        
        /// <summary>
        /// Získá všechny možné hlášky v ruce.
        /// </summary>
        public static List<CardSuit> GetPossibleMarriages(IReadOnlyList<Card> hand)
        {
            var marriages = new List<CardSuit>();
            
            foreach (CardSuit suit in Enum.GetValues(typeof(CardSuit)))
            {
                if (CanDeclareMarriage(hand, suit))
                {
                    marriages.Add(suit);
                }
            }
            
            return marriages;
        }
        
        /// <summary>
        /// Získá hodnotu hlášky.
        /// </summary>
        public static int GetMarriageValue(CardSuit marriageSuit, CardSuit? trumpSuit)
        {
            if (trumpSuit.HasValue && marriageSuit == trumpSuit.Value)
            {
                return 40; // Trumfová hláška
            }
            return 20; // Normální hláška
        }
        
        #endregion
        
        #region Game Types
        
        /// <summary>
        /// Typ hry v mariáši.
        /// </summary>
        public enum GameType
        {
            /// <summary>Normální hra s trumfy - cíl: nasbírat více bodů.</summary>
            Normal,
            
            /// <summary>Sedma - musí vyhrát poslední štych sedmou trumfovou.</summary>
            Seven,
            
            /// <summary>Sedma proti - obrana proti sedmě.</summary>
            SevenAgainst,
            
            /// <summary>Stovka - musí nasbírat 100+ bodů.</summary>
            Hundred,
            
            /// <summary>Stovka proti - obrana proti stovce.</summary>
            HundredAgainst,
            
            /// <summary>Betl - nesmí vzít žádný štych (bez trumfů).</summary>
            Bettel,
            
            /// <summary>Durch - musí vzít všechny štychy (bez trumfů).</summary>
            Durch
        }
        
        /// <summary>
        /// Získá základní hodnotu hry (v korunách/bodech).
        /// </summary>
        public static int GetGameBaseValue(GameType gameType)
        {
            return gameType switch
            {
                GameType.Normal => 1,
                GameType.Seven => 2,
                GameType.SevenAgainst => 2,
                GameType.Hundred => 2,
                GameType.HundredAgainst => 2,
                GameType.Bettel => 5,
                GameType.Durch => 10,
                _ => 1
            };
        }
        
        /// <summary>
        /// Zkontroluje, zda hra používá trumfy.
        /// </summary>
        public static bool GameHasTrumps(GameType gameType)
        {
            return gameType switch
            {
                GameType.Bettel => false,
                GameType.Durch => false,
                _ => true
            };
        }
        
        #endregion
        
        #region Bidding
        
        /// <summary>
        /// Možné volby při dražbě.
        /// </summary>
        public enum BidOption
        {
            /// <summary>Pass - hráč nechce hrát.</summary>
            Pass,
            
            /// <summary>Hra - normální hra.</summary>
            Game,
            
            /// <summary>Sedma - hra na sedmu.</summary>
            Seven,
            
            /// <summary>Stovka - hra na 100+ bodů.</summary>
            Hundred,
            
            /// <summary>Betl - hra bez braní.</summary>
            Bettel,
            
            /// <summary>Durch - hra na všechny štychy.</summary>
            Durch,
            
            /// <summary>Flék - zdvojení sázky.</summary>
            Double,
            
            /// <summary>Re - zdvojení fléku.</summary>
            Redouble,
            
            /// <summary>Tutti/Boty - další zdvojení.</summary>
            Tutti
        }
        
        /// <summary>
        /// Zkontroluje, zda je dražba platná.
        /// </summary>
        public static bool IsValidBid(BidOption currentBid, BidOption newBid)
        {
            // Jednoduchá validace - v reálu by byla složitější
            if (newBid == BidOption.Pass)
                return true;
            
            return (int)newBid > (int)currentBid;
        }
        
        #endregion
        
        #region Seven (Sedma)
        
        /// <summary>
        /// Zkontroluje, zda hráč může hrát sedmu (má trumfovou sedmu).
        /// </summary>
        public static bool HasTrumpSeven(IReadOnlyList<Card> hand, CardSuit trumpSuit)
        {
            return hand.Any(c => c.Suit == trumpSuit && c.Rank == CardRank.Seven);
        }
        
        /// <summary>
        /// Zkontroluje, zda hráč vyhrál sedmu (vyhrál poslední štych se sedmou trumfovou).
        /// </summary>
        public static bool WonSevenWithTrumpSeven(Card lastTrickWinningCard, CardSuit trumpSuit, int trickWinnerIndex, int sevenPlayerIndex)
        {
            if (trickWinnerIndex != sevenPlayerIndex)
                return false;
            
            return lastTrickWinningCard.Suit == trumpSuit && lastTrickWinningCard.Rank == CardRank.Seven;
        }
        
        #endregion
        
        #region Scoring
        
        /// <summary>
        /// Výsledek hry.
        /// </summary>
        public class GameResult
        {
            public int[] PlayerPoints { get; set; }
            public int[] DeclaredMarriages { get; set; }
            public bool SevenWon { get; set; }
            public bool SevenLost { get; set; }
            public GameType GameType { get; set; }
            public int Multiplier { get; set; } = 1; // Fléky
            
            public int GetTotalScore(int playerIndex, int declarer)
            {
                // Základní implementace - v reálu by byla složitější
                int baseScore = GetGameBaseValue(GameType) * Multiplier;
                
                if (playerIndex == declarer)
                {
                    // Hráč hrál
                    int points = PlayerPoints[playerIndex] + DeclaredMarriages[playerIndex];
                    
                    switch (GameType)
                    {
                        case GameType.Normal:
                            return points >= WinningPoints ? baseScore : -baseScore;
                            
                        case GameType.Bettel:
                            // Betl vyhrán pokud nevzal žádný štych (0 bodů)
                            return PlayerPoints[playerIndex] == 0 ? baseScore : -baseScore;
                            
                        case GameType.Durch:
                            // Durch vyhrán pokud vzal všechny štychy (90 bodů)
                            return PlayerPoints[playerIndex] == TotalPoints ? baseScore : -baseScore;
                            
                        default:
                            return points >= WinningPoints ? baseScore : -baseScore;
                    }
                }
                else
                {
                    // Hráč bránil
                    return 0; // Obránci se počítají společně
                }
            }
        }
        
        /// <summary>
        /// Vypočítá konečné skóre hry.
        /// </summary>
        public static GameResult CalculateGameResult(
            int[] playerTrickPoints,
            int[] playerMarriagePoints,
            GameType gameType,
            int declarerIndex,
            bool sevenWon,
            bool sevenLost,
            int multiplier)
        {
            return new GameResult
            {
                PlayerPoints = playerTrickPoints,
                DeclaredMarriages = playerMarriagePoints,
                GameType = gameType,
                SevenWon = sevenWon,
                SevenLost = sevenLost,
                Multiplier = multiplier
            };
        }
        
        #endregion
        
        #region Validation
        
        /// <summary>
        /// Zkontroluje, zda je karta platná v kontextu hry.
        /// </summary>
        public static bool IsCardValid(Card card)
        {
            return card != null &&
                   Enum.IsDefined(typeof(CardSuit), card.Suit) &&
                   Enum.IsDefined(typeof(CardRank), card.Rank);
        }
        
        /// <summary>
        /// Zkontroluje, zda má balíček správný počet karet.
        /// </summary>
        public static bool IsDeckValid(IReadOnlyList<Card> deck)
        {
            if (deck == null || deck.Count != DeckSize)
                return false;
            
            // Zkontrolovat, že jsou všechny karty unikátní
            var uniqueCards = new HashSet<(CardSuit, CardRank)>();
            foreach (var card in deck)
            {
                if (!uniqueCards.Add((card.Suit, card.Rank)))
                    return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Zkontroluje, zda jsou všechny karty rozdány správně.
        /// </summary>
        public static bool AreCardsDealtCorrectly(
            IReadOnlyList<Card> player1Hand,
            IReadOnlyList<Card> player2Hand,
            IReadOnlyList<Card> player3Hand,
            IReadOnlyList<Card> talon)
        {
            if (player1Hand.Count != CardsPerPlayer ||
                player2Hand.Count != CardsPerPlayer ||
                player3Hand.Count != CardsPerPlayer ||
                talon.Count != TalonSize)
            {
                return false;
            }
            
            var allCards = player1Hand
                .Concat(player2Hand)
                .Concat(player3Hand)
                .Concat(talon)
                .ToList();
            
            return IsDeckValid(allCards);
        }
        
        #endregion
    }
}
