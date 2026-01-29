using UnityEngine;
using MariasGame.Core.Interfaces;
using MariasGame.ScriptableObjects;
using MariasGame.Services;

namespace MariasGame.Managers
{
    /// <summary>
    /// Manager pro správu avatarů.
    /// Poskytuje AvatarService.
    /// </summary>
    public class AvatarManager : MonoBehaviour
    {
        [Header("Avatar Database")]
        [SerializeField] private AvatarDatabaseSO avatarDatabaseSO;
        
        private AvatarService _avatarService;
        
        void Awake()
        {
            InitializeServices();
        }
        
        /// <summary>
        /// Inicializuje službu pro práci s avatary.
        /// </summary>
        private void InitializeServices()
        {
            if (avatarDatabaseSO == null)
            {
                Debug.LogError("[AvatarManager] AvatarDatabaseSO is not assigned!");
                return;
            }
            
            _avatarService = new AvatarService(avatarDatabaseSO);
        }
        
        /// <summary>
        /// Získá službu pro poskytování avatarů.
        /// </summary>
        public IAvatarProvider GetAvatarProvider()
        {
            return _avatarService;
        }
        
        /// <summary>
        /// Nastaví nové AvatarDatabaseSO.
        /// </summary>
        public void SetAvatarDatabaseSO(AvatarDatabaseSO newDatabaseSO)
        {
            if (newDatabaseSO != null)
            {
                avatarDatabaseSO = newDatabaseSO;
                InitializeServices();
            }
        }
        
        /// <summary>
        /// Získá AvatarDatabaseSO.
        /// </summary>
        public AvatarDatabaseSO GetAvatarDatabaseSO()
        {
            return avatarDatabaseSO;
        }
    }
}

