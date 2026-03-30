using UnityEngine;
using UnityEngine.Serialization;
using MariasGame.Core;

namespace MariasGame.ScriptableObjects
{
    [CreateAssetMenu(fileName = "CardData", menuName = "MariasGame/Card Data", order = 1)]
    public class CardData : ScriptableObject
    {
        [field: FormerlySerializedAs("suit"), SerializeField] public CardSuit Suit { get; set; }
        [field: FormerlySerializedAs("rank"), SerializeField] public CardRank Rank { get; set; }
        [field: FormerlySerializedAs("isInGame"), SerializeField] public bool IsInGame { get; set; } = true;
        [field: FormerlySerializedAs("cardSprite"), SerializeField] public Sprite CardSprite { get; set; }

        private void OnValidate()
        {
            if (CardSprite == null)
                Debug.LogWarning($"[CardData] {name}: Card sprite is missing!");
        }
    }
}
