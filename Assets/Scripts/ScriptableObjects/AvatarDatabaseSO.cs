using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MariasGame.ScriptableObjects
{
    /// <summary>
    /// ScriptableObject pro databázi avatarů.
    /// </summary>
    [CreateAssetMenu(fileName = "AvatarDatabase", menuName = "MariasGame/Avatar Database", order = 5)]
    public class AvatarDatabaseSO : ScriptableObject
    {
        [Header("Avatars")]
        public List<AvatarDataSO> avatars = new List<AvatarDataSO>();
        
        /// <summary>
        /// Získá AvatarDataSO pro konkrétní index.
        /// </summary>
        public AvatarDataSO GetAvatar(int index)
        {
            if (index < 0 || index >= avatars.Count)
                return null;
                
            return avatars[index];
        }
        
        /// <summary>
        /// Získá AvatarDataSO pro konkrétní ID.
        /// </summary>
        public AvatarDataSO GetAvatarById(int avatarId)
        {
            return avatars.FirstOrDefault(a => a != null && a.avatarId == avatarId);
        }
        
        private void OnValidate()
        {
            if (avatars == null || avatars.Count == 0)
            {
                Debug.LogWarning($"[AvatarDatabaseSO] {name}: No avatars in database!");
                return;
            }
            
            // Kontrola duplicit ID
            var duplicateIds = avatars
                .Where(a => a != null)
                .GroupBy(a => a.avatarId)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key);
                
            if (duplicateIds.Any())
            {
                Debug.LogWarning($"[AvatarDatabaseSO] {name}: Duplicate avatar IDs found: {string.Join(", ", duplicateIds)}");
            }
            
            // Kontrola chybějících assetů
            foreach (var avatar in avatars.Where(a => a != null))
            {
                if (avatar.avatarSprite == null)
                {
                    Debug.LogWarning($"[AvatarDatabaseSO] {name}: Avatar {avatar.avatarName} (ID: {avatar.avatarId}) is missing sprite!");
                }
            }
        }
    }
}

