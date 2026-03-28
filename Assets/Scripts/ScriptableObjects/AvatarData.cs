using UnityEngine;

namespace MariasGame.ScriptableObjects
{
    [CreateAssetMenu(fileName = "AvatarData", menuName = "MariasGame/Avatar Data", order = 4)]
    public class AvatarData : ScriptableObject
    {
        [field: SerializeField] public string AvatarName { get; private set; }
        [field: SerializeField] public Sprite AvatarSprite { get; private set; }
        [field: SerializeField] public int AvatarId { get; private set; }

        private void OnValidate()
        {
            if (AvatarSprite == null)
                Debug.LogWarning($"[AvatarData] {name}: Avatar sprite is missing!");
            if (string.IsNullOrEmpty(AvatarName))
                Debug.LogWarning($"[AvatarData] {name}: Avatar name is missing!");
        }
    }
}
