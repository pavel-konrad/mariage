using UnityEngine;

namespace MariasGame.ScriptableObjects
{
    /// <summary>
    /// ScriptableObject pro téma balíčku karet.
    /// Obsahuje CardDatabaseSO a metadata tématu (název, popis, preview).
    /// </summary>
    [CreateAssetMenu(fileName = "CardTheme", menuName = "MariasGame/Card Theme", order = 7)]
    public class CardThemeSO : ScriptableObject
    {
        [Header("Theme Info")]
        public string themeName;
        public string themeDescription;
        public Sprite themePreview;
        public bool isUnlocked = true; // Zda je téma odemčeno (pro DLC, achievements, atd.)
        
        [Header("Card Database")]
        public CardDatabaseSO cardDatabase;
        
        [Header("Card Back")]
        public Sprite cardBackSprite; // Rub karty pro toto téma
        
        private void OnValidate()
        {
            if (cardDatabase == null)
            {
                Debug.LogWarning($"[CardThemeSO] {name}: Card database is not assigned!");
            }
            
            if (string.IsNullOrEmpty(themeName))
            {
                Debug.LogWarning($"[CardThemeSO] {name}: Theme name is missing!");
            }
            
            if (cardBackSprite == null)
            {
                Debug.LogWarning($"[CardThemeSO] {name}: Card back sprite is not assigned! Cards will not display their backs.");
            }
        }
    }
}

