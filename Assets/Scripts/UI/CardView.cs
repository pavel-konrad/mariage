using UnityEngine;
using UnityEngine.UI;
using MariasGame.Core;
using MariasGame.Core.Interfaces;

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
        [SerializeField] private Image cardBackImage; // Rub karty
        
        [Header("Animation")]
        [SerializeField] private CardAnimationController animationController;
        
        [Header("View State")]
        [SerializeField] private bool isFaceUp = true;
        [SerializeField] private bool isInteractive = true;
        
        private CardState _lastState;
        private ICardDataProvider _cardDataProvider;
        private ICardThemeProvider _themeProvider;
        private bool _isEnemyCard = false; // Určuje, zda je to karta nepřítele (vždy rub)
        
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
            {
                card.OnStateChanged -= OnCardStateChanged;
            }
        }
        
        /// <summary>
        /// Nastaví kartu pro zobrazení.
        /// </summary>
        public void SetCard(CardWithState cardToDisplay, ICardDataProvider dataProvider, ICardThemeProvider themeProvider = null, bool isEnemyCard = false)
        {
            if (card != null)
            {
                card.OnStateChanged -= OnCardStateChanged;
            }
            
            card = cardToDisplay;
            _cardDataProvider = dataProvider;
            _themeProvider = themeProvider;
            _isEnemyCard = isEnemyCard;
            
            if (card != null)
            {
                card.OnStateChanged += OnCardStateChanged;
                _lastState = card.State;
                UpdateCardVisuals();
            }
        }
        
        /// <summary>
        /// Reaguje na změnu stavu karty.
        /// </summary>
        private void OnCardStateChanged(CardState oldState, CardState newState)
        {
            _lastState = newState;
            
            // Spustit animaci přechodu
            if (animationController != null)
            {
                animationController.PlayTransition(oldState, newState);
            }
            
            // Aktualizovat vizuály
            UpdateCardVisuals();
            
            // Aktualizovat interaktivitu
            UpdateInteractivity();
        }
        
        /// <summary>
        /// Aktualizuje vizuální reprezentaci karty.
        /// </summary>
        private void UpdateCardVisuals()
        {
            if (card == null || _cardDataProvider == null)
            {
                return;
            }
            
            // DŮLEŽITÉ: Pokud GameObject není aktivní, nemůžeme aktualizovat komponenty
            if (!gameObject.activeSelf)
            {
                // Tichý return - GameObject může být neaktivní během inicializace
                return;
            }
            
            // Určit, zda má být karta lícem nahoru
            // Pokud je to karta nepřítele, vždy zobrazit rub
            bool shouldShowFaceUp = _isEnemyCard ? false : ShouldShowFaceUp(card.State);
            isFaceUp = shouldShowFaceUp;
            
            // Načíst data karty
            var cardData = _cardDataProvider.GetCardData(card.Suit, card.Rank);
            
            if (cardData == null)
            {
                Debug.LogError($"[CardView.UpdateCardVisuals] CardData is null for {card.Suit}-{card.Rank}!");
                return;
            }
            
            if (shouldShowFaceUp && cardData.Sprite != null)
            {
                // Zobrazit líc karty
                if (cardBackImage != null)
                {
                    // Máme cardBackImage - deaktivovat ho a aktivovat cardImage s lícem
                    cardBackImage.enabled = false;
                }
                
                if (cardImage != null)
                {
                    cardImage.sprite = cardData.Sprite;
                    // DŮLEŽITÉ: Použít enabled místo SetActive, protože cardImage může být na stejném GameObject
                    // Pokud bychom použili SetActive na cardImage.gameObject a cardImage je na stejném GameObject jako CardView,
                    // deaktivovali bychom celý CardView GameObject!
                    cardImage.enabled = true;
                }
            }
            else
            {
                // Zobrazit rub karty - načíst z tématu
                var cardBackSprite = GetCardBackSprite();
                
                if (cardBackImage != null && cardBackSprite != null)
                {
                    // Máme cardBackImage - použít ho pro zobrazení rubu
                    cardBackImage.sprite = cardBackSprite;
                    cardBackImage.enabled = true;
                    
                    // Deaktivovat cardImage (pokud existuje)
                    if (cardImage != null)
                    {
                        cardImage.enabled = false;
                    }
                }
                else if (cardImage != null && cardBackSprite != null)
                {
                    // Nemáme cardBackImage, ale máme cardBackSprite - použít cardImage pro zobrazení rubu
                    cardImage.sprite = cardBackSprite;
                    cardImage.enabled = true;
                    
                    // Deaktivovat cardBackImage (pokud existuje)
                    if (cardBackImage != null)
                    {
                        cardBackImage.enabled = false;
                    }
                }
                else
                {
                    // Fallback: zobrazit cardImage (i když nemáme cardBackSprite)
                    if (cardImage != null)
                    {
                        // Pokud nemáme cardBackSprite, necháme cardImage aktivní (možná má nějaký default sprite)
                        cardImage.enabled = true;
                    }
                    
                    if (cardBackSprite == null)
                    {
                        Debug.LogWarning($"[CardView.UpdateCardVisuals] Card back sprite is null for {card.Suit}-{card.Rank}! Theme may not have card back sprite assigned.");
                    }
                    
                    if (cardBackImage == null && cardBackSprite != null)
                    {
                        Debug.LogWarning($"[CardView.UpdateCardVisuals] CardBackImage is not assigned in CardView prefab! Using CardImage for card back. Consider adding a CardBackImage component.");
                    }
                }
            }
            
        }
        
        /// <summary>
        /// Určuje, zda má být karta lícem nahoru na základě stavu.
        /// </summary>
        private bool ShouldShowFaceUp(CardState state)
        {
            return state switch
            {
                CardState.InDeck => false,
                CardState.Dealing => false, // Během dealingu může být rub
                CardState.InHand => true,
                CardState.Selected => true,
                CardState.Playing => true,
                CardState.Discarding => true,
                CardState.InDiscardPile => true,
                CardState.OnTable => true,
                _ => true
            };
        }
        
        /// <summary>
        /// Aktualizuje interaktivitu karty na základě stavu.
        /// </summary>
        private void UpdateInteractivity()
        {
            if (card == null)
                return;
            
            // Karta je interaktivní pouze v ruce a když je selected
            isInteractive = card.State == CardState.InHand || card.State == CardState.Selected;
            
            // Můžeme přidat další logiku (např. Button component)
        }
        
        /// <summary>
        /// Otočí kartu (face up/down).
        /// </summary>
        public void FlipCard(bool faceUp)
        {
            isFaceUp = faceUp;
            UpdateCardVisuals();
            
            if (animationController != null)
            {
                animationController.PlayFlipAnimation(faceUp);
            }
        }
        
        /// <summary>
        /// Získá sprite pro rub karty z tématu.
        /// </summary>
        private Sprite GetCardBackSprite()
        {
            if (_themeProvider != null)
            {
                return _themeProvider.GetCardBackSprite();
            }
            
            return null;
        }
    }
}

