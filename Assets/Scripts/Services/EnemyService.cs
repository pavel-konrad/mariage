using System.Collections.Generic;
using System.Linq;
using MariasGame.ScriptableObjects;

namespace MariasGame.Services
{
    public class EnemyService
    {
        private readonly EnemyDatabase _database;

        public EnemyService(EnemyDatabase database)
        {
            _database = database ?? throw new System.ArgumentNullException(nameof(database));
        }

        public EnemyData GetEnemy(int index) => _database.GetEnemy(index);

        public IReadOnlyList<EnemyData> GetAllEnemies()
            => _database.Enemies.Where(e => e != null).ToList().AsReadOnly();

        public int EnemyCount => _database.Enemies?.Count(e => e != null) ?? 0;
    }
}
