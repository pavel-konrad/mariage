using UnityEngine;
using MariasGame.Core.Interfaces;
using MariasGame.ScriptableObjects;
using MariasGame.Services;

namespace MariasGame.Managers
{
    /// <summary>
    /// Manager pro poskytování dat nepřátel (AI hráčů).
    /// Encapsuluje IEnemyProvider pro Unity komponenty.
    /// </summary>
    public class EnemyManager : MonoBehaviour
    {
        [Header("Enemy Database")]
        [SerializeField] private EnemyDatabaseSO enemyDatabase;
        
        private IEnemyProvider _enemyProvider;
        
        /// <summary>
        /// Získá IEnemyProvider.
        /// </summary>
        public IEnemyProvider GetEnemyProvider()
        {
            EnsureInitialized();
            return _enemyProvider;
        }
        
        private void Awake()
        {
            Initialize();
        }
        
        /// <summary>
        /// Inicializuje EnemyService.
        /// </summary>
        private void Initialize()
        {
            if (enemyDatabase == null)
            {
                Debug.LogError("[EnemyManager] EnemyDatabaseSO is not assigned!");
                return;
            }
            
            _enemyProvider = new EnemyService(enemyDatabase);
            Debug.Log($"[EnemyManager] Initialized with {_enemyProvider.EnemyCount} enemies.");
        }
        
        /// <summary>
        /// Zajistí, že je služba inicializována (pro Editor kompatibilitu).
        /// </summary>
        public void EnsureInitialized()
        {
            if (_enemyProvider == null)
            {
                Initialize();
            }
        }
        
        private void OnValidate()
        {
            if (enemyDatabase == null)
            {
                Debug.LogWarning("[EnemyManager] EnemyDatabaseSO is not assigned!");
            }
        }
    }
}
