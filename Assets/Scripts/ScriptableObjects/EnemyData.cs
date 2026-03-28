using UnityEngine;

namespace MariasGame.ScriptableObjects
{
    [CreateAssetMenu(fileName = "EnemyData", menuName = "MariasGame/Enemy Data", order = 6)]
    public class EnemyData : ScriptableObject
    {
        [field: SerializeField] public string EnemyName { get; private set; }
        [field: SerializeField] public Sprite EnemyAvatarSprite { get; private set; }
        [field: SerializeField] public int EnemyId { get; private set; }

        private void OnValidate()
        {
            if (EnemyAvatarSprite == null)
                Debug.LogWarning($"[EnemyData] {name}: Enemy avatar sprite is missing!");
            if (string.IsNullOrEmpty(EnemyName))
                Debug.LogWarning($"[EnemyData] {name}: Enemy name is missing!");
        }
    }
}
