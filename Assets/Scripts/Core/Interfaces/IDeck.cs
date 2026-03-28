using System.Collections.Generic;
using MariasGame.Core;

namespace MariasGame.Core.Interfaces
{
    /// <summary>
    /// Interface pro balíček karet.
    /// </summary>
    public interface IDeck
    {
        int Count { get; }
        IReadOnlyList<Card> Cards { get; }
        bool Contains(Card card);
        bool AddCard(Card card);
        bool RemoveCard(Card card);
        Card DrawCard();
        List<Card> DrawCards(int count);
    }
}
