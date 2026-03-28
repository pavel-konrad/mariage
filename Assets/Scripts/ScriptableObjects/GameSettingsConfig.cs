using UnityEngine;
using MariasGame.Core;

namespace MariasGame.ScriptableObjects
{
    [CreateAssetMenu(fileName = "GameSettingsConfig", menuName = "MariasGame/Game Settings Config", order = 6)]
    public class GameSettingsConfig : ScriptableObject
    {
        [field: SerializeField] public int StartingCash { get; private set; } = 1000;
        [field: SerializeField] public int MinBet { get; private set; } = 1;
        [field: SerializeField] public int MaxBet { get; private set; } = 100;

        [field: SerializeField] public int CardsPerPlayer { get; private set; } = 10;
        [field: SerializeField] public int MaxPlayers { get; private set; } = 3;
        [field: SerializeField] public int MinPlayers { get; private set; } = 3;

        [field: SerializeField] public float AIThinkTime { get; private set; } = 1.5f;
        [field: SerializeField] public int AIDifficultyLevel { get; private set; } = 1;

        [field: SerializeField] public float CardAnimationDuration { get; private set; } = 0.5f;
        [field: SerializeField] public float DealCardDelay { get; private set; } = 0.2f;

        [field: SerializeField] public float MasterVolume { get; private set; } = 1.0f;
        [field: SerializeField] public float MusicVolume { get; private set; } = 0.7f;
        [field: SerializeField] public float SFXVolume { get; private set; } = 1.0f;

        public GameSettings ToGameSettings() => new GameSettings
        {
            startingCash          = StartingCash,
            minBet                = MinBet,
            maxBet                = MaxBet,
            cardsPerPlayer        = CardsPerPlayer,
            maxPlayers            = MaxPlayers,
            minPlayers            = MinPlayers,
            aiThinkTime           = AIThinkTime,
            aiDifficultyLevel     = AIDifficultyLevel,
            cardAnimationDuration = CardAnimationDuration,
            dealCardDelay         = DealCardDelay,
            masterVolume          = MasterVolume,
            musicVolume           = MusicVolume,
            sfxVolume             = SFXVolume
        };

        public bool IsValidBet(int betAmount) => betAmount >= MinBet && betAmount <= MaxBet;
        public int ClampBet(int betAmount) => Mathf.Max(MinBet, Mathf.Min(betAmount, MaxBet));
        public bool IsValidPlayerCount(int playerCount) => playerCount >= MinPlayers && playerCount <= MaxPlayers;

        private void OnValidate()
        {
            if (StartingCash < 0)
            {
                Debug.LogWarning($"[GameSettingsConfig] {name}: Starting cash cannot be negative!");
                StartingCash = 0;
            }
            if (MinBet < 0)
            {
                Debug.LogWarning($"[GameSettingsConfig] {name}: Min bet cannot be negative!");
                MinBet = 0;
            }
            if (MaxBet < MinBet)
            {
                Debug.LogWarning($"[GameSettingsConfig] {name}: Max bet cannot be less than min bet!");
                MaxBet = MinBet;
            }
            if (CardsPerPlayer < 1)
            {
                Debug.LogWarning($"[GameSettingsConfig] {name}: Cards per player must be at least 1!");
                CardsPerPlayer = 1;
            }
            if (MinPlayers < 3)
            {
                Debug.LogWarning($"[GameSettingsConfig] {name}: Min players must be at least 3 for Mariáš!");
                MinPlayers = 3;
            }
            if (MaxPlayers < MinPlayers)
            {
                Debug.LogWarning($"[GameSettingsConfig] {name}: Max players cannot be less than min players!");
                MaxPlayers = MinPlayers;
            }
            if (AIDifficultyLevel < 1 || AIDifficultyLevel > 3)
            {
                Debug.LogWarning($"[GameSettingsConfig] {name}: AI difficulty level must be between 1 and 3!");
                AIDifficultyLevel = Mathf.Clamp(AIDifficultyLevel, 1, 3);
            }
        }
    }
}
