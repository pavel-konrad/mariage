using System.Collections.Generic;
using System.Linq;
using MariasGame.Core;
using MariasGame.Core.Interfaces;

namespace MariasGame.Game.Strategies
{
    /// <summary>
    /// Jednoduchá AI strategie pro Mariáš -- náhodný výběr z legálních tahů.
    /// </summary>
    public class EasyAIStrategy : IAIStrategy
    {
        private static readonly System.Random Random = new System.Random();

        public Card ChooseCardToPlay(IReadOnlyList<Card> legalPlays, MariasGameState gameState)
        {
            if (legalPlays == null || legalPlays.Count == 0)
                return null;

            return legalPlays[Random.Next(legalPlays.Count)];
        }

        public MariasGameRules.BidOption ChooseBid(MariasGameState gameState)
        {
            // Easy AI vždy passuje
            return MariasGameRules.BidOption.Pass;
        }

        public CardSuit ChooseTrumpSuit(IReadOnlyList<Card> hand)
        {
            // Náhodná barva z karet v ruce
            if (hand == null || hand.Count == 0)
                return CardSuit.Hearts;

            return hand[Random.Next(hand.Count)].Suit;
        }

        public List<Card> ChooseCardsToDiscard(IReadOnlyList<Card> hand, MariasGameState gameState)
        {
            if (hand == null || hand.Count < 2)
                return new List<Card>();

            // Odhoď 2 náhodné karty
            var shuffled = hand.OrderBy(_ => Random.Next()).ToList();
            return shuffled.Take(2).ToList();
        }
    }
}
