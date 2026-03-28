using System.Collections.Generic;
using UnityEngine;
using MariasGame.Core;
using MariasGame.Core.Events;
using MariasGame.Core.Interfaces;
using MariasGame.Managers;
using MariasGame.UI;

namespace MariasGame.Game
{
    /// <summary>
    /// Spravuje zobrazení karet v ruce lidského hráče (index 0).
    /// Reaguje na CardEvent: CardsDealt, TalonTaken, TalonDiscarded, CardPlayed, LegalPlaysUpdated.
    /// Drátuje CardDragHandler.OnPlayTriggered na GameController.
    /// </summary>
    public class HandPresenter : MonoBehaviour, IObserver<CardEvent>
    {
        [SerializeField] private CardEventManager _cardEvents;
        [SerializeField] private CardViewFactory _cardViewFactory;
        [SerializeField] private GameBootstrapper _bootstrapper;

        private const int HumanPlayerIndex = 0;
        private readonly List<Card> _discardSelection = new();

        void Awake() => _cardEvents.RegisterObserver(this);
        void OnDestroy() => _cardEvents.UnregisterObserver(this);

        public void OnNotify(CardEvent e)
        {
            switch (e.Type)
            {
                case CardEventType.CardsDealt when e.PlayerIndex == HumanPlayerIndex:
                    _cardViewFactory.CreateHandViews(HumanPlayerIndex, e.Cards, isEnemy: false);
                    WireDragHandlers(e.Cards);
                    break;

                case CardEventType.TalonTaken when e.PlayerIndex == HumanPlayerIndex:
                    _cardViewFactory.CreateHandViews(HumanPlayerIndex, e.Cards, isEnemy: false);
                    WireDragHandlers(e.Cards);
                    break;

                case CardEventType.TalonDiscarded when e.PlayerIndex == HumanPlayerIndex:
                    foreach (var card in e.Cards)
                        _cardViewFactory.RemoveCardView(HumanPlayerIndex, card);
                    _discardSelection.Clear();
                    break;

                case CardEventType.CardPlayed when e.PlayerIndex == HumanPlayerIndex:
                    _cardViewFactory.RemoveCardView(HumanPlayerIndex, e.Card);
                    break;

                case CardEventType.LegalPlaysUpdated:
                    UpdateDraggableState(e.Cards);
                    break;
            }
        }

        private void WireDragHandlers(IReadOnlyList<Card> cards)
        {
            foreach (var card in cards)
            {
                var view = _cardViewFactory.GetCardView(HumanPlayerIndex, card);
                if (view == null) continue;

                var drag = view.GetComponent<CardDragHandler>();
                if (drag == null) continue;

                var capturedCard = card;
                drag.OnPlayTriggered += () => OnCardPlayTriggered(capturedCard);
            }
        }

        private void OnCardPlayTriggered(Card card)
        {
            var controller = _bootstrapper.GameController;
            var state = controller.State;

            if (state.Phase == GamePhase.Playing)
            {
                controller.PlayCard(HumanPlayerIndex, card);
            }
            else if (state.Phase == GamePhase.DiscardingTalon &&
                     state.CurrentPlayerIndex == HumanPlayerIndex)
            {
                if (!_discardSelection.Contains(card))
                    _discardSelection.Add(card);

                if (_discardSelection.Count == 2)
                    controller.DiscardToTalon(new List<Card>(_discardSelection));
            }
        }

        private void UpdateDraggableState(IReadOnlyList<Card> legalCards)
        {
            var handViews = _cardViewFactory.GetHandViews(HumanPlayerIndex);
            if (handViews == null) return;

            var legalSet = new HashSet<Card>(legalCards);
            foreach (var kvp in handViews)
            {
                var drag = kvp.Value?.GetComponent<CardDragHandler>();
                drag?.SetDraggable(legalSet.Contains(kvp.Key));
            }
        }
    }
}
