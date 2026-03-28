using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MariasGame.ScriptableObjects;

namespace MariasGame.Services
{
    public class CardThemeService
    {
        private readonly List<CardThemeConfig> _themes;
        private CardThemeConfig _activeTheme;
        private readonly string _saveKey = "CardTheme";

        public CardThemeService(List<CardThemeConfig> themes, CardThemeConfig defaultTheme = null)
        {
            _themes = themes ?? new List<CardThemeConfig>();
            _activeTheme = LoadSavedTheme() ?? defaultTheme ?? (_themes.Count > 0 ? _themes[0] : null);
        }

        public CardThemeConfig GetActiveTheme() => _activeTheme;

        public void SetActiveTheme(CardThemeConfig theme)
        {
            if (theme == null)
            {
                Debug.LogWarning("[CardThemeService] Cannot set null theme!");
                return;
            }
            if (!_themes.Contains(theme))
            {
                Debug.LogWarning($"[CardThemeService] Theme '{theme.ThemeName}' is not in the themes list!");
                return;
            }
            if (!theme.IsUnlocked)
            {
                Debug.LogWarning($"[CardThemeService] Theme '{theme.ThemeName}' is locked!");
                return;
            }
            _activeTheme = theme;
            SaveTheme(theme);
        }

        public CardThemeConfig GetTheme(string themeName)
            => _themes.FirstOrDefault(t => t != null && t.ThemeName == themeName);

        public IReadOnlyList<CardThemeConfig> GetAllThemes()
            => _themes.Where(t => t != null).ToList().AsReadOnly();

        public IReadOnlyList<CardThemeConfig> GetUnlockedThemes()
            => _themes.Where(t => t != null && t.IsUnlocked).ToList().AsReadOnly();

        public Sprite GetCardBackSprite()
        {
            if (_activeTheme == null)
            {
                Debug.LogWarning("[CardThemeService] No active theme! Cannot get card back sprite.");
                return null;
            }
            return _activeTheme.CardBackSprite;
        }

        private void SaveTheme(CardThemeConfig theme)
        {
            if (theme == null) return;
            PlayerPrefs.SetString(_saveKey, theme.ThemeName);
            PlayerPrefs.Save();
        }

        private CardThemeConfig LoadSavedTheme()
        {
            if (!PlayerPrefs.HasKey(_saveKey)) return null;
            return GetTheme(PlayerPrefs.GetString(_saveKey));
        }
    }
}
