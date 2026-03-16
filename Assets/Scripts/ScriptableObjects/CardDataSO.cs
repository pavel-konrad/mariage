using UnityEngine;
using MariasGame.Core;

namespace MariasGame.ScriptableObjects
{
    /// <summary>
    /// ScriptableObject pro data karty.
    /// Obsahuje vizuální assety (sprite) pro konkrétní kartu.
    /// Zvuky a animace jsou řízeny centrálně přes AudioManager a CardAnimationController.
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

        private void OnValidate()
        {
            if (cardSprite == null)
            {
                Debug.LogWarning($"[CardDataSO] {name}: Card sprite is missing!");
            }
        }
    }
}

