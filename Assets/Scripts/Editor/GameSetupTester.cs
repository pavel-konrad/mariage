using UnityEngine;
using UnityEditor;
using MariasGame.Core;
using MariasGame.Core.Interfaces;
using MariasGame.Managers;
using MariasGame.ScriptableObjects;

namespace MariasGame.Editor
{
    /// <summary>
    /// Testovací tool pro ověření správné funkčnosti SOLID refaktoringu.
    /// </summary>
    public class GameSetupTester : EditorWindow
    {
        private GameSettingsManager settingsManager;
        private CardDataManager cardDataManager;
        private CharacterManager characterManager;
        
        [MenuItem("MariasGame/Tools/Game Setup Tester")]
        public static void ShowWindow()
        {
            GetWindow<GameSetupTester>("Game Setup Tester");
        }
        
        void OnGUI()
        {
            GUILayout.Label("Game Setup Tester", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
            settingsManager = (GameSettingsManager)EditorGUILayout.ObjectField("GameSettingsManager", settingsManager, typeof(GameSettingsManager), true);
            cardDataManager = (CardDataManager)EditorGUILayout.ObjectField("CardDataManager", cardDataManager, typeof(CardDataManager), true);
            characterManager = (CharacterManager)EditorGUILayout.ObjectField("CharacterManager", characterManager, typeof(CharacterManager), true);
            
            GUILayout.Space(10);
            
            // Zobrazit status
            if (cardDataManager != null)
            {
                var cardDatabase = cardDataManager.GetActiveTheme()?.CardDatabase;
                var themes = cardDataManager.GetAllThemes();
                
                if (cardDatabase == null && (themes == null || themes.Count == 0))
                {
                    EditorGUILayout.HelpBox("⚠ CardDataManager needs CardDatabaseSO or CardThemeSO assigned!", MessageType.Warning);
                }
                else
                {
                    EditorGUILayout.HelpBox("✓ CardDataManager is configured", MessageType.Info);
                }
            }
            
            if (settingsManager != null)
            {
                EditorGUILayout.HelpBox("✓ GameSettingsManager is assigned", MessageType.Info);
            }
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("Test Deck Creation"))
            {
                TestDeckCreation();
            }
            
            if (GUILayout.Button("Test Player Creation"))
            {
                TestPlayerCreation();
            }
            
            if (GUILayout.Button("Test Card Data"))
            {
                TestCardData();
            }
            
            if (GUILayout.Button("Test All"))
            {
                TestAll();
            }
        }
        
        private void TestDeckCreation()
        {
            if (cardDataManager == null)
            {
                Debug.LogError("CardDataManager is not assigned!");
                EditorUtility.DisplayDialog("Error", "CardDataManager is not assigned! Please assign it in the tester window.", "OK");
                return;
            }
            
            // Zkontrolovat, zda má CardDataManager přiřazené CardDatabaseSO nebo CardThemeSO
            var cardDatabase = cardDataManager.GetActiveTheme()?.CardDatabase;
            var themes = cardDataManager.GetAllThemes();
            
            if (cardDatabase == null && (themes == null || themes.Count == 0))
            {
                Debug.LogError("CardDataManager has no CardDatabaseSO or CardThemeSO assigned! Please assign at least one in the Inspector.");
                EditorUtility.DisplayDialog("Error", 
                    "CardDataManager has no CardDatabaseSO or CardThemeSO assigned!\n\n" +
                    "Please:\n" +
                    "1. Assign CardDatabaseSO in the Legacy field, OR\n" +
                    "2. Add CardThemeSO(s) to Available Themes field\n" +
                    "3. Set Default Theme", 
                    "OK");
                return;
            }
            
            var deckFactory = cardDataManager.GetDeckFactory();
            if (deckFactory == null)
            {
                Debug.LogError("DeckFactory is not available! Check CardDataManager initialization and assigned ScriptableObjects.");
                EditorUtility.DisplayDialog("Error", "DeckFactory is not available! Check that CardDatabaseSO or CardThemeSO is properly assigned.", "OK");
                return;
            }
            
            var deck = deckFactory.CreateStandardDeck();
            Debug.Log($"✓ Deck created: {deck.Count} cards");
            
            if (deck is IShuffleable shuffleable)
            {
                shuffleable.Shuffle();
                Debug.Log($"✓ Deck shuffled");
            }
        }
        
        private void TestPlayerCreation()
        {
            if (settingsManager == null)
            {
                Debug.LogError("GameSettingsManager is not assigned!");
                return;
            }

            var service = characterManager?.GetService();
            var characters = service?.GetAll();
            if (characters == null || characters.Count == 0)
            {
                Debug.LogWarning("No characters in CharacterDatabase.");
                return;
            }

            Debug.Log($"✓ Characters loaded: {characters.Count}");
            foreach (var c in characters)
                Debug.Log($"  - [{c.CharacterId}] {c.CharacterName}");
        }
        
        private void TestCardData()
        {
            if (cardDataManager == null)
            {
                Debug.LogError("CardDataManager is not assigned!");
                EditorUtility.DisplayDialog("Error", "CardDataManager is not assigned! Please assign it in the tester window.", "OK");
                return;
            }
            
            // Zkontrolovat, zda má CardDataManager přiřazené CardDatabaseSO nebo CardThemeSO
            var cardDatabase = cardDataManager.GetActiveTheme()?.CardDatabase;
            var themes = cardDataManager.GetAllThemes();
            
            if (cardDatabase == null && (themes == null || themes.Count == 0))
            {
                Debug.LogError("CardDataManager has no CardDatabaseSO or CardThemeSO assigned! Please assign at least one in the Inspector.");
                EditorUtility.DisplayDialog("Error", 
                    "CardDataManager has no CardDatabaseSO or CardThemeSO assigned!\n\n" +
                    "Please:\n" +
                    "1. Assign CardDatabaseSO in the Legacy field, OR\n" +
                    "2. Add CardThemeSO(s) to Available Themes field\n" +
                    "3. Set Default Theme", 
                    "OK");
                return;
            }
            
            var cardDataService = cardDataManager.GetCardDataService();
            if (cardDataService == null)
            {
                Debug.LogError("CardDataService is not available! Check CardDataManager initialization and assigned ScriptableObjects.");
                EditorUtility.DisplayDialog("Error", "CardDataService is not available! Check that CardDatabaseSO or CardThemeSO is properly assigned.", "OK");
                return;
            }

            var sprite = cardDataService.GetCardSprite(CardSuit.Hearts, CardRank.Ace);
            if (sprite == null)
            {
                Debug.LogWarning("Card sprite is null. This might mean the card doesn't exist in the database or sprites are not assigned.");
            }
            else
            {
                Debug.Log($"✓ Card sprite loaded: {sprite.name}");
            }
        }
        
        private void TestAll()
        {
            Debug.Log("=== Starting Game Setup Tests ===");
            TestDeckCreation();
            TestPlayerCreation();
            TestCardData();
            Debug.Log("=== Tests Complete ===");
        }
    }
}

