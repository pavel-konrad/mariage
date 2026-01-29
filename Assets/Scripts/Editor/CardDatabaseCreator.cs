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
    /// Editor tool pro vytváření CardDatabaseSO s kompletním balíčkem 32 karet.
    /// Vytvoří CardDataSO pro každou kartu a CardDatabaseSO, který je obsahuje.
    /// </summary>
    public class CardDatabaseCreator : EditorWindow
    {
        private string databaseName = "Standard32";
        private string themeName = "Standard Theme";
        private string outputPath = "Assets/ScriptableObjects/Cards";
        private string spriteResourcePath = "Cards"; // Cesta v Resources folder
        private string cardBackSpritePath = "Cards/CardBack"; // Cesta k rubu karty v Resources
        private bool autoLoadSprites = true;
        private bool createTheme = true; // Zda vytvořit CardThemeSO současně
        
        [MenuItem("MariasGame/Tools/Create New Card Database")]
        public static void ShowWindow()
        {
            GetWindow<CardDatabaseCreator>("Card Database Creator");
        }
        
        /// <summary>
        /// Rychlé vytvoření CardDatabaseSO s defaultními hodnotami.
        /// Vytvoří databázi v Assets/ScriptableObjects/Cards.
        /// </summary>
        [MenuItem("MariasGame/Tools/Create Card Database (Quick)")]
        public static void CreateCardDatabaseQuick()
        {
            CreateCardDatabaseQuick(false);
        }
        
        /// <summary>
        /// Rychlé vytvoření CardDatabaseSO a CardThemeSO s defaultními hodnotami.
        /// Vytvoří databázi a theme v Assets/ScriptableObjects/Cards.
        /// </summary>
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
            
            // Vytvořit output folder pokud neexistuje
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
                AssetDatabase.Refresh();
            }
            
            // Vytvořit seznam všech karet
            var cardDataList = new List<CardDataSO>();
            var allSuits = System.Enum.GetValues(typeof(CardSuit)).Cast<CardSuit>().ToArray();
            var allRanks = System.Enum.GetValues(typeof(CardRank)).Cast<CardRank>().ToArray();
            
            int createdCount = 0;
            int spriteLoadedCount = 0;
            
            foreach (var suit in allSuits)
            {
                foreach (var rank in allRanks)
                {
                    var cardData = CreateCardDataSOStatic(suit, rank, outputPath);
                    if (cardData != null)
                    {
                        cardDataList.Add(cardData);
                        createdCount++;
                        
                        if (LoadCardSpriteStatic(cardData, suit, rank, spriteResourcePath))
                        {
                            spriteLoadedCount++;
                        }
                    }
                }
            }
            
            // Vytvořit CardDatabaseSO
            var database = CreateCardDatabaseSOStatic(databaseName, cardDataList, outputPath);
            
            // Vytvořit CardThemeSO pokud je požadováno
            CardThemeSO theme = null;
            if (createTheme)
            {
                theme = CreateCardThemeSOStatic(themeName, database, outputPath, cardBackSpritePath);
            }
            
            // Uložit změny
            EditorUtility.SetDirty(database);
            if (theme != null)
            {
                EditorUtility.SetDirty(theme);
            }
            foreach (var cardData in cardDataList)
            {
                EditorUtility.SetDirty(cardData);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            string logMessage = $"[CardDatabaseCreator] Created CardDatabase '{databaseName}' with {createdCount} cards. " +
                $"{spriteLoadedCount}/{createdCount} sprites loaded from Resources/{spriteResourcePath}.";
            
            if (createTheme && theme != null)
            {
                logMessage += $"\n[CardDatabaseCreator] Created CardTheme '{themeName}'.";
                if (theme.cardBackSprite == null)
                {
                    logMessage += $" Warning: Card back sprite not found at Resources/{cardBackSpritePath}.";
                }
            }
            
            Debug.Log(logMessage);
            
            // Zvýraznit vytvořený database nebo theme
            Selection.activeObject = theme != null ? theme : database;
            EditorGUIUtility.PingObject(Selection.activeObject);
        }
        
        private static CardDataSO CreateCardDataSOStatic(CardSuit suit, CardRank rank, string outputPath)
        {
            string fileName = $"{suit}_{rank}.asset";
            string assetPath = Path.Combine(outputPath, fileName).Replace('\\', '/');
            
            if (File.Exists(assetPath))
            {
                var existing = AssetDatabase.LoadAssetAtPath<CardDataSO>(assetPath);
                if (existing != null)
                {
                    return existing;
                }
            }
            
            var cardData = ScriptableObject.CreateInstance<CardDataSO>();
            cardData.suit = suit;
            cardData.rank = rank;
            cardData.isInGame = true;
            cardData.name = $"{suit} {rank}";
            
            AssetDatabase.CreateAsset(cardData, assetPath);
            
            return cardData;
        }
        
        private static bool LoadCardSpriteStatic(CardDataSO cardData, CardSuit suit, CardRank rank, string spriteResourcePath)
        {
            string spritePath = $"{spriteResourcePath}/{suit}_{rank}";
            Sprite sprite = Resources.Load<Sprite>(spritePath);
            
            if (sprite != null)
            {
                cardData.cardSprite = sprite;
                EditorUtility.SetDirty(cardData);
                return true;
            }
            
            return false;
        }
        
        private static CardDatabaseSO CreateCardDatabaseSOStatic(string databaseName, List<CardDataSO> cardDataList, string outputPath)
        {
            string fileName = $"{databaseName}_Database.asset";
            string assetPath = Path.Combine(outputPath, fileName).Replace('\\', '/');
            
            CardDatabaseSO database = AssetDatabase.LoadAssetAtPath<CardDatabaseSO>(assetPath);
            
            if (database == null)
            {
                database = ScriptableObject.CreateInstance<CardDatabaseSO>();
                database.gameName = "Marias";
                database.deckName = databaseName;
                AssetDatabase.CreateAsset(database, assetPath);
            }
            else
            {
                database.deckName = databaseName;
            }
            
            database.cards = cardDataList;
            
            return database;
        }
        
        void OnGUI()
        {
            GUILayout.Label("Card Database Creator", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
            EditorGUILayout.HelpBox("Tento nástroj vytvoří CardDatabaseSO s kompletním balíčkem 32 karet (8 hodnot × 4 barvy). " +
                "Pro každou kartu vytvoří CardDataSO asset. Volitelně vytvoří také CardThemeSO.", MessageType.Info);
            
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
            {
                CreateCardDatabase();
            }
            
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
            
            // Vytvořit output folder pokud neexistuje
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
                AssetDatabase.Refresh();
            }
            
            // Vytvořit seznam všech karet (32 karet: 8 hodnot × 4 barvy)
            var cardDataList = new List<CardDataSO>();
            var allSuits = System.Enum.GetValues(typeof(CardSuit)).Cast<CardSuit>().ToArray();
            var allRanks = System.Enum.GetValues(typeof(CardRank)).Cast<CardRank>().ToArray();
            
            int createdCount = 0;
            int spriteLoadedCount = 0;
            
            foreach (var suit in allSuits)
            {
                foreach (var rank in allRanks)
                {
                    // Vytvořit CardDataSO
                    var cardData = CreateCardDataSOStatic(suit, rank, outputPath);
                    if (cardData != null)
                    {
                        cardDataList.Add(cardData);
                        createdCount++;
                        
                        // Načíst sprite pokud je auto-load zapnutý
                        if (autoLoadSprites)
                        {
                            if (LoadCardSpriteStatic(cardData, suit, rank, spriteResourcePath))
                            {
                                spriteLoadedCount++;
                            }
                        }
                    }
                }
            }
            
            // Vytvořit CardDatabaseSO
            var database = CreateCardDatabaseSOStatic(databaseName, cardDataList, outputPath);
            
            // Vytvořit CardThemeSO pokud je požadováno
            CardThemeSO theme = null;
            if (createTheme)
            {
                theme = CreateCardThemeSOStatic(themeName, database, outputPath, cardBackSpritePath);
            }
            
            // Uložit změny
            EditorUtility.SetDirty(database);
            if (theme != null)
            {
                EditorUtility.SetDirty(theme);
            }
            foreach (var cardData in cardDataList)
            {
                EditorUtility.SetDirty(cardData);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            // Zobrazit výsledek
            string message = $"Card Database created successfully!\n\n" +
                $"Created: {createdCount} CardDataSO assets\n" +
                $"Sprites loaded: {spriteLoadedCount}/{createdCount}\n" +
                $"Database: {database.name}\n" +
                $"Location: {outputPath}";
            
            if (createTheme && theme != null)
            {
                message += $"\n\nTheme: {theme.name}";
                if (theme.cardBackSprite == null)
                {
                    message += $"\nWarning: Card back sprite not found at Resources/{cardBackSpritePath}";
                }
            }
            
            if (spriteLoadedCount < createdCount && autoLoadSprites)
            {
                message += $"\n\nWarning: {createdCount - spriteLoadedCount} sprites were not found in Resources/{spriteResourcePath}";
            }
            
            EditorUtility.DisplayDialog("Success", message, "OK");
            
            // Zvýraznit vytvořený database nebo theme v Project window
            Selection.activeObject = theme != null ? theme : database;
            EditorGUIUtility.PingObject(Selection.activeObject);
        }
        
        private static CardThemeSO CreateCardThemeSOStatic(string themeName, CardDatabaseSO database, string outputPath, string cardBackSpritePath)
        {
            string fileName = $"{themeName.Replace(" ", "_")}_Theme.asset";
            string assetPath = Path.Combine(outputPath, fileName).Replace('\\', '/');
            
            CardThemeSO theme = AssetDatabase.LoadAssetAtPath<CardThemeSO>(assetPath);
            
            if (theme == null)
            {
                theme = ScriptableObject.CreateInstance<CardThemeSO>();
                theme.themeName = themeName;
                theme.themeDescription = $"Theme for {database.deckName} card deck";
                theme.isUnlocked = true;
                AssetDatabase.CreateAsset(theme, assetPath);
            }
            else
            {
                theme.themeName = themeName;
            }
            
            // Nastavit databázi
            theme.cardDatabase = database;
            
            // Načíst card back sprite
            if (!string.IsNullOrEmpty(cardBackSpritePath))
            {
                Sprite cardBackSprite = Resources.Load<Sprite>(cardBackSpritePath);
                if (cardBackSprite != null)
                {
                    theme.cardBackSprite = cardBackSprite;
                }
            }
            
            return theme;
        }
        
    }
}

