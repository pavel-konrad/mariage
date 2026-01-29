using UnityEngine;
using MariasGame.Core;

namespace MariasGame.ScriptableObjects
{
    /// <summary>
    /// ScriptableObject pro data karty.
    /// Obsahuje Unity assety (sprite, sound, animation) pro konkrétní kartu.
    /// </summary>
    [CreateAssetMenu(fileName = "CardData", menuName = "MariasGame/Card Data", order = 1)]
    public class CardDataSO : ScriptableObject
    {
        [Header("Card Info")]
        public CardSuit suit;
        public CardRank rank;
        public bool isInGame = true;
        
        [Header("Visual Assets")]
        public Sprite cardSprite;
        
        [Header("Audio Assets")]
        [Tooltip("Variace zvuků pro náhodný výběr")]
        public AudioClip[] cardSounds;
        
        [Header("Animation")]
        public AnimationClip cardAnimation;
        
        /// <summary>
        /// Vrací náhodný zvuk z variations.
        /// </summary>
        public AudioClip GetRandomSound()
        {
            if (cardSounds == null || cardSounds.Length == 0)
                return null;
                
            int randomIndex = Random.Range(0, cardSounds.Length);
            return cardSounds[randomIndex];
        }
        
        private void OnValidate()
        {
            // Validace v Editoru
            if (cardSprite == null)
            {
                Debug.LogWarning($"[CardDataSO] {name}: Card sprite is missing!");
            }
        }
    }
}

