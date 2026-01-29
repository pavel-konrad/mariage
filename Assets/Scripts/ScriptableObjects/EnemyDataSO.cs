using UnityEngine;

namespace MariasGame.ScriptableObjects
{
    /// <summary>
    /// ScriptableObject pro data nepřítele (AI hráče).
    /// Obsahuje jméno a avatar sprite pro konkrétního nepřítele.
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyData", menuName = "MariasGame/Enemy Data", order = 6)]
    public class EnemyDataSO : ScriptableObject
    {
        [Header("Enemy Info")]
        public string enemyName;
        public Sprite enemyAvatarSprite;
        public int enemyId;
        
        private void OnValidate()
        {
            // Validace v Editoru
            if (enemyAvatarSprite == null)
            {
                Debug.LogWarning($"[EnemyDataSO] {name}: Enemy avatar sprite is missing!");
            }
            
            if (string.IsNullOrEmpty(enemyName))
            {
                Debug.LogWarning($"[EnemyDataSO] {name}: Enemy name is missing!");
            }
        }
    }
}
