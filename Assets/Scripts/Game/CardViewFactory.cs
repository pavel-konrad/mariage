using System.Collections.Generic;
using UnityEngine;
using MariasGame.Core;
using MariasGame.Managers;
using MariasGame.UI;

namespace MariasGame.Game
{
    /// <summary>
    /// Vytváří a spravuje CardView prefaby pro ruky hráčů.
    /// </summary>
    public class CardViewFactory : MonoBehaviour
    {
        [Header("Prefab")]
        [SerializeField] private GameObject _cardViewPrefab;

        [Header("Hand Containers")]
        [SerializeField] private Transform _humanHandContainer;
        [SerializeField] private Transform _aiPlayer1Container;
        [SerializeField] private Transform _aiPlayer2Container;

        [Header("Dependencies")]
        [SerializeField] private CardDataManager _cardDataManager;

        private readonly Dictionary<Card, CardView>[] _handViews = new Dictionary<Card, CardView>[3];

        void Awake()
        {
            for (int i = 0; i < 3; i++)
                _handViews[i] = new Dictionary<Card, CardView>();
        }

        public void CreateHandViews(int playerIndex, IReadOnlyList<Card> cards, bool isEnemy)
        {
            var container = GetContainer(playerIndex);
            if (container == null) return;

            var dataProvider  = _cardDataManager.GetCardDataService();
            var themeProvider = _cardDataManager.GetThemeService();

            foreach (var card in cards)
            {
                var go    = Instantiate(_cardViewPrefab, container);
                var view  = go.GetComponent<CardView>();
                var state = new CardWithState(card.Suit, card.Rank, CardState.InHand);
                view.SetCard(state, dataProvider, themeProvider, isEnemy);
                _handViews[playerIndex][card] = view;
            }
        }

        public void RemoveCardView(int playerIndex, Card card)
        {
            if (playerIndex < 0 || playerIndex >= 3) return;
            if (_handViews[playerIndex].TryGetValue(card, out var view))
            {
                if (view != null) Destroy(view.gameObject);
                _handViews[playerIndex].Remove(card);
            }
        }

        public CardView GetCardView(int playerIndex, Card card)
        {
            if (playerIndex < 0 || playerIndex >= 3) return null;
            return _handViews[playerIndex].TryGetValue(card, out var view) ? view : null;
        }

        public IReadOnlyDictionary<Card, CardView> GetHandViews(int playerIndex)
        {
            if (playerIndex < 0 || playerIndex >= 3) return null;
            return _handViews[playerIndex];
        }

        public void ClearHandViews(int playerIndex)
        {
            if (playerIndex < 0 || playerIndex >= 3) return;
            foreach (var view in _handViews[playerIndex].Values)
                if (view != null) Destroy(view.gameObject);
            _handViews[playerIndex].Clear();
        }

        private Transform GetContainer(int playerIndex) => playerIndex switch
        {
            0 => _humanHandContainer,
            1 => _aiPlayer1Container,
            2 => _aiPlayer2Container,
            _ => null
        };
    }
}
