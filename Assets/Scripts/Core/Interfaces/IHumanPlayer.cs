namespace MariasGame.Core.Interfaces
{
    /// <summary>
    /// Interface pro lidského hráče.
    /// Kombinuje IPlayer, IHand, IBetManager bez ICardChooser a IBetChooser.
    /// </summary>
    public interface IHumanPlayer : IPlayer, IHand, IBetManager
    {
    }
}

