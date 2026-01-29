using System.Collections.Generic;
using System.Linq;
using MariasGame.Core;
using MariasGame.Core.Interfaces;
using MariasGame.Game.Strategies;

namespace MariasGame.Game
{
    /// <summary>
    /// Factory pro vytváření hráčů.
    /// Centralizovaná logika pro vytváření Human a AI hráčů.
    /// Používá dependency injection pro ISettingsProvider a IAvatarProvider.
    /// </summary>
    public static class PlayerFactory
    {
        /// <summary>
        /// Vytvoří lidského hráče s nastavením z ISettingsProvider.
        /// </summary>
        public static HumanPlayer CreateHumanPlayer(
            int id, 
            string name, 
            ISettingsProvider settingsProvider, 
            IAvatarProvider avatarProvider = null,
            int avatarIndex = 0)
        {
            var settings = settingsProvider?.GetSettings() ?? GameSettings.CreateDefault();
            var player = new HumanPlayer(id, name, settings.startingCash, avatarIndex);
            
            // Nastavit avatar pokud je dostupný
            if (avatarProvider != null && avatarIndex >= 0 && avatarIndex < avatarProvider.AvatarCount)
            {
                // Avatar je už nastaven v konstruktoru
            }
            
            return player;
        }
        
        /// <summary>
        /// Vytvoří lidského hráče s vlastním nastavením.
        /// </summary>
        public static HumanPlayer CreateHumanPlayer(int id, string name, int startingCash, int avatarIndex = 0)
        {
            return new HumanPlayer(id, name, startingCash, avatarIndex);
        }
        
        /// <summary>
        /// Vytvoří AI hráče s nastavením z ISettingsProvider.
        /// </summary>
        public static AIPlayer CreateAIPlayer(
            int id, 
            string name, 
            ISettingsProvider settingsProvider, 
            IAIStrategy strategy = null,
            IAvatarProvider avatarProvider = null,
            int avatarIndex = 0)
        {
            var settings = settingsProvider?.GetSettings() ?? GameSettings.CreateDefault();
            
            // Zkusit načíst jméno z avatar dat, pokud není poskytnuto
            string aiName = name;
            if (string.IsNullOrEmpty(aiName) && avatarProvider != null && avatarIndex >= 0 && avatarIndex < avatarProvider.AvatarCount)
            {
                var avatarData = avatarProvider.GetAvatar(avatarIndex);
                if (avatarData != null && !string.IsNullOrEmpty(avatarData.AvatarName))
                {
                    aiName = avatarData.AvatarName;
                }
            }
            
            // Fallback na defaultní jméno
            if (string.IsNullOrEmpty(aiName))
            {
                aiName = $"AI Player {id}";
            }
            
            // Pokud není poskytnuta strategie, vytvoř ji na základě nastavení
            if (strategy == null)
            {
                strategy = CreateStrategyForDifficulty(settings.aiDifficultyLevel);
            }
            
            var player = new AIPlayer(id, aiName, strategy, settings.startingCash, avatarIndex);
            
            return player;
        }
        
        /// <summary>
        /// Vytvoří AI hráče s vlastním nastavením.
        /// </summary>
        public static AIPlayer CreateAIPlayer(
            int id, 
            string name, 
            IAIStrategy strategy, 
            int startingCash, 
            int avatarIndex = 0)
        {
            string aiName = name ?? $"AI Player {id}";
            return new AIPlayer(id, aiName, strategy, startingCash, avatarIndex);
        }
        
        /// <summary>
        /// Vytvoří seznam hráčů pro hru s nastavením z ISettingsProvider.
        /// Lidský hráč si vybere avatar a jméno na začátku (používá IAvatarProvider).
        /// AI hráči (nepřátelé) jsou vybíráni náhodně z EnemyDatabase (používá IEnemyProvider).
        /// </summary>
        public static List<IPlayer> CreateGamePlayers(
            ISettingsProvider settingsProvider,
            IAvatarProvider avatarProvider = null,
            IEnemyProvider enemyProvider = null,
            int humanCount = 1,
            int aiCount = 2)
        {
            var players = new List<IPlayer>();
            int playerId = 1;
            
            // Načíst nepřátele z databáze (pro AI hráče)
            List<EnemyDataWithIndex> availableEnemies = new List<EnemyDataWithIndex>();
            if (enemyProvider != null && enemyProvider.EnemyCount > 0)
            {
                var allEnemies = enemyProvider.GetAllEnemies();
                for (int i = 0; i < allEnemies.Count; i++)
                {
                    if (allEnemies[i] != null)
                    {
                        availableEnemies.Add(new EnemyDataWithIndex
                        {
                            EnemyData = allEnemies[i],
                            Index = i
                        });
                    }
                }
                
                // Zamíchat nepřátele pro náhodný výběr
                availableEnemies = ShuffleList(availableEnemies);
            }
            
            // Zkontrolovat, zda máme dostatek nepřátel
            if (availableEnemies.Count < aiCount)
            {
                UnityEngine.Debug.LogWarning($"[PlayerFactory] Not enough enemies in database! Need {aiCount}, but only {availableEnemies.Count} available. Some AI players will have duplicate enemies.");
            }
            
            // Vytvořit lidské hráče (avatar a jméno si vybere hráč na začátku)
            // Prozatím použijeme výchozí avatar index 0
            for (int i = 0; i < humanCount; i++)
            {
                string playerName = humanCount == 1 ? "Player" : $"Player {i + 1}";
                int avatarIndex = 0; // Hráč si vybere avatar později
                
                players.Add(CreateHumanPlayer(playerId++, playerName, settingsProvider, avatarProvider, avatarIndex));
            }
            
            // Vytvořit AI hráče z nepřátel
            // Pro nepřátele používáme IEnemyProvider, ale pro kompatibilitu s UI musíme použít avatar index
            // UI bude muset být upraveno, aby pracovalo s IEnemyProvider místo IAvatarProvider pro AI hráče
            for (int i = 0; i < aiCount; i++)
            {
                string enemyName = $"AI Player {i + 1}"; // Výchozí jméno
                int enemyIndex = 0; // Index v EnemyDatabase
                
                if (i < availableEnemies.Count)
                {
                    var selectedEnemy = availableEnemies[i];
                    enemyIndex = selectedEnemy.Index;
                    enemyName = selectedEnemy.EnemyData.EnemyName;
                }
                else if (availableEnemies.Count > 0)
                {
                    // Fallback: použít první dostupný nepřítel
                    enemyIndex = availableEnemies[0].Index;
                    enemyName = availableEnemies[0].EnemyData.EnemyName ?? $"AI Player {i + 1}";
                }
                
                // Pro kompatibilitu s UI používáme enemyIndex jako avatarIndex
                // Poznámka: UI bude muset být upraveno, aby rozlišovalo mezi human avatary a enemy avatary
                players.Add(CreateAIPlayer(playerId++, enemyName, settingsProvider, null, avatarProvider, enemyIndex));
            }
            
            return players;
        }
        
        /// <summary>
        /// Pomocná třída pro uchování enemy dat s jejich indexem.
        /// </summary>
        private class EnemyDataWithIndex
        {
            public EnemyData EnemyData;
            public int Index;
        }
        
        /// <summary>
        /// Vytvoří standardní hru Mariáš s 1 lidským a 2 AI hráči (3 hráči celkem).
        /// </summary>
        public static List<IPlayer> CreateStandardGame(
            ISettingsProvider settingsProvider,
            IAvatarProvider avatarProvider = null,
            IEnemyProvider enemyProvider = null)
        {
            return CreateGamePlayers(settingsProvider, avatarProvider, enemyProvider, humanCount: 1, aiCount: 2);
        }
        
        /// <summary>
        /// Vytvoří hru pouze s AI hráči (pro testování).
        /// </summary>
        public static List<IPlayer> CreateAIGame(
            ISettingsProvider settingsProvider,
            int aiCount = 2,
            IAvatarProvider avatarProvider = null)
        {
            return CreateGamePlayers(settingsProvider, avatarProvider, humanCount: 0, aiCount: aiCount);
        }
        
        /// <summary>
        /// Vytvoří AI strategii na základě úrovně obtížnosti.
        /// </summary>
        private static IAIStrategy CreateStrategyForDifficulty(int difficultyLevel)
        {
            return difficultyLevel switch
            {
                1 => new EasyAIStrategy(),
                2 => new MediumAIStrategy(),
                3 => new HardAIStrategy(),
                _ => new EasyAIStrategy() // Default to Easy
            };
        }
        
        /// <summary>
        /// Zamíchá seznam (Fisher-Yates shuffle).
        /// </summary>
        private static List<T> ShuffleList<T>(List<T> list)
        {
            var shuffled = new List<T>(list);
            var random = new System.Random();
            
            for (int i = shuffled.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                T temp = shuffled[i];
                shuffled[i] = shuffled[j];
                shuffled[j] = temp;
            }
            
            return shuffled;
        }
    }
}

