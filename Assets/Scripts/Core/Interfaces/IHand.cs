using System.Collections.Generic;
using MariasGame.Core;

namespace MariasGame.Core.Interfaces
{
    /// <summary>
    /// Interface pro správu ruky hráče.
    /// Definuje operace pro práci s kartami v ruce.
    /// </summary>
    public interface IHand
    {
        IReadOnlyList<Card> Hand { get; }
        int HandCount { get; }
        bool HasCards { get; }
        
        void AddCard(Card card);
        bool RemoveCard(Card card);
        void ClearHand();
    }
}

