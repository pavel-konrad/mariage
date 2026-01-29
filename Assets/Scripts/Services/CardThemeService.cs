using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MariasGame.Core.Interfaces;
using MariasGame.ScriptableObjects;

namespace MariasGame.Services
{
    /// <summary>
    /// Služba pro správu témat balíčků karet.
    /// Poskytuje možnost přepínat mezi různými tématy.
    /// </summary>
    public class CardThemeService : ICardThemeProvider
    {
        private readonly List<CardThemeSO> _themes;
        private CardThemeSO _activeTheme;
        private readonly string _saveKey = "CardTheme";
        
        public CardThemeService(List<CardThemeSO> themes, CardThemeSO defaultTheme = null)
        {
            _themes = themes ?? new List<CardThemeSO>();
            
            // Načíst uložené téma nebo použít defaultní
            _activeTheme = LoadSavedTheme() ?? defaultTheme ?? (_themes.Count > 0 ? _themes[0] : null);
        }
        
        /// <summary>
        /// Získá aktuálně aktivní téma.
        /// </summary>
        public CardThemeSO GetActiveTheme()
        {
            return _activeTheme;
        }
        
        /// <summary>
        /// Nastaví aktivní téma.
        /// </summary>
        public void SetActiveTheme(CardThemeSO theme)
        {
            if (theme == null)
            {
                Debug.LogWarning("[CardThemeService] Cannot set null theme!");
                return;
            }
            
            if (!_themes.Contains(theme))
            {
                Debug.LogWarning($"[CardThemeService] Theme '{theme.themeName}' is not in the themes list!");
                return;
            }
            
            if (!theme.isUnlocked)
            {
                Debug.LogWarning($"[CardThemeService] Theme '{theme.themeName}' is locked!");
                return;
            }
            
            _activeTheme = theme;
            SaveTheme(theme);
        }
        
        /// <summary>
        /// Získá téma podle názvu.
        /// </summary>
        public CardThemeSO GetTheme(string themeName)
        {
            return _themes.FirstOrDefault(t => t != null && t.themeName == themeName);
        }
        
        /// <summary>
        /// Získá všechna dostupná téma.
        /// </summary>
        public IReadOnlyList<CardThemeSO> GetAllThemes()
        {
            return _themes.Where(t => t != null).ToList().AsReadOnly();
        }
        
        /// <summary>
        /// Získá všechna odemčená téma.
        /// </summary>
        public IReadOnlyList<CardThemeSO> GetUnlockedThemes()
        {
            return _themes.Where(t => t != null && t.isUnlocked).ToList().AsReadOnly();
        }
        
        /// <summary>
        /// Uloží vybrané téma do PlayerPrefs.
        /// </summary>
        private void SaveTheme(CardThemeSO theme)
        {
            if (theme == null) return;
            
            PlayerPrefs.SetString(_saveKey, theme.themeName);
            PlayerPrefs.Save();
        }
        
        /// <summary>
        /// Načte uložené téma z PlayerPrefs.
        /// </summary>
        private CardThemeSO LoadSavedTheme()
        {
            if (!PlayerPrefs.HasKey(_saveKey))
                return null;
                
            string themeName = PlayerPrefs.GetString(_saveKey);
            return GetTheme(themeName);
        }
        
        /// <summary>
        /// Získá sprite pro rub karty aktuálního tématu.
        /// </summary>
        public Sprite GetCardBackSprite()
        {
            if (_activeTheme == null)
            {
                Debug.LogWarning("[CardThemeService] No active theme! Cannot get card back sprite.");
                return null;
            }
            
            return _activeTheme.cardBackSprite;
        }
    }
}

