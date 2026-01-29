using UnityEngine;
using MariasGame.Core;

namespace MariasGame.ScriptableObjects
{
    /// <summary>
    /// ScriptableObject pro nastavení hry.
    /// Obsahuje všechny konfigurovatelné hodnoty pro hru.
    /// </summary>
    [CreateAssetMenu(fileName = "GameSettings", menuName = "MariasGame/Game Settings", order = 6)]
    public class GameSettingsSO : ScriptableObject
    {
        [Header("Player Settings")]
        public int startingCash = 1000;
        public int minBet = 1;
        public int maxBet = 100;
        
        [Header("Game Rules")]
        public int cardsPerPlayer = 10;
        public int maxPlayers = 3;
        public int minPlayers = 3;
        
        [Header("AI Settings")]
        public float aiThinkTime = 1.5f;
        public int aiDifficultyLevel = 1; // 1 = Easy, 2 = Medium, 3 = Hard
        
        [Header("UI Settings")]
        public float cardAnimationDuration = 0.5f;
        public float dealCardDelay = 0.2f;
        
        [Header("Audio Settings")]
        [Range(0f, 1f)]
        public float masterVolume = 1.0f;
        [Range(0f, 1f)]
        public float musicVolume = 0.7f;
        [Range(0f, 1f)]
        public float sfxVolume = 1.0f;
        
        /// <summary>
        /// Vytvoří GameSettings z tohoto ScriptableObject.
        /// </summary>
        public GameSettings ToGameSettings()
        {
            return new GameSettings
            {
                startingCash = startingCash,
                minBet = minBet,
                maxBet = maxBet,
                cardsPerPlayer = cardsPerPlayer,
                maxPlayers = maxPlayers,
                minPlayers = minPlayers,
                aiThinkTime = aiThinkTime,
                aiDifficultyLevel = aiDifficultyLevel,
                cardAnimationDuration = cardAnimationDuration,
                dealCardDelay = dealCardDelay,
                masterVolume = masterVolume,
                musicVolume = musicVolume,
                sfxVolume = sfxVolume
            };
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
            return Mathf.Max(minBet, Mathf.Min(betAmount, maxBet));
        }
        
        /// <summary>
        /// Zkontroluje, zda je počet hráčů platný.
        /// </summary>
        public bool IsValidPlayerCount(int playerCount)
        {
            return playerCount >= minPlayers && playerCount <= maxPlayers;
        }
        
        private void OnValidate()
        {
            // Validace v Editoru
            if (startingCash < 0)
            {
                Debug.LogWarning($"[GameSettingsSO] {name}: Starting cash cannot be negative!");
                startingCash = 0;
            }
            
            if (minBet < 0)
            {
                Debug.LogWarning($"[GameSettingsSO] {name}: Min bet cannot be negative!");
                minBet = 0;
            }
            
            if (maxBet < minBet)
            {
                Debug.LogWarning($"[GameSettingsSO] {name}: Max bet cannot be less than min bet!");
                maxBet = minBet;
            }
            
            if (cardsPerPlayer < 1)
            {
                Debug.LogWarning($"[GameSettingsSO] {name}: Cards per player must be at least 1!");
                cardsPerPlayer = 1;
            }
            
            if (minPlayers < 3)
            {
                Debug.LogWarning($"[GameSettingsSO] {name}: Min players must be at least 3 for Mariáš!");
                minPlayers = 3;
            }
            
            if (maxPlayers < minPlayers)
            {
                Debug.LogWarning($"[GameSettingsSO] {name}: Max players cannot be less than min players!");
                maxPlayers = minPlayers;
            }
            
            if (aiDifficultyLevel < 1 || aiDifficultyLevel > 3)
            {
                Debug.LogWarning($"[GameSettingsSO] {name}: AI difficulty level must be between 1 and 3!");
                aiDifficultyLevel = Mathf.Clamp(aiDifficultyLevel, 1, 3);
            }
        }
    }
}

