using System.Collections.Generic;
using MariasGame.Core;
using MariasGame.Core.Interfaces;

namespace MariasGame.Game
{
    /// <summary>
    /// Implementace AI hráče pro Mariáš.
    /// Dědí z PlayerBase a implementuje IAIPlayer.
    /// Používá IAIStrategy pro výběr karet, dražbu, trumfy a odhazování.
    /// </summary>
    public class AIPlayer : PlayerBase, IAIPlayer
    {
        private readonly IAIStrategy _strategy;

        public AIPlayer(int id, string name, IAIStrategy strategy, int startingCash = 1000, int avatarIndex = 0)
            : base(id, name, startingCash, avatarIndex)
        {
            _strategy = strategy ?? throw new System.ArgumentNullException(nameof(strategy));
        }

        /// <summary>
        /// AI hráč je vždy IsHuman = false.
        /// </summary>
        public override bool IsHuman => false;

        /// <summary>
        /// AI volí kartu podle své strategie.
        /// </summary>
        public Card ChooseCardToPlay(IReadOnlyList<Card> legalPlays, MariasGameState gameState)
        {
            if (legalPlays == null || legalPlays.Count == 0)
                return null;

            return _strategy.ChooseCardToPlay(legalPlays, gameState);
        }

        /// <summary>
        /// AI volí nabídku v dražbě.
        /// </summary>
        public MariasGameRules.BidOption ChooseBid(MariasGameState gameState)
        {
            return _strategy.ChooseBid(gameState);
        }

        /// <summary>
        /// AI volí trumfovou barvu.
        /// </summary>
        public CardSuit ChooseTrumpSuit()
        {
            return _strategy.ChooseTrumpSuit(Hand);
        }

        /// <summary>
        /// AI volí karty k odhození.
        /// </summary>
        public List<Card> ChooseCardsToDiscard(MariasGameState gameState)
        {
            return _strategy.ChooseCardsToDiscard(Hand, gameState);
        }

        public override string ToString()
        {
            return $"[AI] {base.ToString()}";
        }
    }
}
