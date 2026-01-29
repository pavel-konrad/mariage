using UnityEngine;

namespace MariasGame.ScriptableObjects
{
    /// <summary>
    /// ScriptableObject pro data avataru.
    /// </summary>
    [CreateAssetMenu(fileName = "AvatarData", menuName = "MariasGame/Avatar Data", order = 4)]
    public class AvatarDataSO : ScriptableObject
    {
        [Header("Avatar Info")]
        public string avatarName;
        public Sprite avatarSprite;
        public int avatarId;
        
        private void OnValidate()
        {
            // Validace v Editoru
            if (avatarSprite == null)
            {
                Debug.LogWarning($"[AvatarDataSO] {name}: Avatar sprite is missing!");
            }
            
            if (string.IsNullOrEmpty(avatarName))
            {
                Debug.LogWarning($"[AvatarDataSO] {name}: Avatar name is missing!");
            }
        }
    }
}

