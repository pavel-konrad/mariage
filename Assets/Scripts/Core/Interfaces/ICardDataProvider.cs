using UnityEngine;
using MariasGame.Core;

namespace MariasGame.Core.Interfaces
{
    /// <summary>
    /// Interface pro poskytování dat karet.
    /// </summary>
    public interface ICardDataProvider
    {
        CardData GetCardData(CardSuit suit, CardRank rank);
    }
    
    /// <summary>
    /// Unity data karty (sprite).
    /// Zvuky a animace jsou řízeny centrálně přes AudioManager a CardAnimationController.
    /// </summary>
    [System.Serializable]
    public class CardData
    {
        public Sprite Sprite;
    }
}

