using System.Collections.Generic;

namespace MariasGame.Core.Interfaces
{
    /// <summary>
    /// Interface pro výběr karty k hraní.
    /// Implementováno pouze AI hráči.
    /// </summary>
    public interface ICardChooser
    {
        Card ChooseCardToPlay(IReadOnlyList<Card> legalPlays, MariasGameState gameState);
    }
}
