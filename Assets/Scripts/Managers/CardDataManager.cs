using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MariasGame.Core.Interfaces;
using MariasGame.ScriptableObjects;
using MariasGame.Services;

namespace MariasGame.Managers
{
    public class CardDataManager : MonoBehaviour
    {
        [SerializeField] private List<CardThemeConfig> availableThemes = new List<CardThemeConfig>();
        [SerializeField] private CardThemeConfig defaultTheme;

        [Header("Legacy (Deprecated)")]
        [SerializeField] private CardDatabase cardDatabase;

        private CardDataService _cardDataService;
        private AssetLoaderService _assetLoaderService;
        private DeckFactoryService _deckFactoryService;
        private CardThemeService _themeService;

        void Awake() => InitializeServices();

        private void InitializeServices()
        {
            _assetLoaderService = new AssetLoaderService();

            if (availableThemes != null && availableThemes.Count > 0)
            {
                _themeService = new CardThemeService(availableThemes, defaultTheme);
                var activeTheme = _themeService.GetActiveTheme();

                if (activeTheme != null && activeTheme.CardDatabase != null)
                    InitializeServicesWithDatabase(activeTheme.CardDatabase);
                else
                {
                    Debug.LogError("[CardDataManager] No valid theme or card database found!");
                    FallbackToLegacyDatabase();
                }
            }
            else if (cardDatabase != null)
            {
                Debug.LogWarning("[CardDataManager] Using legacy CardDatabase. Consider using CardThemeConfig instead.");
                InitializeServicesWithDatabase(cardDatabase);
            }
            else
            {
                Debug.LogError("[CardDataManager] No card database or themes assigned!");
            }
        }

        private void InitializeServicesWithDatabase(CardDatabase database)
        {
            if (database == null)
            {
                Debug.LogError("[CardDataManager] CardDatabase is null!");
                return;
            }
            _cardDataService = new CardDataService(database);
            _deckFactoryService = new DeckFactoryService(database);
        }

        private void FallbackToLegacyDatabase()
        {
            if (cardDatabase != null)
                InitializeServicesWithDatabase(cardDatabase);
        }

        private void EnsureInitialized()
        {
            if (_cardDataService == null && _deckFactoryService == null)
                InitializeServices();
        }

        public CardDataService GetCardDataService()
        {
            EnsureInitialized();
            return _cardDataService;
        }

        public AssetLoaderService GetAssetLoaderService()
        {
            EnsureInitialized();
            return _assetLoaderService;
        }

        public IDeckFactory GetDeckFactory()
        {
            EnsureInitialized();
            return _deckFactoryService;
        }

        public CardThemeService GetThemeService()
        {
            EnsureInitialized();
            return _themeService;
        }

        public void SetActiveTheme(CardThemeConfig theme)
        {
            if (_themeService == null)
            {
                Debug.LogError("[CardDataManager] ThemeService is not initialized!");
                return;
            }
            _themeService.SetActiveTheme(theme);
            var activeTheme = _themeService.GetActiveTheme();
            if (activeTheme != null && activeTheme.CardDatabase != null)
                InitializeServicesWithDatabase(activeTheme.CardDatabase);
        }

        public CardThemeConfig GetActiveTheme()
        {
            EnsureInitialized();
            return _themeService?.GetActiveTheme();
        }

        public IReadOnlyList<CardThemeConfig> GetAllThemes()
        {
            EnsureInitialized();
            return _themeService?.GetAllThemes() ?? new List<CardThemeConfig>().AsReadOnly();
        }

        [System.Obsolete("Use SetActiveTheme instead.")]
        public void SetCardDatabase(CardDatabase newDatabase)
        {
            if (newDatabase != null)
            {
                cardDatabase = newDatabase;
                InitializeServicesWithDatabase(newDatabase);
            }
        }

        [System.Obsolete("Use GetActiveTheme instead.")]
        public CardDatabase GetCardDatabase()
        {
            var activeTheme = GetActiveTheme();
            return activeTheme != null ? activeTheme.CardDatabase : cardDatabase;
        }
    }
}
