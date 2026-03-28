using UnityEngine;
using MariasGame.Core;
using MariasGame.Core.Events;
using MariasGame.Core.Interfaces;
using MariasGame.Managers;
using MariasGame.UI;

namespace MariasGame.Game
{
    /// <summary>
    /// Zobrazuje/skrývá BiddingModal pro lidského hráče (index 0).
    /// Přepojuje výběr nabídky na GameController.MakeBid().
    /// </summary>
    public class BiddingPresenter : MonoBehaviour, IObserver<GameEvent>
    {
        [SerializeField] private GameEventManager _gameEvents;
        [SerializeField] private BiddingModal _biddingModal;
        [SerializeField] private GameBootstrapper _bootstrapper;

        void Awake()
        {
            _gameEvents.RegisterObserver(this);
            _biddingModal.OnBidSelected += bid =>
                _bootstrapper.GameController.MakeBid(0, bid);
        }

        void OnDestroy() => _gameEvents.UnregisterObserver(this);

        public void OnNotify(GameEvent e)
        {
            if (e.Type != GameEventType.PlayerTurnStarted) return;
            if (e.PlayerIndex != 0) return;
            if (_bootstrapper.GameController.State.Phase == GamePhase.Bidding)
                _biddingModal.Show();
        }
    }
}
