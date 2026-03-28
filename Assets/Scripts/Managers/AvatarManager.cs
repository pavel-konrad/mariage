using UnityEngine;
using MariasGame.ScriptableObjects;
using MariasGame.Services;

namespace MariasGame.Managers
{
    public class AvatarManager : MonoBehaviour
    {
        [SerializeField] private AvatarDatabase avatarDatabase;

        private AvatarService _avatarService;

        void Awake() => Initialize();

        private void Initialize()
        {
            if (avatarDatabase == null)
            {
                Debug.LogError("[AvatarManager] AvatarDatabase is not assigned!");
                return;
            }
            _avatarService = new AvatarService(avatarDatabase);
        }

        public AvatarService GetAvatarService() => _avatarService;

        public void SetAvatarDatabase(AvatarDatabase newDatabase)
        {
            if (newDatabase != null)
            {
                avatarDatabase = newDatabase;
                Initialize();
            }
        }

        public AvatarDatabase GetAvatarDatabase() => avatarDatabase;
    }
}
