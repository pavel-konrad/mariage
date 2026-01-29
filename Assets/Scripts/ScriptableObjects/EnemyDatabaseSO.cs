using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MariasGame.ScriptableObjects
{
    /// <summary>
    /// ScriptableObject pro databázi nepřátel (AI hráčů).
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyDatabase", menuName = "MariasGame/Enemy Database", order = 7)]
    public class EnemyDatabaseSO : ScriptableObject
    {
        [Header("Enemies")]
        public List<EnemyDataSO> enemies = new List<EnemyDataSO>();
        
        /// <summary>
        /// Získá EnemyDataSO pro konkrétní index.
        /// </summary>
        public EnemyDataSO GetEnemy(int index)
        {
            if (index < 0 || index >= enemies.Count)
                return null;
                
            return enemies[index];
        }
        
        /// <summary>
        /// Získá EnemyDataSO pro konkrétní ID.
        /// </summary>
        public EnemyDataSO GetEnemyById(int enemyId)
        {
            return enemies.FirstOrDefault(e => e != null && e.enemyId == enemyId);
        }
        
        private void OnValidate()
        {
            if (enemies == null || enemies.Count == 0)
            {
                Debug.LogWarning($"[EnemyDatabaseSO] {name}: No enemies in database!");
                return;
            }
            
            // Kontrola duplicit ID
            var duplicateIds = enemies
                .Where(e => e != null)
                .GroupBy(e => e.enemyId)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key);
                
            if (duplicateIds.Any())
            {
                Debug.LogWarning($"[EnemyDatabaseSO] {name}: Duplicate enemy IDs found: {string.Join(", ", duplicateIds)}");
            }
            
            // Kontrola chybějících assetů
            foreach (var enemy in enemies.Where(e => e != null))
            {
                if (enemy.enemyAvatarSprite == null)
                {
                    Debug.LogWarning($"[EnemyDatabaseSO] {name}: Enemy {enemy.enemyName} (ID: {enemy.enemyId}) is missing avatar sprite!");
                }
            }
        }
    }
}
