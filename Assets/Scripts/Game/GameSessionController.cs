using UnityEngine;
using MariasGame.Core;
using MariasGame.Core.Events;
using MariasGame.Core.Interfaces;
using MariasGame.Managers;

namespace MariasGame.Game
{
    /// <summary>
    /// Zaznamenává výsledky kol do GameSessionData.
    /// Nepracuje s UI – pouze drží session stav a poskytuje ho jiným.
    /// </summary>
    public class GameSessionController : MonoBehaviour, IObserver<GameEvent>
    {
        [SerializeField] private GameEventManager _gameEvents;

        public GameSessionData Session { get; private set; }

        void Awake()
        {
            Session = new GameSessionData();
            _gameEvents.RegisterObserver(this);
        }

        void OnDestroy() => _gameEvents.UnregisterObserver(this);

        public void OnNotify(GameEvent e)
        {
            if (e.Type != GameEventType.GameEnded) return;

            Session.RecordRoundResult(e.Result, e.DeclarerIndex, e.PlayerIndex);
        }
    }
}
