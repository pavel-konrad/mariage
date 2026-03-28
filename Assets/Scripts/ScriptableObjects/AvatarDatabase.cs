using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MariasGame.ScriptableObjects
{
    [CreateAssetMenu(fileName = "AvatarDatabase", menuName = "MariasGame/Avatar Database", order = 5)]
    public class AvatarDatabase : ScriptableObject
    {
        [field: SerializeField] public List<AvatarData> Avatars { get; private set; } = new List<AvatarData>();

        public AvatarData GetAvatar(int index)
        {
            if (index < 0 || index >= Avatars.Count) return null;
            return Avatars[index];
        }

        public AvatarData GetAvatarById(int avatarId)
            => Avatars.FirstOrDefault(a => a != null && a.AvatarId == avatarId);

        private void OnValidate()
        {
            if (Avatars == null || Avatars.Count == 0)
            {
                Debug.LogWarning($"[AvatarDatabase] {name}: No avatars in database!");
                return;
            }

            var duplicateIds = Avatars
                .Where(a => a != null)
                .GroupBy(a => a.AvatarId)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key);

            if (duplicateIds.Any())
                Debug.LogWarning($"[AvatarDatabase] {name}: Duplicate avatar IDs found: {string.Join(", ", duplicateIds)}");

            foreach (var avatar in Avatars.Where(a => a != null))
            {
                if (avatar.AvatarSprite == null)
                    Debug.LogWarning($"[AvatarDatabase] {name}: Avatar {avatar.AvatarName} (ID: {avatar.AvatarId}) is missing sprite!");
            }
        }
    }
}
