using System.Collections.Generic;
using System.Linq;
using MariasGame.Core.Interfaces;
using MariasGame.ScriptableObjects;

namespace MariasGame.Services
{
    /// <summary>
    /// Služba pro poskytování avatarů.
    /// Načítá avatary z AvatarDatabaseSO.
    /// </summary>
    public class AvatarService : IAvatarProvider
    {
        private readonly AvatarDatabaseSO _database;
        private readonly List<AvatarData> _cachedAvatars = new List<AvatarData>();
        
        public AvatarService(AvatarDatabaseSO database)
        {
            _database = database ?? throw new System.ArgumentNullException(nameof(database));
            CacheAvatars();
        }
        
        /// <summary>
        /// Získá avatar pro konkrétní index.
        /// </summary>
        public AvatarData GetAvatar(int index)
        {
            if (index < 0 || index >= _cachedAvatars.Count)
                return null;
                
            return _cachedAvatars[index];
        }
        
        /// <summary>
        /// Získá všechny avatary.
        /// </summary>
        public IReadOnlyList<AvatarData> GetAllAvatars()
        {
            return _cachedAvatars.AsReadOnly();
        }
        
        /// <summary>
        /// Počet avatarů.
        /// </summary>
        public int AvatarCount => _cachedAvatars.Count;
        
        /// <summary>
        /// Získá avatar podle ID.
        /// </summary>
        public AvatarData GetAvatarById(int avatarId)
        {
            var avatarSO = _database.GetAvatarById(avatarId);
            if (avatarSO == null)
                return null;
                
            return new AvatarData
            {
                AvatarName = avatarSO.avatarName,
                AvatarSprite = avatarSO.avatarSprite,
                AvatarId = avatarSO.avatarId
            };
        }
        
        private void CacheAvatars()
        {
            _cachedAvatars.Clear();
            
            if (_database.avatars == null)
                return;
                
            foreach (var avatarSO in _database.avatars.Where(a => a != null))
            {
                _cachedAvatars.Add(new AvatarData
                {
                    AvatarName = avatarSO.avatarName,
                    AvatarSprite = avatarSO.avatarSprite,
                    AvatarId = avatarSO.avatarId
                });
            }
        }
    }
}

