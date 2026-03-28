using UnityEngine;
using MariasGame.Core;
using MariasGame.Core.Events;
using MariasGame.Core.Interfaces;
using MariasGame.Managers;
using MariasGame.UI;

namespace MariasGame.Game
{
    /// <summary>
    /// Zobrazuje/skrývá TrumpSelectionModal pro lidského hráče (index 0).
    /// Přepojuje výběr trumfu na GameController.DeclareTrump().
    /// </summary>
    public class TrumpPresenter : MonoBehaviour, IObserver<GameEvent>
    {
        [SerializeField] private GameEventManager _gameEvents;
        [SerializeField] private TrumpSelectionModal _trumpModal;
        [SerializeField] private GameBootstrapper _bootstrapper;

        void Awake()
        {
            _gameEvents.RegisterObserver(this);
            _trumpModal.OnTrumpSelected += suit =>
                _bootstrapper.GameController.DeclareTrump(suit);
        }

        void OnDestroy() => _gameEvents.UnregisterObserver(this);

        public void OnNotify(GameEvent e)
        {
            if (e.Type != GameEventType.PlayerTurnStarted) return;
            if (e.PlayerIndex != 0) return;
            if (_bootstrapper.GameController.State.Phase == GamePhase.Declaring)
                _trumpModal.Show();
        }
    }
}
