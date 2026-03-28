using System.Collections.Generic;
using System.Linq;
using MariasGame.ScriptableObjects;

namespace MariasGame.Services
{
    public class AvatarService
    {
        private readonly AvatarDatabase _database;

        public AvatarService(AvatarDatabase database)
        {
            _database = database ?? throw new System.ArgumentNullException(nameof(database));
        }

        public AvatarData GetAvatar(int index) => _database.GetAvatar(index);

        public IReadOnlyList<AvatarData> GetAllAvatars()
            => _database.Avatars.Where(a => a != null).ToList().AsReadOnly();

        public int AvatarCount => _database.Avatars?.Count(a => a != null) ?? 0;

        public AvatarData GetAvatarById(int avatarId) => _database.GetAvatarById(avatarId);
    }
}
