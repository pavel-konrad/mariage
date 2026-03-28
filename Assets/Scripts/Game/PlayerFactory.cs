using System.Collections.Generic;
using System.Linq;
using MariasGame.Core;
using MariasGame.Core.Interfaces;
using MariasGame.Game.Strategies;
using MariasGame.ScriptableObjects;
using MariasGame.Services;

namespace MariasGame.Game
{
    /// <summary>
    /// Factory pro vytváření hráčů.
    /// Centralizovaná logika pro vytváření Human a AI hráčů.
    /// </summary>
    public static class PlayerFactory
    {
        public static HumanPlayer CreateHumanPlayer(
            int id,
            string name,
            ISettingsProvider settingsProvider,
            int avatarIndex = 0)
        {
            var settings = settingsProvider?.GetSettings() ?? GameSettings.CreateDefault();
            return new HumanPlayer(id, name, settings.startingCash, avatarIndex);
        }

        public static HumanPlayer CreateHumanPlayer(int id, string name, int startingCash, int avatarIndex = 0)
            => new HumanPlayer(id, name, startingCash, avatarIndex);

        public static AIPlayer CreateAIPlayer(
            int id,
            string name,
            ISettingsProvider settingsProvider,
            IAIStrategy strategy = null,
            AvatarService avatarService = null,
            int avatarIndex = 0)
        {
            var settings = settingsProvider?.GetSettings() ?? GameSettings.CreateDefault();

            string aiName = name;
            if (string.IsNullOrEmpty(aiName) && avatarService != null && avatarIndex >= 0 && avatarIndex < avatarService.AvatarCount)
            {
                var avatarData = avatarService.GetAvatar(avatarIndex);
                if (avatarData != null && !string.IsNullOrEmpty(avatarData.AvatarName))
                    aiName = avatarData.AvatarName;
            }

            if (string.IsNullOrEmpty(aiName))
                aiName = $"AI Player {id}";

            if (strategy == null)
                strategy = CreateStrategyForDifficulty(settings.aiDifficultyLevel);

            return new AIPlayer(id, aiName, strategy, settings.startingCash, avatarIndex);
        }

        public static AIPlayer CreateAIPlayer(int id, string name, IAIStrategy strategy, int startingCash, int avatarIndex = 0)
            => new AIPlayer(id, name ?? $"AI Player {id}", strategy, startingCash, avatarIndex);

        public static List<IPlayer> CreateGamePlayers(
            ISettingsProvider settingsProvider,
            AvatarService avatarService = null,
            EnemyService enemyService = null,
            int humanCount = 1,
            int aiCount = 2)
        {
            var players = new List<IPlayer>();
            int playerId = 1;

            List<EnemyDataWithIndex> availableEnemies = new List<EnemyDataWithIndex>();
            if (enemyService != null && enemyService.EnemyCount > 0)
            {
                var allEnemies = enemyService.GetAllEnemies();
                for (int i = 0; i < allEnemies.Count; i++)
                {
                    if (allEnemies[i] != null)
                        availableEnemies.Add(new EnemyDataWithIndex { EnemyData = allEnemies[i], Index = i });
                }
                availableEnemies = ShuffleList(availableEnemies);
            }

            if (availableEnemies.Count < aiCount)
                UnityEngine.Debug.LogWarning($"[PlayerFactory] Not enough enemies in database! Need {aiCount}, but only {availableEnemies.Count} available.");

            for (int i = 0; i < humanCount; i++)
            {
                string playerName = humanCount == 1 ? "Player" : $"Player {i + 1}";
                players.Add(CreateHumanPlayer(playerId++, playerName, settingsProvider, avatarIndex: 0));
            }

            for (int i = 0; i < aiCount; i++)
            {
                string enemyName = $"AI Player {i + 1}";
                int enemyIndex = 0;

                if (i < availableEnemies.Count)
                {
                    enemyName  = availableEnemies[i].EnemyData.EnemyName;
                    enemyIndex = availableEnemies[i].Index;
                }
                else if (availableEnemies.Count > 0)
                {
                    enemyName  = availableEnemies[0].EnemyData.EnemyName ?? $"AI Player {i + 1}";
                    enemyIndex = availableEnemies[0].Index;
                }

                players.Add(CreateAIPlayer(playerId++, enemyName, settingsProvider, null, avatarService, enemyIndex));
            }

            return players;
        }

        public static List<IPlayer> CreateStandardGame(
            ISettingsProvider settingsProvider,
            AvatarService avatarService = null,
            EnemyService enemyService = null)
            => CreateGamePlayers(settingsProvider, avatarService, enemyService, humanCount: 1, aiCount: 2);

        public static List<IPlayer> CreateAIGame(
            ISettingsProvider settingsProvider,
            int aiCount = 2,
            AvatarService avatarService = null)
            => CreateGamePlayers(settingsProvider, avatarService, humanCount: 0, aiCount: aiCount);

        private static IAIStrategy CreateStrategyForDifficulty(int difficultyLevel) => difficultyLevel switch
        {
            2 => new MediumAIStrategy(),
            3 => new HardAIStrategy(),
            _ => new EasyAIStrategy()
        };

        private class EnemyDataWithIndex
        {
            public EnemyData EnemyData;
            public int Index;
        }

        private static List<T> ShuffleList<T>(List<T> list)
        {
            var shuffled = new List<T>(list);
            var random = new System.Random();
            for (int i = shuffled.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
            }
            return shuffled;
        }
    }
}
