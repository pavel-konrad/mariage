namespace MariasGame.Core.Interfaces
{
    /// <summary>
    /// Interface pro rozhodování v dražbě Mariáše.
    /// Implementováno pouze AI hráči.
    /// </summary>
    public interface IBetChooser
    {
        MariasGameRules.BidOption ChooseBid(MariasGameState gameState);
    }
}
