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
    /// Unity data karty (sprite, sound, animace).
    /// </summary>
    [System.Serializable]
    public class CardData
    {
        public Sprite Sprite;
        public AudioClip Sound;
        public AnimationClip Animation;
    }
}

