using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MariasGame.ScriptableObjects
{
    [CreateAssetMenu(fileName = "EnemyDatabase", menuName = "MariasGame/Enemy Database", order = 7)]
    public class EnemyDatabase : ScriptableObject
    {
        [field: SerializeField] public List<EnemyData> Enemies { get; private set; } = new List<EnemyData>();

        public EnemyData GetEnemy(int index)
        {
            if (index < 0 || index >= Enemies.Count) return null;
            return Enemies[index];
        }

        public EnemyData GetEnemyById(int enemyId)
            => Enemies.FirstOrDefault(e => e != null && e.EnemyId == enemyId);

        private void OnValidate()
        {
            if (Enemies == null || Enemies.Count == 0)
            {
                Debug.LogWarning($"[EnemyDatabase] {name}: No enemies in database!");
                return;
            }

            var duplicateIds = Enemies
                .Where(e => e != null)
                .GroupBy(e => e.EnemyId)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key);

            if (duplicateIds.Any())
                Debug.LogWarning($"[EnemyDatabase] {name}: Duplicate enemy IDs found: {string.Join(", ", duplicateIds)}");

            foreach (var enemy in Enemies.Where(e => e != null))
            {
                if (enemy.EnemyAvatarSprite == null)
                    Debug.LogWarning($"[EnemyDatabase] {name}: Enemy {enemy.EnemyName} (ID: {enemy.EnemyId}) is missing avatar sprite!");
            }
        }
    }
}
