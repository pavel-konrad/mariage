using System.Collections.Generic;
using UnityEngine;
using MariasGame.ScriptableObjects;

namespace MariasGame.Core.Interfaces
{
    /// <summary>
    /// Interface pro poskytování témat balíčků karet.
    /// </summary>
    public interface ICardThemeProvider
    {
        /// <summary>
        /// Získá aktuálně aktivní téma.
        /// </summary>
        CardThemeSO GetActiveTheme();
        
        /// <summary>
        /// Nastaví aktivní téma.
        /// </summary>
        void SetActiveTheme(CardThemeSO theme);
        
        /// <summary>
        /// Získá téma podle názvu.
        /// </summary>
        CardThemeSO GetTheme(string themeName);
        
        /// <summary>
        /// Získá všechna dostupná téma.
        /// </summary>
        IReadOnlyList<CardThemeSO> GetAllThemes();
        
        /// <summary>
        /// Získá všechna odemčená téma.
        /// </summary>
        IReadOnlyList<CardThemeSO> GetUnlockedThemes();
        
        /// <summary>
        /// Získá sprite pro rub karty aktuálního tématu.
        /// </summary>
        Sprite GetCardBackSprite();
    }
}

