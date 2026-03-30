using MariasGame.ScriptableObjects;

namespace MariasGame.Core
{
    /// <summary>
    /// Přenos dat z úvodní obrazovky do herní scény.
    /// </summary>
    public sealed class GameLaunchContext
    {
        public string PlayerName { get; }
        public int PlayerCharacterId { get; }
        public int Enemy1CharacterId { get; }
        public int Enemy2CharacterId { get; }
        public GameModeConfig GameModeConfig { get; }

        public GameLaunchContext(
            string playerName,
            int playerCharacterId,
            int enemy1CharacterId,
            int enemy2CharacterId,
            GameModeConfig gameModeConfig)
        {
            PlayerName = playerName;
            PlayerCharacterId = playerCharacterId;
            Enemy1CharacterId = enemy1CharacterId;
            Enemy2CharacterId = enemy2CharacterId;
            GameModeConfig = gameModeConfig;
        }
    }
}
