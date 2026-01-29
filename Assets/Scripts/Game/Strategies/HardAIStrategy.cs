using System.Collections.Generic;
using System.Linq;
using MariasGame.Core;
using MariasGame.Core.Interfaces;

namespace MariasGame.Game.Strategies
{
    /// <summary>
    /// Pokročilá AI strategie pro Mariáš.
    /// Počítá odehrané karty, sleduje trumfy, volí optimální tahy.
    /// </summary>
    public class HardAIStrategy : IAIStrategy
    {
        private static readonly System.Random Random = new System.Random();

        public Card ChooseCardToPlay(IReadOnlyList<Card> legalPlays, MariasGameState gameState)
        {
            if (legalPlays == null || legalPlays.Count == 0)
                return null;

            if (legalPlays.Count == 1)
                return legalPlays[0];

            // === Vynášíme (jsme první ve štychu) ===
            if (gameState.CurrentTrick == null || gameState.CurrentTrick.Count == 0)
            {
                return ChooseLeadCard(legalPlays, gameState);
            }

            // === Odpovídáme ===
            return ChooseFollowCard(legalPlays, gameState);
        }

        private Card ChooseLeadCard(IReadOnlyList<Card> legalPlays, MariasGameState gameState)
        {
            // Pokud máme eso, vytáhni ho (sbíráme body)
            var aces = legalPlays.Where(c => c.Rank == CardRank.Ace).ToList();
            if (aces.Count > 0)
            {
                // Preferuj netrumfové eso (trumfové eso si schováme)
                var nonTrumpAce = aces.FirstOrDefault(c => c.Suit != gameState.TrumpSuit);
                return nonTrumpAce ?? aces.First();
            }

            // Pokud máme desítku s esem odehraným, je to bezpečný vynos
            var safeTens = legalPlays
                .Where(c => c.Rank == CardRank.Ten && IsAcePlayed(c.Suit, gameState))
                .ToList();
            if (safeTens.Count > 0)
                return safeTens.First();

            // Jinak vynášej nízké karty z krátkých barev (abychom se zbavili krátkých)
            var suitCounts = legalPlays.GroupBy(c => c.Suit).ToDictionary(g => g.Key, g => g.Count());
            return legalPlays
                .OrderBy(c => suitCounts.GetValueOrDefault(c.Suit, 0))
                .ThenBy(c => MariasGameRules.GetCardPoints(c.Rank))
                .ThenBy(c => MariasGameRules.GetCardStrength(c.Rank))
                .First();
        }

        private Card ChooseFollowCard(IReadOnlyList<Card> legalPlays, MariasGameState gameState)
        {
            var leadCard = gameState.CurrentTrick[0];
            var leadSuit = leadCard.Suit;

            // Spočítej body ve štychu
            int trickPoints = gameState.CurrentTrick.Sum(c => MariasGameRules.GetCardPoints(c.Rank));

            // Určení aktuálně nejvyšší karty ve štychu
            int bestStrength = 0;
            foreach (var c in gameState.CurrentTrick)
            {
                int s = c.Suit == gameState.TrumpSuit && leadSuit != gameState.TrumpSuit
                    ? MariasGameRules.GetCardStrength(c.Rank) + 100
                    : c.Suit == leadSuit
                        ? MariasGameRules.GetCardStrength(c.Rank)
                        : 0;
                if (s > bestStrength) bestStrength = s;
            }

            // Najdi karty, které přebijí
            var winners = legalPlays.Where(c =>
            {
                int s = c.Suit == gameState.TrumpSuit && leadSuit != gameState.TrumpSuit
                    ? MariasGameRules.GetCardStrength(c.Rank) + 100
                    : c.Suit == leadSuit
                        ? MariasGameRules.GetCardStrength(c.Rank)
                        : 0;
                return s > bestStrength;
            }).ToList();

            // Pokud je ve štychu hodně bodů a můžeme vyhrát, zahraj nejlevnější vítěznou kartu
            if (winners.Count > 0 && trickPoints >= 10)
            {
                return winners
                    .OrderBy(c => MariasGameRules.GetCardStrength(c.Rank))
                    .First();
            }

            // Pokud můžeme vyhrát obecně, zahraj nejlevnější vítěznou
            if (winners.Count > 0)
            {
                return winners
                    .OrderBy(c => MariasGameRules.GetCardPoints(c.Rank))
                    .ThenBy(c => MariasGameRules.GetCardStrength(c.Rank))
                    .First();
            }

            // Nemůžeme vyhrát -- odhoď nejméně hodnotnou kartu
            return legalPlays
                .OrderBy(c => MariasGameRules.GetCardPoints(c.Rank))
                .ThenBy(c => MariasGameRules.GetCardStrength(c.Rank))
                .First();
        }

        private bool IsAcePlayed(CardSuit suit, MariasGameState gameState)
        {
            if (gameState.TrickHistory == null) return false;
            return gameState.TrickHistory
                .SelectMany(t => t.Cards)
                .Any(c => c.Suit == suit && c.Rank == CardRank.Ace);
        }

        public MariasGameRules.BidOption ChooseBid(MariasGameState gameState)
        {
            var hand = gameState.GetCurrentPlayerHand();
            if (hand == null) return MariasGameRules.BidOption.Pass;

            int aceCount = hand.Count(c => c.Rank == CardRank.Ace);
            int tenCount = hand.Count(c => c.Rank == CardRank.Ten);
            int totalHighCards = aceCount + tenCount;

            // Najdi nejsilnější barvu
            var strongestSuit = hand
                .GroupBy(c => c.Suit)
                .OrderByDescending(g => g.Count())
                .ThenByDescending(g => g.Sum(c => MariasGameRules.GetCardStrength(c.Rank)))
                .First();

            int strongestSuitCount = strongestSuit.Count();
            bool hasSuitAce = strongestSuit.Any(c => c.Rank == CardRank.Ace);
            bool hasSuitTen = strongestSuit.Any(c => c.Rank == CardRank.Ten);

            // Stovka: 4+ karet v jedné barvě s esem a desítkou, plus další esa
            if (strongestSuitCount >= 4 && hasSuitAce && hasSuitTen && aceCount >= 3)
                return MariasGameRules.BidOption.Hundred;

            // Sedma: pokud máme sedmu v nejsilnější barvě + hodně trumfů
            bool hasSeven = strongestSuit.Any(c => c.Rank == CardRank.Seven);
            if (hasSeven && strongestSuitCount >= 5 && hasSuitAce)
                return MariasGameRules.BidOption.Seven;

            // Hra: dostatečně silná ruka
            if (totalHighCards >= 4 || (aceCount >= 3 && strongestSuitCount >= 4))
                return MariasGameRules.BidOption.Game;

            // Betl: žádná esa, žádné desítky, hodně nízkých karet
            int lowCardCount = hand.Count(c => c.Rank <= CardRank.Nine);
            if (aceCount == 0 && tenCount == 0 && lowCardCount >= 7)
                return MariasGameRules.BidOption.Bettel;

            return MariasGameRules.BidOption.Pass;
        }

        public CardSuit ChooseTrumpSuit(IReadOnlyList<Card> hand)
        {
            if (hand == null || hand.Count == 0)
                return CardSuit.Hearts;

            // Zvol barvu s nejvyšší celkovou silou (počet karet + síla)
            return hand
                .GroupBy(c => c.Suit)
                .OrderByDescending(g => g.Count() * 10 + g.Sum(c => MariasGameRules.GetCardStrength(c.Rank)))
                .First()
                .Key;
        }

        public List<Card> ChooseCardsToDiscard(IReadOnlyList<Card> hand, MariasGameState gameState)
        {
            if (hand == null || hand.Count < 2)
                return new List<Card>();

            // Odhoď karty s body z krátkých netrumfových barev (body se přičtou forhontovi)
            // Ale preferuj odhazovat karty s body (10, K, Q, J)
            var trumpSuit = gameState.TrumpSuit;

            var candidates = hand
                .Where(c => c.Suit != trumpSuit) // Neodhadujeme trumfy
                .Where(c => c.Rank != CardRank.Ace) // Esa si necháme
                .OrderByDescending(c => MariasGameRules.GetCardPoints(c.Rank)) // Preferuj body do talonu
                .ThenBy(c => MariasGameRules.GetCardStrength(c.Rank))
                .ToList();

            // Pokud nemáme dost netrumfových, přidej i trumfy
            if (candidates.Count < 2)
            {
                var remaining = hand
                    .Except(candidates)
                    .OrderBy(c => MariasGameRules.GetCardStrength(c.Rank))
                    .ToList();
                candidates.AddRange(remaining);
            }

            return candidates.Take(2).ToList();
        }
    }
}
