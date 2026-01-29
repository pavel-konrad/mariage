using System.Collections.Generic;
using System.Linq;
using MariasGame.Core.Interfaces;
using MariasGame.ScriptableObjects;

namespace MariasGame.Services
{
    /// <summary>
    /// Služba pro poskytování dat nepřátel (AI hráčů).
    /// Načítá nepřátele z EnemyDatabaseSO.
    /// </summary>
    public class EnemyService : IEnemyProvider
    {
        private readonly EnemyDatabaseSO _database;
        private readonly List<EnemyData> _cachedEnemies = new List<EnemyData>();
        
        public EnemyService(EnemyDatabaseSO database)
        {
            _database = database ?? throw new System.ArgumentNullException(nameof(database));
            CacheEnemies();
        }
        
        /// <summary>
        /// Získá nepřítele pro konkrétní index.
        /// </summary>
        public EnemyData GetEnemy(int index)
        {
            if (index < 0 || index >= _cachedEnemies.Count)
                return null;
                
            return _cachedEnemies[index];
        }
        
        /// <summary>
        /// Získá všechny nepřátele.
        /// </summary>
        public IReadOnlyList<EnemyData> GetAllEnemies()
        {
            return _cachedEnemies.AsReadOnly();
        }
        
        /// <summary>
        /// Počet nepřátel.
        /// </summary>
        public int EnemyCount => _cachedEnemies.Count;
        
        private void CacheEnemies()
        {
            _cachedEnemies.Clear();
            
            if (_database.enemies == null)
                return;
                
            foreach (var enemySO in _database.enemies.Where(e => e != null))
            {
                _cachedEnemies.Add(new EnemyData
                {
                    EnemyName = enemySO.enemyName,
                    EnemyAvatarSprite = enemySO.enemyAvatarSprite,
                    EnemyId = enemySO.enemyId
                });
            }
        }
    }
}
