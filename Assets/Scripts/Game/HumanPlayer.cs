using MariasGame.Core;
using MariasGame.Core.Interfaces;

namespace MariasGame.Game
{
    /// <summary>
    /// Implementace lidského hráče.
    /// Dědí z PlayerBase a implementuje IHumanPlayer.
    /// Legalita tahů je řízena MariasGameController přes MariasGameRules.GetLegalPlays().
    /// </summary>
    public class HumanPlayer : PlayerBase, IHumanPlayer
    {
        public HumanPlayer(int id, string name, int startingCash = 1000, int avatarIndex = 0)
            : base(id, name, startingCash, avatarIndex)
        {
        }

        /// <summary>
        /// Lidský hráč je vždy IsHuman = true.
        /// </summary>
        public override bool IsHuman => true;

        public override string ToString()
        {
            return $"[Human] {base.ToString()}";
        }
    }
}
