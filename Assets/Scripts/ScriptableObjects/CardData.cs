using UnityEngine;
using MariasGame.Core;

namespace MariasGame.ScriptableObjects
{
    [CreateAssetMenu(fileName = "CardData", menuName = "MariasGame/Card Data", order = 1)]
    public class CardData : ScriptableObject
    {
        [field: SerializeField] public CardSuit Suit { get; set; }
        [field: SerializeField] public CardRank Rank { get; set; }
        [field: SerializeField] public bool IsInGame { get; set; } = true;
        [field: SerializeField] public Sprite CardSprite { get; set; }

        private void OnValidate()
        {
            if (CardSprite == null)
                Debug.LogWarning($"[CardData] {name}: Card sprite is missing!");
        }
    }
}
