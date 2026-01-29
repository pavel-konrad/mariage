using System.Collections.Generic;
using System.Linq;
using MariasGame.Core;
using MariasGame.Core.Interfaces;

namespace MariasGame.Game.Strategies
{
    /// <summary>
    /// Střední AI strategie pro Mariáš.
    /// Používá základní heuristiky: preferuje trumfy, hraje nízké karty, snaží se vyhrát štych.
    /// </summary>
    public class MediumAIStrategy : IAIStrategy
    {
        private static readonly System.Random Random = new System.Random();

        public Card ChooseCardToPlay(IReadOnlyList<Card> legalPlays, MariasGameState gameState)
        {
            if (legalPlays == null || legalPlays.Count == 0)
                return null;

            // Pokud je jen jedna možnost, zahraj ji
            if (legalPlays.Count == 1)
                return legalPlays[0];

            // Pokud jsme první ve štychu, zahraj nejnižší kartu (šetříme silné)
            if (gameState.CurrentTrick == null || gameState.CurrentTrick.Count == 0)
            {
                return legalPlays
                    .OrderBy(c => MariasGameRules.GetCardStrength(c.Rank))
                    .First();
            }

            // Pokud štych probíhá, zkus vyhrát co nejlevnější kartou
            var leadSuit = gameState.CurrentTrick[0].Suit;
            int currentBestStrength = gameState.CurrentTrick
                .Where(c => c.Suit == leadSuit || c.Suit == gameState.TrumpSuit)
                .Max(c => c.Suit == gameState.TrumpSuit && leadSuit != gameState.TrumpSuit
                    ? MariasGameRules.GetCardStrength(c.Rank) + 100
                    : MariasGameRules.GetCardStrength(c.Rank));

            // Najdi nejlevnější kartu, která přebije
            var winningCards = legalPlays
                .Where(c =>
                {
                    int strength = c.Suit == gameState.TrumpSuit && leadSuit != gameState.TrumpSuit
                        ? MariasGameRules.GetCardStrength(c.Rank) + 100
                        : MariasGameRules.GetCardStrength(c.Rank);
                    return c.Suit == leadSuit || c.Suit == gameState.TrumpSuit
                        ? strength > currentBestStrength
                        : false;
                })
                .OrderBy(c => MariasGameRules.GetCardStrength(c.Rank))
                .ToList();

            if (winningCards.Count > 0)
                return winningCards.First();

            // Nemůžeme vyhrát -- zahraj nejnižší kartu
            return legalPlays
                .OrderBy(c => MariasGameRules.GetCardPoints(c.Rank))
                .ThenBy(c => MariasGameRules.GetCardStrength(c.Rank))
                .First();
        }

        public MariasGameRules.BidOption ChooseBid(MariasGameState gameState)
        {
            // Střední AI: nabídne Hru pokud má hodně trumfů/es, jinak passuje
            var hand = gameState.GetCurrentPlayerHand();
            if (hand == null) return MariasGameRules.BidOption.Pass;

            int aceCount = hand.Count(c => c.Rank == CardRank.Ace);
            int tenCount = hand.Count(c => c.Rank == CardRank.Ten);

            if (aceCount >= 3 || (aceCount >= 2 && tenCount >= 2))
                return MariasGameRules.BidOption.Game;

            return MariasGameRules.BidOption.Pass;
        }

        public CardSuit ChooseTrumpSuit(IReadOnlyList<Card> hand)
        {
            if (hand == null || hand.Count == 0)
                return CardSuit.Hearts;

            // Zvol barvu, které má nejvíce karet
            return hand
                .GroupBy(c => c.Suit)
                .OrderByDescending(g => g.Count())
                .ThenByDescending(g => g.Sum(c => MariasGameRules.GetCardStrength(c.Rank)))
                .First()
                .Key;
        }

        public List<Card> ChooseCardsToDiscard(IReadOnlyList<Card> hand, MariasGameState gameState)
        {
            if (hand == null || hand.Count < 2)
                return new List<Card>();

            // Odhoď 2 nejslabší karty (preferuj karty s body -- body se přičtou)
            // Prioritně odhazuj karty bez bodové hodnoty z netrumfové barvy
            return hand
                .OrderBy(c => MariasGameRules.GetCardPoints(c.Rank))
                .ThenBy(c => MariasGameRules.GetCardStrength(c.Rank))
                .Take(2)
                .ToList();
        }
    }
}
