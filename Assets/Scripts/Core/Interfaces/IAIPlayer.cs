namespace MariasGame.Core.Interfaces
{
    /// <summary>
    /// Interface pro AI hráče v Mariáši.
    /// Kombinuje IPlayer, IHand, IBetManager, ICardChooser, IBetChooser.
    /// </summary>
    public interface IAIPlayer : IPlayer, IHand, IBetManager, ICardChooser, IBetChooser
    {
    }
}

