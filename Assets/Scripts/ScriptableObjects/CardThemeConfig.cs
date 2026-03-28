using UnityEngine;

namespace MariasGame.ScriptableObjects
{
    [CreateAssetMenu(fileName = "CardThemeConfig", menuName = "MariasGame/Card Theme Config", order = 7)]
    public class CardThemeConfig : ScriptableObject
    {
        [field: SerializeField] public string ThemeName { get; set; }
        [field: SerializeField] public string ThemeDescription { get; set; }
        [field: SerializeField] public Sprite ThemePreview { get; set; }
        [field: SerializeField] public bool IsUnlocked { get; set; } = true;
        [field: SerializeField] public CardDatabase CardDatabase { get; set; }
        [field: SerializeField] public Sprite CardBackSprite { get; set; }

        private void OnValidate()
        {
            if (CardDatabase == null)
                Debug.LogWarning($"[CardThemeConfig] {name}: Card database is not assigned!");
            if (string.IsNullOrEmpty(ThemeName))
                Debug.LogWarning($"[CardThemeConfig] {name}: Theme name is missing!");
            if (CardBackSprite == null)
                Debug.LogWarning($"[CardThemeConfig] {name}: Card back sprite is not assigned!");
        }
    }
}
