using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MariasGame.Core.Interfaces;
using MariasGame.ScriptableObjects;
using MariasGame.Services;

namespace MariasGame.Managers
{
    /// <summary>
    /// Manager pro správu dat karet a témat balíčků.
    /// Poskytuje CardDataService, AssetLoaderService, DeckFactoryService, CardThemeService.
    /// Podporuje více témat balíčků s možností přepínání.
    /// </summary>
    public class CardDataManager : MonoBehaviour
    {
        [Header("Card Themes")]
        [SerializeField] private List<CardThemeSO> availableThemes = new List<CardThemeSO>();
        [SerializeField] private CardThemeSO defaultTheme;
        
        [Header("Legacy (Deprecated)")]
        [SerializeField] private CardDatabaseSO cardDatabaseSO; // Pro zpětnou kompatibilitu
        
        private CardDataService _cardDataService;
        private AssetLoaderService _assetLoaderService;
        private DeckFactoryService _deckFactoryService;
        private CardThemeService _themeService;
        
        void Awake()
        {
            InitializeServices();
        }
        
        /// <summary>
        /// Inicializuje služby pro práci s kartami.
        /// </summary>
        private void InitializeServices()
        {
            _assetLoaderService = new AssetLoaderService();
            
            // Pokud máme téma, použijeme je
            if (availableThemes != null && availableThemes.Count > 0)
            {
                _themeService = new CardThemeService(availableThemes, defaultTheme);
                var activeTheme = _themeService.GetActiveTheme();
                
                if (activeTheme != null && activeTheme.cardDatabase != null)
                {
                    InitializeServicesWithDatabase(activeTheme.cardDatabase);
                }
                else
                {
                    Debug.LogError("[CardDataManager] No valid theme or card database found!");
                    FallbackToLegacyDatabase();
                }
            }
            else if (cardDatabaseSO != null)
            {
                // Fallback na legacy CardDatabaseSO
                Debug.LogWarning("[CardDataManager] Using legacy CardDatabaseSO. Consider using CardThemeSO instead.");
                InitializeServicesWithDatabase(cardDatabaseSO);
            }
            else
            {
                Debug.LogError("[CardDataManager] No card database or themes assigned!");
            }
        }
        
        /// <summary>
        /// Inicializuje služby s konkrétní CardDatabaseSO.
        /// </summary>
        private void InitializeServicesWithDatabase(CardDatabaseSO database)
        {
            if (database == null)
            {
                Debug.LogError("[CardDataManager] CardDatabaseSO is null!");
                return;
            }
            
            _cardDataService = new CardDataService(database, _assetLoaderService);
            _deckFactoryService = new DeckFactoryService(database);
        }
        
        /// <summary>
        /// Fallback na legacy CardDatabaseSO.
        /// </summary>
        private void FallbackToLegacyDatabase()
        {
            if (cardDatabaseSO != null)
            {
                InitializeServicesWithDatabase(cardDatabaseSO);
            }
        }
        
        /// <summary>
        /// Zajistí, že jsou služby inicializované.
        /// Volá se automaticky před poskytnutím služby.
        /// </summary>
        private void EnsureInitialized()
        {
            if (_cardDataService == null && _deckFactoryService == null)
            {
                InitializeServices();
            }
        }
        
        /// <summary>
        /// Získá službu pro poskytování dat karet.
        /// </summary>
        public ICardDataProvider GetCardDataProvider()
        {
            EnsureInitialized();
            return _cardDataService;
        }
        
        /// <summary>
        /// Získá službu pro načítání Unity assetů.
        /// </summary>
        public IAssetLoader GetAssetLoader()
        {
            EnsureInitialized();
            return _assetLoaderService;
        }
        
        /// <summary>
        /// Získá službu pro vytváření balíčků.
        /// </summary>
        public IDeckFactory GetDeckFactory()
        {
            EnsureInitialized();
            return _deckFactoryService;
        }
        
        /// <summary>
        /// Získá službu pro správu témat.
        /// </summary>
        public ICardThemeProvider GetThemeProvider()
        {
            EnsureInitialized();
            return _themeService;
        }
        
        /// <summary>
        /// Nastaví aktivní téma balíčku.
        /// </summary>
        public void SetActiveTheme(CardThemeSO theme)
        {
            if (_themeService == null)
            {
                Debug.LogError("[CardDataManager] ThemeService is not initialized!");
                return;
            }
            
            _themeService.SetActiveTheme(theme);
            
            // Reinitializovat služby s novým tématem
            var activeTheme = _themeService.GetActiveTheme();
            if (activeTheme != null && activeTheme.cardDatabase != null)
            {
                InitializeServicesWithDatabase(activeTheme.cardDatabase);
            }
        }
        
        /// <summary>
        /// Získá aktuálně aktivní téma.
        /// </summary>
        public CardThemeSO GetActiveTheme()
        {
            return _themeService?.GetActiveTheme();
        }
        
        /// <summary>
        /// Získá všechna dostupná téma.
        /// </summary>
        public IReadOnlyList<CardThemeSO> GetAllThemes()
        {
            return _themeService?.GetAllThemes() ?? new List<CardThemeSO>().AsReadOnly();
        }
        
        /// <summary>
        /// Legacy: Nastaví nové CardDatabaseSO (deprecated, použijte SetActiveTheme).
        /// </summary>
        [System.Obsolete("Use SetActiveTheme instead.")]
        public void SetCardDatabaseSO(CardDatabaseSO newDatabaseSO)
        {
            if (newDatabaseSO != null)
            {
                cardDatabaseSO = newDatabaseSO;
                InitializeServicesWithDatabase(newDatabaseSO);
            }
        }
        
        /// <summary>
        /// Legacy: Získá CardDatabaseSO (deprecated, použijte GetActiveTheme).
        /// </summary>
        [System.Obsolete("Use GetActiveTheme instead.")]
        public CardDatabaseSO GetCardDatabaseSO()
        {
            var activeTheme = GetActiveTheme();
            return activeTheme != null ? activeTheme.cardDatabase : cardDatabaseSO;
        }
    }
}

