using System.Collections.Generic;
using UnityEngine;

namespace MariasGame.Core.Interfaces
{
    /// <summary>
    /// Interface pro poskytování dat nepřátel (AI hráčů).
    /// Abstrakce pro přístup k nepřátelům (ScriptableObject, Database, atd.).
    /// </summary>
    public interface IEnemyProvider
    {
        EnemyData GetEnemy(int index);
        IReadOnlyList<EnemyData> GetAllEnemies();
        int EnemyCount { get; }
    }
    
    /// <summary>
    /// Data nepřítele (AI hráče).
    /// </summary>
    [System.Serializable]
    public class EnemyData
    {
        public string EnemyName;
        public Sprite EnemyAvatarSprite;
        public int EnemyId;
    }
}
