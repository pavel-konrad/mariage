using UnityEngine;
using MariasGame.Core;
using MariasGame.Managers;
using MariasGame.ScriptableObjects;

namespace MariasGame.Game
{
    /// <summary>
    /// Drátuje všechny závislosti a předává řízení GameFlowControlleru.
    /// Nahrazuje ServiceLocator – závislosti jsou explicitní přes Inspector.
    /// </summary>
    public class GameBootstrapper : MonoBehaviour
    {
        [Header("Event Managers")]
        [SerializeField] private GameEventManager gameEventManager;
        [SerializeField] private CardEventManager cardEventManager;
        [SerializeField] private ScoreEventManager scoreEventManager;

        [Header("Data Managers")]
        [SerializeField] private CardDataManager cardDataManager;

        [Header("Game Mode")]
        [SerializeField] private GameModeConfig gameModeConfig;

        [Header("Scene Controllers")]
        [SerializeField] private GameFlowController gameFlowController;

        private MariasGameController _gameController;

        public MariasGameController GameController => _gameController;

        void Awake()
        {
            var deckFactory = cardDataManager.GetDeckFactory();
            var selectedMode = GameLaunchContextStore.HasContext && GameLaunchContextStore.Current.GameModeConfig != null
                ? GameLaunchContextStore.Current.GameModeConfig
                : gameModeConfig;

            _gameController = new MariasGameController(
                deckFactory,
                gameEventManager,
                cardEventManager,
                scoreEventManager);

            _gameController.Initialize(selectedMode);
            // Presentery a controllery se zaregistrují samy ve svém Awake()
        }

        void Start()
        {
            gameFlowController.StartGame();
        }
    }
}
