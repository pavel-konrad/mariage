using UnityEngine;
using UnityEngine.UI;
using MariasGame.Core;
using MariasGame.Services;

namespace MariasGame.UI
{
    /// <summary>
    /// UI komponenta pro zobrazení karty.
    /// Reaguje na změny stavu karty a spouští odpovídající animace.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class CardView : MonoBehaviour
    {
        [Header("Card Reference")]
        [SerializeField] private CardWithState card;

        [Header("UI Components")]
        [SerializeField] private Image cardImage;
        [SerializeField] private Image cardBackImage;

        [Header("Animation")]
        [SerializeField] private CardAnimationController animationController;

        [Header("View State")]
        [SerializeField] private bool isFaceUp = true;
        [SerializeField] private bool isInteractive = true;

        private CardState _lastState;
        private CardDataService _cardDataService;
        private CardThemeService _themeService;
        private bool _isEnemyCard = false;
        private bool _flipOverride = false;

        void Awake()
        {
            if (cardImage == null)
                cardImage = GetComponent<Image>();

            if (animationController == null)
                animationController = GetComponent<CardAnimationController>();
        }

        void OnEnable()
        {
            if (card != null)
            {
                card.OnStateChanged += OnCardStateChanged;
                _lastState = card.State;
                UpdateCardVisuals();
            }
        }

        void OnDisable()
        {
            if (card != null)
                card.OnStateChanged -= OnCardStateChanged;
        }

        public void SetCard(CardWithState cardToDisplay, CardDataService dataService, CardThemeService themeService = null, bool isEnemyCard = false)
        {
            if (card != null)
                card.OnStateChanged -= OnCardStateChanged;

            card = cardToDisplay;
            _cardDataService = dataService;
            _themeService = themeService;
            _isEnemyCard = isEnemyCard;

            if (card != null)
            {
                card.OnStateChanged += OnCardStateChanged;
                _lastState = card.State;
                UpdateCardVisuals();
            }
        }

        private void OnCardStateChanged(CardState oldState, CardState newState)
        {
            _lastState = newState;

            if (animationController != null)
                animationController.PlayTransition(oldState, newState);

            UpdateCardVisuals();
            UpdateInteractivity();
        }

        private void UpdateCardVisuals()
        {
            if (card == null || _cardDataService == null) return;
            if (!gameObject.activeSelf) return;

            bool shouldShowFaceUp;
            if (_flipOverride)
            {
                shouldShowFaceUp = isFaceUp;
            }
            else
            {
                shouldShowFaceUp = _isEnemyCard ? false : ShouldShowFaceUp(card.State);
                isFaceUp = shouldShowFaceUp;
            }

            if (shouldShowFaceUp)
            {
                var sprite = _cardDataService.GetCardSprite(card.Suit, card.Rank);

                if (cardBackImage != null) cardBackImage.enabled = false;

                if (cardImage != null)
                {
                    cardImage.sprite = sprite;
                    cardImage.enabled = true;
                }
            }
            else
            {
                var cardBackSprite = _themeService?.GetCardBackSprite();

                if (cardBackImage != null && cardBackSprite != null)
                {
                    cardBackImage.sprite = cardBackSprite;
                    cardBackImage.enabled = true;
                    if (cardImage != null) cardImage.enabled = false;
                }
                else if (cardImage != null)
                {
                    if (cardBackSprite != null) cardImage.sprite = cardBackSprite;
                    cardImage.enabled = true;
                    if (cardBackImage != null) cardBackImage.enabled = false;
                }
            }
        }

        private bool ShouldShowFaceUp(CardState state) => state switch
        {
            CardState.InDeck   => false,
            CardState.Dealing  => false,
            _                  => true
        };

        private void UpdateInteractivity()
        {
            if (card == null) return;
            isInteractive = card.State == CardState.InHand || card.State == CardState.Selected;
        }

        public void FlipCard(bool faceUp)
        {
            _flipOverride = true;
            isFaceUp = faceUp;
            UpdateCardVisuals();

            if (animationController != null)
                animationController.PlayFlipAnimation(faceUp);
        }
    }
}
