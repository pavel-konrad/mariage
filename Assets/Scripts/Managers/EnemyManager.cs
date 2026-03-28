using UnityEngine;
using MariasGame.ScriptableObjects;
using MariasGame.Services;

namespace MariasGame.Managers
{
    public class EnemyManager : MonoBehaviour
    {
        [SerializeField] private EnemyDatabase enemyDatabase;

        private EnemyService _enemyService;

        public EnemyService GetEnemyService()
        {
            EnsureInitialized();
            return _enemyService;
        }

        private void Awake() => Initialize();

        private void Initialize()
        {
            if (enemyDatabase == null)
            {
                Debug.LogError("[EnemyManager] EnemyDatabase is not assigned!");
                return;
            }
            _enemyService = new EnemyService(enemyDatabase);
#if UNITY_EDITOR
            Debug.Log($"[EnemyManager] Initialized with {_enemyService.EnemyCount} enemies.");
#endif
        }

        public void EnsureInitialized()
        {
            if (_enemyService == null) Initialize();
        }

        private void OnValidate()
        {
            if (enemyDatabase == null)
                Debug.LogWarning("[EnemyManager] EnemyDatabase is not assigned!");
        }
    }
}
