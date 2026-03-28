using System.Collections.Generic;
using UnityEngine;
using MariasGame.Core.Events;
using MariasGame.Core.Interfaces;
using MariasGame.Managers;

namespace MariasGame.Game
{
    /// <summary>
    /// Orchestruje průběh scény: spouští hru, reaguje na konec kola.
    /// Přebírá odpovědnost za spouštění kol od GameBootstrapper.
    /// </summary>
    public class GameFlowController : MonoBehaviour, IObserver<GameEvent>
    {
        [Header("Event Managers")]
        [SerializeField] private GameEventManager _gameEvents;

        [Header("Dependencies")]
        [SerializeField] private GameBootstrapper _bootstrapper;
        [SerializeField] private GameSessionController _sessionController;

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

        /// <summary>
        /// Spustí první kolo. Voláno z GameBootstrapper.Start().
        /// </summary>
        public void StartGame()
        {
            _bootstrapper.GameController.StartNewGame(_playerNames);
        }

        public void OnNotify(GameEvent e)
        {
            if (e.Type != GameEventType.GameEnded) return;

            // Session data jsou zaznamenána automaticky v GameSessionController.
            // TODO: Zobrazit výsledkový panel (Game Over UI) s daty ze _sessionController.Session.
            // Po potvrzení hráče zavolat StartGame() pro nové kolo.
        }
    }
}
