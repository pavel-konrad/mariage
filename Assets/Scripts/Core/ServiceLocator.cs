using System;
using System.Collections.Generic;
using UnityEngine;
using MariasGame.Managers;
using MariasGame.Core.Interfaces;

namespace MariasGame.Core
{
    /// <summary>
    /// Centrální Service Locator pro správu všech managerů a služeb.
    /// Automaticky načítá reference na managery při spuštění.
    /// Singleton pattern pro globální přístup.
    /// </summary>
    public class ServiceLocator : MonoBehaviour
    {
        private static ServiceLocator _instance;
        public static ServiceLocator Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<ServiceLocator>();
                    if (_instance == null)
                    {
                        var go = new GameObject("[ServiceLocator]");
                        _instance = go.AddComponent<ServiceLocator>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }
        
        [Header("Auto-Discovery")]
        [Tooltip("Automaticky vyhledá managery ve scéně při Awake")]
        [SerializeField] private bool autoDiscoverManagers = true;
        
        [Header("Manager References (Optional)")]
        [Tooltip("Můžete ručně přiřadit reference, nebo nechat auto-discovery")]
        [SerializeField] private CardDataManager cardDataManager;
        [SerializeField] private GameSettingsManager settingsManager;
        [SerializeField] private AvatarManager avatarManager;
        [SerializeField] private EnemyManager enemyManager;
        [SerializeField] private AudioManager audioManager;
        [SerializeField] private VFXManager vfxManager;
        
        // Cached service providers
        private ICardDataProvider _cardDataProvider;
        private IDeckFactory _deckFactory;
        private ISettingsProvider _settingsProvider;
        private IAvatarProvider _avatarProvider;
        private IEnemyProvider _enemyProvider;
        private ICardThemeProvider _cardThemeProvider;
        
        // Events pro notifikaci o změnách služeb
        public event Action OnServicesReady;
        public event Action<string> OnServiceRegistered;
        public event Action<string> OnServiceUnregistered;
        
        private bool _isInitialized = false;
        public bool IsInitialized => _isInitialized;
        
        void Awake()
        {
            // Singleton setup
            if (_instance != null && _instance != this)
            {
                Debug.LogWarning("[ServiceLocator] Duplicate instance found, destroying this one.");
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            
            if (transform.parent == null)
            {
                DontDestroyOnLoad(gameObject);
            }
            
            if (autoDiscoverManagers)
            {
                DiscoverManagers();
            }
            
            CacheServiceProviders();
            _isInitialized = true;
            
            Debug.Log("[ServiceLocator] Initialized successfully.");
            OnServicesReady?.Invoke();
        }
        
        /// <summary>
        /// Automaticky vyhledá managery ve scéně.
        /// </summary>
        public void DiscoverManagers()
        {
            Debug.Log("[ServiceLocator] Auto-discovering managers...");
            
            if (cardDataManager == null)
                cardDataManager = FindObjectOfType<CardDataManager>();
            
            if (settingsManager == null)
                settingsManager = FindObjectOfType<GameSettingsManager>();
            
            if (avatarManager == null)
                avatarManager = FindObjectOfType<AvatarManager>();
            
            if (enemyManager == null)
                enemyManager = FindObjectOfType<EnemyManager>();
            
            if (audioManager == null)
                audioManager = FindObjectOfType<AudioManager>();
            
            if (vfxManager == null)
                vfxManager = FindObjectOfType<VFXManager>();
            
            LogDiscoveryResults();
        }
        
        /// <summary>
        /// Loguje výsledky auto-discovery.
        /// </summary>
        private void LogDiscoveryResults()
        {
            Debug.Log($"[ServiceLocator] Discovery results:");
            Debug.Log($"  - CardDataManager: {(cardDataManager != null ? "✓" : "✗")}");
            Debug.Log($"  - GameSettingsManager: {(settingsManager != null ? "✓" : "✗")}");
            Debug.Log($"  - AvatarManager: {(avatarManager != null ? "✓" : "✗")}");
            Debug.Log($"  - EnemyManager: {(enemyManager != null ? "✓" : "✗")}");
            Debug.Log($"  - AudioManager: {(audioManager != null ? "✓" : "✗")}");
            Debug.Log($"  - VFXManager: {(vfxManager != null ? "✓" : "✗")}");
        }
        
        /// <summary>
        /// Cache service providers pro rychlejší přístup.
        /// </summary>
        private void CacheServiceProviders()
        {
            _cardDataProvider = cardDataManager?.GetCardDataProvider();
            _deckFactory = cardDataManager?.GetDeckFactory();
            _settingsProvider = settingsManager;
            _avatarProvider = avatarManager?.GetAvatarProvider();
            _enemyProvider = enemyManager?.GetEnemyProvider();
            _cardThemeProvider = cardDataManager?.GetThemeProvider();
        }
        
        /// <summary>
        /// Znovu načte všechny služby (např. po změně scény).
        /// </summary>
        public void RefreshServices()
        {
            DiscoverManagers();
            CacheServiceProviders();
            Debug.Log("[ServiceLocator] Services refreshed.");
        }
        
        #region Manager Accessors
        
        /// <summary>
        /// Získá CardDataManager.
        /// </summary>
        public CardDataManager GetCardDataManager()
        {
            return cardDataManager;
        }
        
        /// <summary>
        /// Získá GameSettingsManager.
        /// </summary>
        public GameSettingsManager GetSettingsManager()
        {
            return settingsManager;
        }
        
        /// <summary>
        /// Získá AvatarManager.
        /// </summary>
        public AvatarManager GetAvatarManager()
        {
            return avatarManager;
        }
        
        /// <summary>
        /// Získá EnemyManager.
        /// </summary>
        public EnemyManager GetEnemyManager()
        {
            return enemyManager;
        }
        
        /// <summary>
        /// Získá AudioManager.
        /// </summary>
        public AudioManager GetAudioManager()
        {
            return audioManager;
        }
        
        /// <summary>
        /// Získá VFXManager.
        /// </summary>
        public VFXManager GetVFXManager()
        {
            return vfxManager;
        }
        
        #endregion
        
        #region Service Provider Accessors
        
        /// <summary>
        /// Získá ICardDataProvider.
        /// </summary>
        public ICardDataProvider GetCardDataProvider()
        {
            if (_cardDataProvider == null)
                _cardDataProvider = cardDataManager?.GetCardDataProvider();
            return _cardDataProvider;
        }
        
        /// <summary>
        /// Získá IDeckFactory.
        /// </summary>
        public IDeckFactory GetDeckFactory()
        {
            if (_deckFactory == null)
                _deckFactory = cardDataManager?.GetDeckFactory();
            return _deckFactory;
        }
        
        /// <summary>
        /// Získá ISettingsProvider.
        /// </summary>
        public ISettingsProvider GetSettingsProvider()
        {
            return _settingsProvider;
        }
        
        /// <summary>
        /// Získá IAvatarProvider.
        /// </summary>
        public IAvatarProvider GetAvatarProvider()
        {
            if (_avatarProvider == null)
                _avatarProvider = avatarManager?.GetAvatarProvider();
            return _avatarProvider;
        }
        
        /// <summary>
        /// Získá IEnemyProvider.
        /// </summary>
        public IEnemyProvider GetEnemyProvider()
        {
            if (_enemyProvider == null)
                _enemyProvider = enemyManager?.GetEnemyProvider();
            return _enemyProvider;
        }
        
        /// <summary>
        /// Získá ICardThemeProvider.
        /// </summary>
        public ICardThemeProvider GetCardThemeProvider()
        {
            if (_cardThemeProvider == null)
                _cardThemeProvider = cardDataManager?.GetThemeProvider();
            return _cardThemeProvider;
        }
        
        #endregion
        
        #region Manual Registration
        
        /// <summary>
        /// Ručně registruje CardDataManager.
        /// </summary>
        public void RegisterCardDataManager(CardDataManager manager)
        {
            cardDataManager = manager;
            _cardDataProvider = manager?.GetCardDataProvider();
            _deckFactory = manager?.GetDeckFactory();
            _cardThemeProvider = manager?.GetThemeProvider();
            OnServiceRegistered?.Invoke(nameof(CardDataManager));
        }
        
        /// <summary>
        /// Ručně registruje GameSettingsManager.
        /// </summary>
        public void RegisterSettingsManager(GameSettingsManager manager)
        {
            settingsManager = manager;
            _settingsProvider = manager;
            OnServiceRegistered?.Invoke(nameof(GameSettingsManager));
        }
        
        /// <summary>
        /// Ručně registruje AvatarManager.
        /// </summary>
        public void RegisterAvatarManager(AvatarManager manager)
        {
            avatarManager = manager;
            _avatarProvider = manager?.GetAvatarProvider();
            OnServiceRegistered?.Invoke(nameof(AvatarManager));
        }
        
        /// <summary>
        /// Ručně registruje EnemyManager.
        /// </summary>
        public void RegisterEnemyManager(EnemyManager manager)
        {
            enemyManager = manager;
            _enemyProvider = manager?.GetEnemyProvider();
            OnServiceRegistered?.Invoke(nameof(EnemyManager));
        }
        
        /// <summary>
        /// Ručně registruje AudioManager.
        /// </summary>
        public void RegisterAudioManager(AudioManager manager)
        {
            audioManager = manager;
            OnServiceRegistered?.Invoke(nameof(AudioManager));
        }
        
        /// <summary>
        /// Ručně registruje VFXManager.
        /// </summary>
        public void RegisterVFXManager(VFXManager manager)
        {
            vfxManager = manager;
            OnServiceRegistered?.Invoke(nameof(VFXManager));
        }
        
        #endregion
        
        #region Validation
        
        /// <summary>
        /// Zkontroluje, zda jsou všechny požadované služby dostupné.
        /// </summary>
        public bool ValidateRequiredServices()
        {
            bool isValid = true;
            
            if (cardDataManager == null)
            {
                Debug.LogError("[ServiceLocator] CardDataManager is missing!");
                isValid = false;
            }
            
            if (settingsManager == null)
            {
                Debug.LogError("[ServiceLocator] GameSettingsManager is missing!");
                isValid = false;
            }
            
            return isValid;
        }
        
        /// <summary>
        /// Zkontroluje dostupnost konkrétní služby.
        /// </summary>
        public bool HasService<T>() where T : class
        {
            var typeName = typeof(T).Name;
            
            return typeName switch
            {
                nameof(CardDataManager) => cardDataManager != null,
                nameof(GameSettingsManager) => settingsManager != null,
                nameof(AvatarManager) => avatarManager != null,
                nameof(EnemyManager) => enemyManager != null,
                nameof(AudioManager) => audioManager != null,
                nameof(VFXManager) => vfxManager != null,
                nameof(ICardDataProvider) => _cardDataProvider != null,
                nameof(IDeckFactory) => _deckFactory != null,
                nameof(ISettingsProvider) => _settingsProvider != null,
                nameof(IAvatarProvider) => _avatarProvider != null,
                nameof(IEnemyProvider) => _enemyProvider != null,
                nameof(ICardThemeProvider) => _cardThemeProvider != null,
                _ => false
            };
        }
        
        #endregion
    }
}
