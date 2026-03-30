using UnityEngine;
using MariasGame.ScriptableObjects;
using MariasGame.Services;

namespace MariasGame.Managers
{
    public class CharacterManager : MonoBehaviour
    {
        [SerializeField] private CharacterDatabase characterDatabase;

        private CharacterService _service;

        void Awake() => Initialize();

        private void Initialize()
        {
            if (characterDatabase == null)
            {
                Debug.LogError("[CharacterManager] CharacterDatabase is not assigned!");
                return;
            }
            _service = new CharacterService(characterDatabase);
        }

        public CharacterService GetService()
        {
            EnsureInitialized();
            return _service;
        }

        public void EnsureInitialized()
        {
            if (_service == null) Initialize();
        }

        private void OnValidate()
        {
            if (characterDatabase == null)
                Debug.LogWarning("[CharacterManager] CharacterDatabase is not assigned!");
        }
    }
}
