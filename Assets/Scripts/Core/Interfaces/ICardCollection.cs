using System.Collections.Generic;
using MariasGame.Core;

namespace MariasGame.Core.Interfaces
{
    /// <summary>
    /// Základní interface pro kolekci karet.
    /// Společný interface pro kolekce karet (Deck, Talon, atd.).
    /// </summary>
    public interface ICardCollection
    {
        int Count { get; }
        bool IsEmpty { get; }
        IReadOnlyList<Card> Cards { get; }
        
        void AddCard(Card card);
        void AddCards(IEnumerable<Card> cards);
        Card DrawCard();
        Card PeekTopCard();
        void Clear();
    }
}

