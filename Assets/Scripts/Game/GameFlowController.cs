using System.Collections.Generic;
using UnityEngine;
using MariasGame.Core;
using MariasGame.Core.Events;
using MariasGame.Core.Interfaces;
using MariasGame.Managers;

namespace MariasGame.Game
{
    /// <summary>
    /// Orchestruje průběh scény: spouští hru, reaguje na konec kola.
    /// </summary>
    public class GameFlowController : MonoBehaviour, IObserver<GameEvent>
    {
        [Header("Event Managers")]
        [SerializeField] private GameEventManager _gameEvents;

        [Header("Dependencies")]
        [SerializeField] private GameBootstrapper _bootstrapper;
        [SerializeField] private GameSessionController _sessionController;
        [SerializeField] private CharacterManager _characterManager;

        [Header("Player Names")]
        [SerializeField] private string _humanPlayerName = "Ty";
        [SerializeField] private string _aiPlayer1Name   = "Pepa AI";
        [SerializeField] private string _aiPlayer2Name   = "Karel AI";

        private List<string> _playerNames;

        void Awake()
        {
            _playerNames = new List<string> { _humanPlayerName, _aiPlayer1Name, _aiPlayer2Name };
            _gameEvents.RegisterObserver(this);
        }

        void OnDestroy() => _gameEvents.UnregisterObserver(this);

        public void StartGame()
        {
            ApplyLaunchContextIfAvailable();
            _bootstrapper.GameController.StartNewGame(_playerNames);
        }

        private void ApplyLaunchContextIfAvailable()
        {
            if (!GameLaunchContextStore.HasContext) return;

            var context = GameLaunchContextStore.Current;
            var service = _characterManager != null ? _characterManager.GetService() : null;

            string humanName = string.IsNullOrWhiteSpace(context.PlayerName)
                ? _humanPlayerName : context.PlayerName.Trim();

            if (service != null && string.IsNullOrWhiteSpace(context.PlayerName))
            {
                var playerChar = service.GetById(context.PlayerCharacterId);
                if (playerChar != null && !string.IsNullOrWhiteSpace(playerChar.CharacterName))
                    humanName = playerChar.CharacterName;
            }

            string ai1Name = ResolveName(service, context.Enemy1CharacterId, _aiPlayer1Name);
            string ai2Name = ResolveName(service, context.Enemy2CharacterId, _aiPlayer2Name);

            _playerNames = new List<string> { humanName, ai1Name, ai2Name };
        }

        private static string ResolveName(Services.CharacterService service, int characterId, string fallback)
        {
            if (service == null) return fallback;
            var character = service.GetById(characterId);
            return character != null && !string.IsNullOrWhiteSpace(character.CharacterName)
                ? character.CharacterName : fallback;
        }

        public void OnNotify(GameEvent e)
        {
            if (e.Type != GameEventType.GameEnded) return;

            // TODO: Zobrazit výsledkový panel s daty ze _sessionController.Session.
        }
    }
}
