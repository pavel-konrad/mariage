using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using MariasGame.Core;
using MariasGame.ScriptableObjects;
using System.Collections.Generic;

namespace MariasGame.Editor
{
    /// <summary>
    /// Editor tool pro vytváření CardDatabase s kompletním balíčkem 32 karet.
    /// Vytvoří CardData pro každou kartu a CardDatabase, který je obsahuje.
    /// </summary>
    public class CardDatabaseCreator : EditorWindow
    {
        private string databaseName = "Standard32";
        private string themeName = "Standard Theme";
        private string outputPath = "Assets/ScriptableObjects/Cards";
        private string spriteResourcePath = "Cards";
        private string cardBackSpritePath = "Cards/CardBack";
        private bool autoLoadSprites = true;
        private bool createTheme = true;

        [MenuItem("MariasGame/Tools/Create New Card Database")]
        public static void ShowWindow()
        {
            GetWindow<CardDatabaseCreator>("Card Database Creator");
        }

        [MenuItem("MariasGame/Tools/Create Card Database (Quick)")]
        public static void CreateCardDatabaseQuick()
        {
            CreateCardDatabaseQuick(false);
        }

        [MenuItem("MariasGame/Tools/Create New Theme (Database + Theme)")]
        public static void CreateCardThemeQuick()
        {
            CreateCardDatabaseQuick(true);
        }

        private static void CreateCardDatabaseQuick(bool createTheme)
        {
            string outputPath = "Assets/ScriptableObjects/Cards";
            string databaseName = "Standard32";
            string themeName = "Standard Theme";
            string spriteResourcePath = "Cards";
            string cardBackSpritePath = "Cards/CardBack";

            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
                AssetDatabase.Refresh();
            }

            var cardDataList = new List<CardData>();
            var allSuits = System.Enum.GetValues(typeof(CardSuit)).Cast<CardSuit>().ToArray();
            var allRanks = System.Enum.GetValues(typeof(CardRank)).Cast<CardRank>().ToArray();

            int createdCount = 0;
            int spriteLoadedCount = 0;

            foreach (var suit in allSuits)
            {
                foreach (var rank in allRanks)
                {
                    var cardData = CreateCardDataStatic(suit, rank, outputPath);
                    if (cardData != null)
                    {
                        cardDataList.Add(cardData);
                        createdCount++;
                        if (LoadCardSpriteStatic(cardData, suit, rank, spriteResourcePath))
                            spriteLoadedCount++;
                    }
                }
            }

            var database = CreateCardDatabaseStatic(databaseName, cardDataList, outputPath);

            CardThemeConfig theme = null;
            if (createTheme)
                theme = CreateCardThemeConfigStatic(themeName, database, outputPath, cardBackSpritePath);

            EditorUtility.SetDirty(database);
            if (theme != null) EditorUtility.SetDirty(theme);
            foreach (var cardData in cardDataList) EditorUtility.SetDirty(cardData);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            string logMessage = $"[CardDatabaseCreator] Created CardDatabase '{databaseName}' with {createdCount} cards. " +
                $"{spriteLoadedCount}/{createdCount} sprites loaded from Resources/{spriteResourcePath}.";

            if (createTheme && theme != null)
            {
                logMessage += $"\n[CardDatabaseCreator] Created CardTheme '{themeName}'.";
                if (theme.CardBackSprite == null)
                    logMessage += $" Warning: Card back sprite not found at Resources/{cardBackSpritePath}.";
            }

            Debug.Log(logMessage);
            Selection.activeObject = theme != null ? (Object)theme : database;
            EditorGUIUtility.PingObject(Selection.activeObject);
        }

        private static CardData CreateCardDataStatic(CardSuit suit, CardRank rank, string outputPath)
        {
            string fileName = $"{suit}_{rank}.asset";
            string assetPath = Path.Combine(outputPath, fileName).Replace('\\', '/');

            if (File.Exists(assetPath))
            {
                var existing = AssetDatabase.LoadAssetAtPath<CardData>(assetPath);
                if (existing != null) return existing;
            }

            var cardData = ScriptableObject.CreateInstance<CardData>();
            cardData.Suit = suit;
            cardData.Rank = rank;
            cardData.IsInGame = true;
            cardData.name = $"{suit} {rank}";

            AssetDatabase.CreateAsset(cardData, assetPath);
            return cardData;
        }

        private static bool LoadCardSpriteStatic(CardData cardData, CardSuit suit, CardRank rank, string spriteResourcePath)
        {
            string spritePath = $"{spriteResourcePath}/{suit}_{rank}";
            Sprite sprite = Resources.Load<Sprite>(spritePath);

            if (sprite != null)
            {
                cardData.CardSprite = sprite;
                EditorUtility.SetDirty(cardData);
                return true;
            }

            return false;
        }

        private static CardDatabase CreateCardDatabaseStatic(string databaseName, List<CardData> cardDataList, string outputPath)
        {
            string fileName = $"{databaseName}_Database.asset";
            string assetPath = Path.Combine(outputPath, fileName).Replace('\\', '/');

            CardDatabase database = AssetDatabase.LoadAssetAtPath<CardDatabase>(assetPath);

            if (database == null)
            {
                database = ScriptableObject.CreateInstance<CardDatabase>();
                database.GameName = "Marias";
                database.DeckName = databaseName;
                AssetDatabase.CreateAsset(database, assetPath);
            }
            else
            {
                database.DeckName = databaseName;
            }

            database.Cards = cardDataList;
            return database;
        }

        void OnGUI()
        {
            GUILayout.Label("Card Database Creator", EditorStyles.boldLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox("Tento nástroj vytvoří CardDatabase s kompletním balíčkem 32 karet (8 hodnot × 4 barvy). " +
                "Pro každou kartu vytvoří CardData asset. Volitelně vytvoří také CardThemeConfig.", MessageType.Info);

            GUILayout.Space(10);

            databaseName = EditorGUILayout.TextField("Database Name", databaseName);
            createTheme = EditorGUILayout.Toggle("Create Card Theme", createTheme);

            if (createTheme)
            {
                themeName = EditorGUILayout.TextField("Theme Name", themeName);
                cardBackSpritePath = EditorGUILayout.TextField("Card Back Sprite Path", cardBackSpritePath);
            }

            GUILayout.Space(5);
            outputPath = EditorGUILayout.TextField("Output Path", outputPath);
            spriteResourcePath = EditorGUILayout.TextField("Sprite Resource Path", spriteResourcePath);
            autoLoadSprites = EditorGUILayout.Toggle("Auto-load Sprites from Resources", autoLoadSprites);

            GUILayout.Space(10);

            if (GUILayout.Button("Create Card Database", GUILayout.Height(30)))
                CreateCardDatabase();

            GUILayout.Space(10);

            EditorGUILayout.HelpBox("Poznámka: Sprites by měly být v Resources/" + spriteResourcePath + " ve formátu: {Suit}_{Rank} (např. Hearts_Seven, Diamonds_King)",
                MessageType.Info);
        }

        private void CreateCardDatabase()
        {
            if (string.IsNullOrEmpty(databaseName))
            {
                EditorUtility.DisplayDialog("Error", "Database name cannot be empty!", "OK");
                return;
            }

            if (string.IsNullOrEmpty(outputPath))
            {
                EditorUtility.DisplayDialog("Error", "Output path cannot be empty!", "OK");
                return;
            }

            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
                AssetDatabase.Refresh();
            }

            var cardDataList = new List<CardData>();
            var allSuits = System.Enum.GetValues(typeof(CardSuit)).Cast<CardSuit>().ToArray();
            var allRanks = System.Enum.GetValues(typeof(CardRank)).Cast<CardRank>().ToArray();

            int createdCount = 0;
            int spriteLoadedCount = 0;

            foreach (var suit in allSuits)
            {
                foreach (var rank in allRanks)
                {
                    var cardData = CreateCardDataStatic(suit, rank, outputPath);
                    if (cardData != null)
                    {
                        cardDataList.Add(cardData);
                        createdCount++;
                        if (autoLoadSprites && LoadCardSpriteStatic(cardData, suit, rank, spriteResourcePath))
                            spriteLoadedCount++;
                    }
                }
            }

            var database = CreateCardDatabaseStatic(databaseName, cardDataList, outputPath);

            CardThemeConfig theme = null;
            if (createTheme)
                theme = CreateCardThemeConfigStatic(themeName, database, outputPath, cardBackSpritePath);

            EditorUtility.SetDirty(database);
            if (theme != null) EditorUtility.SetDirty(theme);
            foreach (var cardData in cardDataList) EditorUtility.SetDirty(cardData);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            string message = $"Card Database created successfully!\n\n" +
                $"Created: {createdCount} CardData assets\n" +
                $"Sprites loaded: {spriteLoadedCount}/{createdCount}\n" +
                $"Database: {database.name}\n" +
                $"Location: {outputPath}";

            if (createTheme && theme != null)
            {
                message += $"\n\nTheme: {theme.name}";
                if (theme.CardBackSprite == null)
                    message += $"\nWarning: Card back sprite not found at Resources/{cardBackSpritePath}";
            }

            if (spriteLoadedCount < createdCount && autoLoadSprites)
                message += $"\n\nWarning: {createdCount - spriteLoadedCount} sprites were not found in Resources/{spriteResourcePath}";

            EditorUtility.DisplayDialog("Success", message, "OK");

            Selection.activeObject = theme != null ? (Object)theme : database;
            EditorGUIUtility.PingObject(Selection.activeObject);
        }

        private static CardThemeConfig CreateCardThemeConfigStatic(string themeName, CardDatabase database, string outputPath, string cardBackSpritePath)
        {
            string fileName = $"{themeName.Replace(" ", "_")}_Theme.asset";
            string assetPath = Path.Combine(outputPath, fileName).Replace('\\', '/');

            CardThemeConfig theme = AssetDatabase.LoadAssetAtPath<CardThemeConfig>(assetPath);

            if (theme == null)
            {
                theme = ScriptableObject.CreateInstance<CardThemeConfig>();
                theme.ThemeName = themeName;
                theme.ThemeDescription = $"Theme for {database.DeckName} card deck";
                theme.IsUnlocked = true;
                AssetDatabase.CreateAsset(theme, assetPath);
            }
            else
            {
                theme.ThemeName = themeName;
            }

            theme.CardDatabase = database;

            if (!string.IsNullOrEmpty(cardBackSpritePath))
            {
                Sprite cardBackSprite = Resources.Load<Sprite>(cardBackSpritePath);
                if (cardBackSprite != null)
                    theme.CardBackSprite = cardBackSprite;
            }

            return theme;
        }
    }
}
