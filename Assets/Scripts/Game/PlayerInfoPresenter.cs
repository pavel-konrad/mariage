using UnityEngine;
using MariasGame.Core;
using MariasGame.Core.Events;
using MariasGame.Core.Interfaces;
using MariasGame.Managers;
using MariasGame.UI;

namespace MariasGame.Game
{
    /// <summary>
    /// Zobrazuje jméno a skóre každého hráče a zvýrazňuje aktivního hráče.
    /// Reaguje na ScoreEvent (aktualizace bodů) a GameEvent (tah, hláška, start hry).
    /// </summary>
    public class PlayerInfoPresenter : MonoBehaviour, IObserver<ScoreEvent>, IObserver<GameEvent>
    {
        [Header("Event Managers")]
        [SerializeField] private ScoreEventManager _scoreEvents;
        [SerializeField] private GameEventManager _gameEvents;

        [Header("Player Panels")]
        [SerializeField] private PlayerInfoPanel _panel0; // lidský hráč
        [SerializeField] private PlayerInfoPanel _panel1; // AI 1
        [SerializeField] private PlayerInfoPanel _panel2; // AI 2

        [Header("Dependencies")]
        [SerializeField] private GameBootstrapper _bootstrapper;

        private PlayerInfoPanel[] _panels;

        void Awake()
        {
            _panels = new[] { _panel0, _panel1, _panel2 };
            _scoreEvents.RegisterObserver(this);
            _gameEvents.RegisterObserver(this);
        }

        void OnDestroy()
        {
            _scoreEvents.UnregisterObserver(this);
            _gameEvents.UnregisterObserver(this);
        }

        public void OnNotify(ScoreEvent e)
        {
            if (e.PlayerIndex < 0 || e.PlayerIndex >= 3) return;
            _panels[e.PlayerIndex]?.UpdateScore(e.NewScore);
        }

        public void OnNotify(GameEvent e)
        {
            switch (e.Type)
            {
                case GameEventType.GameStarted:
                    var names = _bootstrapper.GameController.State.PlayerNames;
                    for (int i = 0; i < 3; i++)
                        _panels[i]?.SetPlayerInfo(names[i]);
                    break;

                case GameEventType.PlayerTurnStarted:
                    for (int i = 0; i < 3; i++)
                        _panels[i]?.SetHighlight(i == e.PlayerIndex);
                    break;

                case GameEventType.MarriageDeclared:
                    if (e.PlayerIndex >= 0 && e.PlayerIndex < 3)
                        _panels[e.PlayerIndex]?.ShowDeclaration(e.TrumpSuit);
                    break;
            }
        }
    }
}
