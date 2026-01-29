using System.Collections.Generic;
using MariasGame.Core;

namespace MariasGame.Core.Interfaces
{
    /// <summary>
    /// Interface pro balíček karet.
    /// Rozšiřuje ICardCollection o operace specifické pro balíček.
    /// </summary>
    public interface IDeck : ICardCollection
    {
        List<Card> DrawCards(int count);
    }
}

