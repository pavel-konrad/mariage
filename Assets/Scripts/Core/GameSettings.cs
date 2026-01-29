using System;

namespace MariasGame.Core
{
    /// <summary>
    /// Centralizované nastavení herních parametrů.
    /// Obsahuje všechny konfigurovatelné hodnoty pro hru.
    /// </summary>
    [Serializable]
    public class GameSettings
    {
        // Player Settings
        public int startingCash = 1000;
        public int minBet = 1;
        public int maxBet = 100;
        
        // Game Rules
        public int cardsPerPlayer = 10;
        public int maxPlayers = 3;
        public int minPlayers = 3;
        
        // AI Settings
        public float aiThinkTime = 1.5f;
        public int aiDifficultyLevel = 1; // 1 = Easy, 2 = Medium, 3 = Hard
        
        // UI Settings
        public float cardAnimationDuration = 0.5f;
        public float dealCardDelay = 0.2f;
        
        // Audio Settings
        public float masterVolume = 1.0f;
        public float musicVolume = 0.7f;
        public float sfxVolume = 1.0f;
        
        /// <summary>
        /// Vytvoří defaultní nastavení hry.
        /// </summary>
        public static GameSettings CreateDefault()
        {
            return new GameSettings();
        }
        
        /// <summary>
        /// Zkontroluje, zda je sázka v platném rozsahu.
        /// </summary>
        public bool IsValidBet(int betAmount)
        {
            return betAmount >= minBet && betAmount <= maxBet;
        }
        
        /// <summary>
        /// Omezí sázku na platný rozsah.
        /// </summary>
        public int ClampBet(int betAmount)
        {
            return Math.Max(minBet, Math.Min(betAmount, maxBet));
        }
        
        /// <summary>
        /// Zkontroluje, zda je počet hráčů platný.
        /// </summary>
        public bool IsValidPlayerCount(int playerCount)
        {
            return playerCount >= minPlayers && playerCount <= maxPlayers;
        }
        
        public override string ToString()
        {
            return $"GameSettings(Cash: {startingCash}, Bet: {minBet}-{maxBet}, Players: {minPlayers}-{maxPlayers}, Cards: {cardsPerPlayer})";
        }
    }
}
