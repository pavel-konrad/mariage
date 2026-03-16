using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MariasGame.Core;
using MariasGame.Core.Interfaces;
using MariasGame.UI;

namespace MariasGame.Game
{
    /// <summary>
    /// Hlavní manager pro Mariáš UI a herní smyčku.
    /// Propojuje MariasGameController s Unity UI.
    /// </summary>
    public class MariasGameManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Transform playerHandContainer;
        [SerializeField] private Transform[] enemyHandContainers;
        [SerializeField] private Transform trickContainer;
        [SerializeField] private Transform deckContainer;
        
        [Header("Info Panels")]
        [SerializeField] private PlayerInfoPanel playerInfoPanel;
        [SerializeField] private PlayerInfoPanel[] enemyInfoPanels;
        
        [Header("Modals")]
        [SerializeField] private BiddingModal biddingModal;
        [SerializeField] private TrumpSelectionModal trumpSelectionModal;
        [SerializeField] private MarriageButton marriageButton;

        [Header("Panels")]
        [SerializeField] private GameObject talonPanel;
        
        [Header("Status UI")]
        [SerializeField] private TMP_Text gamePhaseText;
        [SerializeField] private Image trumpIndicator;
        [SerializeField] private TMP_Text trumpText;
        
        [Header("Prefabs")]
        [SerializeField] private CardView cardViewPrefab;
        
        [Header("Settings")]
        [SerializeField] private float aiThinkDelay = 1f;
        [SerializeField] private float trickClearDelay = 1.5f;
        
        // Game Controller
        private MariasGameController _gameController;
        private Dictionary<Card, CardView> _cardViews = new Dictionary<Card, CardView>();
        
        // Service references
        private ICardDataProvider _cardDataProvider;
        private ICardThemeProvider _themeProvider;
        
        // Local player index (0 = player, 1-2 = AI)
        private const int LocalPlayerIndex = 0;
        
        // Discard selection for talon
        private List<Card> _selectedForDiscard = new List<Card>();
        
        void Start()
        {
            InitializeGame();
        }
        
        /// <summary>
        /// Inicializace hry.
        /// </summary>
        private void InitializeGame()
        {
            // Získat služby
            var locator = ServiceLocator.Instance;
            _cardDataProvider = locator.GetCardDataProvider();
            _themeProvider = locator.GetCardThemeProvider();
            
            // Vytvořit game controller s IDeckFactory z dat
            var deckFactory = locator.GetDeckFactory();
            _gameController = new MariasGameController(deckFactory);

            // Subscribnout na eventy
            SubscribeToEvents();
            SubscribeToModalEvents();

            // Skrýt všechny panely
            HideAllPanels();
            
            // Spustit novou hru
            StartNewGame();
        }
        
        /// <summary>
        /// Subscribne na všechny herní eventy.
        /// </summary>
        private void SubscribeToEvents()
        {
            _gameController.OnGameStarted += HandleGameStarted;
            _gameController.OnCardsDealt += HandleCardsDealt;
            _gameController.OnPhaseChanged += HandlePhaseChanged;
            _gameController.OnPlayerTurnStarted += HandlePlayerTurnStarted;
            _gameController.OnCardPlayed += HandleCardPlayed;
            _gameController.OnTrickWon += HandleTrickWon;
            _gameController.OnTrumpDeclared += HandleTrumpDeclared;
            _gameController.OnMarriageDeclared += HandleMarriageDeclared;
            _gameController.OnScoreChanged += HandleScoreChanged;
            _gameController.OnGameEnded += HandleGameEnded;
            _gameController.OnLegalPlaysCalculated += HandleLegalPlaysCalculated;
            _gameController.OnSevenDeclared += HandleSevenDeclared;
            _gameController.OnBettelDeclared += HandleBettelDeclared;
            _gameController.OnDurchDeclared += HandleDurchDeclared;
        }

        /// <summary>
        /// Subscribne na eventy z modálních komponent.
        /// </summary>
        private void SubscribeToModalEvents()
        {
            if (biddingModal != null)
            {
                biddingModal.OnBidSelected += (bid) =>
                {
                    if (_gameController.State.CurrentPlayerIndex == LocalPlayerIndex)
                        _gameController.MakeBid(LocalPlayerIndex, bid);
                };
            }

            if (trumpSelectionModal != null)
            {
                Debug.Log("[MariasGameManager] Subscribing to TrumpSelectionModal.OnTrumpSelected");
                trumpSelectionModal.OnTrumpSelected += (suit) =>
                {
                    Debug.Log($"[MariasGameManager] OnTrumpSelected fired! Suit: {suit}, DeclarerIndex: {_gameController.State.DeclarerIndex}, LocalPlayerIndex: {LocalPlayerIndex}");
                    if (_gameController.State.DeclarerIndex == LocalPlayerIndex)
                    {
                        Debug.Log($"[MariasGameManager] Calling DeclareTrump({suit})");
                        _gameController.DeclareTrump(suit);
                    }
                    else
                    {
                        Debug.LogWarning($"[MariasGameManager] DeclarerIndex ({_gameController.State.DeclarerIndex}) != LocalPlayerIndex ({LocalPlayerIndex}), ignoring trump selection");
                    }
                };
            }
            else
            {
                Debug.LogError("[MariasGameManager] trumpSelectionModal is NULL!");
            }

            if (marriageButton != null)
            {
                marriageButton.OnMarriageDeclared += (suit) =>
                {
                    _gameController.DeclareMarriage(suit);
                    marriageButton.HideButton();
                };
            }
        }
        
        /// <summary>
        /// Spustí novou hru.
        /// </summary>
        public void StartNewGame()
        {
            // Vyčistit předchozí hru
            ClearAllCards();
            _selectedForDiscard.Clear();
            
            var playerNames = new List<string> { "Ty", "Pepa AI", "Karel AI" };
            _gameController.StartNewGame(playerNames);
        }
        
        #region Event Handlers
        
        private void HandleGameStarted()
        {
            Debug.Log("[MariasGameManager] Hra začala!");
            ServiceLocator.Instance.GetAudioManager()?.PlayCardShuffle();
        }
        
        private void HandleCardsDealt(int playerIndex, List<Card> cards)
        {
            Debug.Log($"[MariasGameManager] Hráč {playerIndex} dostal {cards.Count} karet");

            // Talon karty — zobrazit lícem nahoru v talonPanelu, po chvíli přesunout do ruky
            if (_gameController.State.Phase == GamePhase.TakingTalon)
            {
                Transform talonTarget = talonPanel != null ? talonPanel.transform : playerHandContainer;
                foreach (var card in cards)
                {
                    var cv = CreateCardView(card, talonTarget, false);
                    if (cv != null) cv.FlipCard(true);
                }
                if (talonPanel != null) ArrangeCardsInHand(talonPanel.transform);
                StartCoroutine(MoveTalonCardsToHand(new List<Card>(cards)));
                ServiceLocator.Instance.GetAudioManager()?.PlayCardDeal();
                return;
            }

            Transform container = GetHandContainer(playerIndex);
            bool isEnemy = playerIndex != LocalPlayerIndex;

            foreach (var card in cards)
            {
                CreateCardView(card, container, isEnemy);
            }

            // Rozložit karty v ruce
            ArrangeCardsInHand(container);

            // Aktualizovat info panel
            UpdatePlayerCards(playerIndex);

            ServiceLocator.Instance.GetAudioManager()?.PlayCardDeal();
        }
        
        private void HandlePhaseChanged(GamePhase phase)
        {
            Debug.Log($"[MariasGameManager] Fáze: {phase}");

            // Aktualizovat UI text
            UpdatePhaseText(phase);

            // Zobrazit/skrýt příslušné panely
            UpdatePanelsForPhase(phase);

            // Povolit drag na kartách při odhazování do talonu
            if (phase == GamePhase.DiscardingTalon)
            {
                EnablePlayerCardDrags();
            }
        }
        
        private void HandlePlayerTurnStarted(int playerIndex)
        {
            Debug.Log($"[MariasGameManager] Na tahu: Hráč {playerIndex}");

            // Zvýraznit aktivního hráče
            HighlightActivePlayer(playerIndex);

            // MarriageButton — zobrazit jen když lokální hráč vynáší a má hlášku
            if (playerIndex == LocalPlayerIndex &&
                _gameController.State.Phase == GamePhase.Playing &&
                _gameController.State.CurrentTrick.Count == 0)
            {
                var marriages = _gameController.GetPossibleMarriages();
                marriageButton?.UpdateState(marriages);
            }
            else
            {
                marriageButton?.HideButton();
            }

            // Pokud je na tahu AI a jsme ve fázi hraní
            if (playerIndex != LocalPlayerIndex &&
                _gameController.State.Phase == GamePhase.Playing)
            {
                StartCoroutine(AIPlayCoroutine(playerIndex));
            }

            // Pokud je na tahu AI a jsme v dražbě
            if (playerIndex != LocalPlayerIndex &&
                _gameController.State.Phase == GamePhase.Bidding)
            {
                StartCoroutine(AIBidCoroutine(playerIndex));
            }

            // Zakázat drag, když není tah lokálního hráče nebo nejsme v Playing/DiscardingTalon fázi
            var currentPhase = _gameController.State.Phase;
            if (playerIndex != LocalPlayerIndex ||
                (currentPhase != GamePhase.Playing && currentPhase != GamePhase.DiscardingTalon))
            {
                DisableAllCardDrags();
            }
        }
        
        private void HandleCardPlayed(int playerIndex, Card card)
        {
            Debug.Log($"[MariasGameManager] Hráč {playerIndex} zahrál {card}");
            
            // Přesunout kartu do trick containeru
            if (_cardViews.TryGetValue(card, out var cardView))
            {
                // Uložit původní parent pro aktualizaci layoutu
                var previousParent = cardView.transform.parent;
                
                // Otočit kartu lícem nahoru (i pro AI)
                cardView.FlipCard(true);
                
                // Přesunout do trick area (worldPositionStays=false pro správné UI pozicování)
                cardView.transform.SetParent(trickContainer, false);
                
                // Aktualizovat layout předchozího containeru (ruka hráče)
                if (previousParent != null)
                {
                    ArrangeCardsInHand(previousParent);
                }
                
                // Pozice podle pořadí ve štychu
                int trickPosition = _gameController.State.CurrentTrick.Count;
                PositionCardInTrick(cardView, trickPosition);
            }
            
            ServiceLocator.Instance.GetAudioManager()?.PlayCardPlace();
            
            if (trickContainer != null)
            {
                ServiceLocator.Instance.GetVFXManager()?.PlayCardPlaceEffect(
                    trickContainer.position);
            }
        }
        
        private void HandleTrickWon(int winnerIndex, List<Card> cards)
        {
            Debug.Log($"[MariasGameManager] Štych vyhrál hráč {winnerIndex}");
            
            ServiceLocator.Instance.GetAudioManager()?.PlayTrickWin();
            
            if (trickContainer != null)
            {
                ServiceLocator.Instance.GetVFXManager()?.PlayTrickWinEffect(
                    trickContainer.position);
            }
            
            // Počkat a pak schovat karty daného štychu (ne všechny v trickContainer!)
            StartCoroutine(ClearTrickAfterDelay(new List<Card>(cards)));
        }
        
        private void HandleTrumpDeclared(CardSuit trumpSuit)
        {
            Debug.Log($"[MariasGameManager] Trumfy: {trumpSuit}");
            
            // Zobrazit trumfovou barvu
            if (trumpIndicator != null)
            {
                trumpIndicator.gameObject.SetActive(true);
                trumpIndicator.color = GetSuitColor(trumpSuit);
            }
            
            if (trumpText != null)
            {
                trumpText.text = GetSuitSymbol(trumpSuit);
                trumpText.gameObject.SetActive(true);
            }
        }
        
        private void HandleMarriageDeclared(CardSuit suit, int points)
        {
            Debug.Log($"[MariasGameManager] Hláška! {suit} = {points} bodů");
            
            ServiceLocator.Instance.GetAudioManager()?.PlayMarriageCall();
            
            if (playerHandContainer != null)
            {
                ServiceLocator.Instance.GetVFXManager()?.PlayMarriageEffect(
                    playerHandContainer.position);
            }
        }
        
        private void HandleScoreChanged(int playerIndex, int newScore)
        {
            if (playerIndex == LocalPlayerIndex)
            {
                playerInfoPanel?.UpdateScore(newScore);
            }
            else if (playerIndex - 1 < enemyInfoPanels.Length)
            {
                enemyInfoPanels[playerIndex - 1]?.UpdateScore(newScore);
            }
        }
        
        private void HandleGameEnded(int winnerIndex, MariasGameRules.GameResult result)
        {
            Debug.Log($"[MariasGameManager] Hra skončila! Vítěz: {winnerIndex}");
            
            if (winnerIndex == LocalPlayerIndex)
            {
                ServiceLocator.Instance.GetAudioManager()?.PlayGameWin();
                ServiceLocator.Instance.GetVFXManager()?.PlayConfettiEffect(Vector3.zero);
            }
            else
            {
                ServiceLocator.Instance.GetAudioManager()?.PlayGameLose();
            }
        }
        
        private void HandleLegalPlaysCalculated(List<Card> legalPlays)
        {
            // Zvýraznit hratelné karty pouze pro lokálního hráče
            if (_gameController.State.CurrentPlayerIndex != LocalPlayerIndex)
                return;

            foreach (var kvp in _cardViews)
            {
                // Pouze karty v ruce hráče
                if (kvp.Value.transform.parent == playerHandContainer)
                {
                    bool isPlayable = legalPlays.Contains(kvp.Key);
                    SetCardInteractable(kvp.Value, isPlayable);

                    // Drag povolit jen pro hratelné karty
                    var dragHandler = kvp.Value.GetComponent<CardDragHandler>();
                    dragHandler?.SetDraggable(isPlayable);
                }
            }
        }
        
        private void HandleSevenDeclared()
        {
            Debug.Log("[MariasGameManager] Sedma!");
            ServiceLocator.Instance.GetAudioManager()?.PlaySevenCall();
        }
        
        private void HandleBettelDeclared()
        {
            Debug.Log("[MariasGameManager] Betl!");
            ServiceLocator.Instance.GetAudioManager()?.PlayBettelCall();
        }
        
        private void HandleDurchDeclared()
        {
            Debug.Log("[MariasGameManager] Durch!");
            ServiceLocator.Instance.GetAudioManager()?.PlayDurchCall();
        }
        
        #endregion
        
        #region UI Methods
        
        private Transform GetHandContainer(int playerIndex)
        {
            if (playerIndex == LocalPlayerIndex)
                return playerHandContainer;
            
            int enemyIndex = playerIndex - 1;
            if (enemyIndex >= 0 && enemyIndex < enemyHandContainers.Length)
                return enemyHandContainers[enemyIndex];
            
            return null;
        }
        
        private CardView CreateCardView(Card card, Transform container, bool isEnemy)
        {
            if (cardViewPrefab == null || container == null)
            {
                Debug.LogWarning("[MariasGameManager] CardView prefab or container is null!");
                return null;
            }
            
            // DŮLEŽITÉ: worldPositionStays = false pro správné pozicování UI elementů v Canvasu
            var cardView = Instantiate(cardViewPrefab, container, false);
            
            // Kompletní reset RectTransform
            var rectTransform = cardView.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.localScale = Vector3.one;
                rectTransform.localRotation = Quaternion.identity;
                rectTransform.anchoredPosition = Vector2.zero;
                
                // Nastavit anchory na střed (layout je posune)
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                rectTransform.pivot = new Vector2(0.5f, 0.5f);
            }
            
            var cardWithState = new CardWithState(card.Suit, card.Rank, CardState.InHand);
            
            cardView.SetCard(cardWithState, _cardDataProvider, _themeProvider, isEnemy);
            
            // Přidat tap/drag handler pro hráčovy karty
            if (!isEnemy)
            {
                // Zajistit raycast pro detekci dotyku
                var image = cardView.GetComponent<Image>();
                if (image != null)
                {
                    image.raycastTarget = true;
                }

                var dragHandler = cardView.gameObject.AddComponent<CardDragHandler>();
                var capturedCard = card;

                // Tap → nadzvednutí / výběr pro discard
                dragHandler.OnTapped += () => OnCardTapped(capturedCard);

                // Vytažení nahoru přes práh → zahrání karty
                dragHandler.OnPlayTriggered += () => OnCardDropped(capturedCard);

                // Drag je povolen jen v Playing fázi (nastaví se dynamicky)
                dragHandler.SetDraggable(false);
            }
            
            _cardViews[card] = cardView;
            return cardView;
        }
        
        /// <summary>
        /// Tap na kartu — nadzvedne ji nebo vybere pro discard.
        /// </summary>
        private void OnCardTapped(Card card)
        {
            var phase = _gameController.State.Phase;

            switch (phase)
            {
                case GamePhase.DiscardingTalon:
                    ToggleCardForDiscard(card);
                    break;
                default:
                    if (_cardViews.TryGetValue(card, out var cardView))
                    {
                        ToggleCardLift(cardView);
                    }
                    break;
            }
        }

        /// <summary>
        /// Swipe nahoru přes práh — zahraje kartu nebo ji vybere k odhození.
        /// </summary>
        private void OnCardDropped(Card card)
        {
            var phase = _gameController.State.Phase;

            if (phase == GamePhase.Playing)
            {
                TryPlayCard(card);
            }
            else if (phase == GamePhase.DiscardingTalon)
            {
                SwipeCardForDiscard(card);
            }
        }

        /// <summary>
        /// Přepíná nadzvednutí karty (vizuální feedback).
        /// </summary>
        private void ToggleCardLift(CardView cardView)
        {
            var rect = cardView.GetComponent<RectTransform>();
            if (rect != null)
            {
                bool isLifted = rect.anchoredPosition.y > 10f;
                float newY = isLifted ? 0f : 30f;
                rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, newY);
            }
        }
        
        private void TryPlayCard(Card card)
        {
            if (_gameController.State.CurrentPlayerIndex != LocalPlayerIndex)
            {
                Debug.Log("Nejsi na tahu!");
                return;
            }
            
            bool success = _gameController.PlayCard(LocalPlayerIndex, card);
            if (!success)
            {
                Debug.Log("Tuto kartu nelze zahrát!");
                // TODO: Shake animation nebo zvuk chyby
            }
        }
        
        /// <summary>
        /// Swipe pro odhození karty do talonu (bez vizuálního nadzvednutí — to řeší throw animace).
        /// </summary>
        private void SwipeCardForDiscard(Card card)
        {
            if (_selectedForDiscard.Contains(card)) return;
            if (_selectedForDiscard.Count >= 2) return;

            _selectedForDiscard.Add(card);

            Debug.Log($"Swipe odhození: {_selectedForDiscard.Count}/2");

            if (_selectedForDiscard.Count == 2)
            {
                StartCoroutine(ConfirmDiscardAfterDelay(0.5f));
            }
        }

        private void ToggleCardForDiscard(Card card)
        {
            if (_selectedForDiscard.Contains(card))
            {
                _selectedForDiscard.Remove(card);
                SetCardSelected(_cardViews[card], false);
            }
            else if (_selectedForDiscard.Count < 2)
            {
                _selectedForDiscard.Add(card);
                SetCardSelected(_cardViews[card], true);
            }
            
            Debug.Log($"Vybráno k odhození: {_selectedForDiscard.Count}/2");
            
            // Automaticky potvrdit, když jsou vybrány 2 karty
            if (_selectedForDiscard.Count == 2)
            {
                // Malé zpoždění pro vizuální feedback
                StartCoroutine(ConfirmDiscardAfterDelay(0.5f));
            }
        }
        
        private System.Collections.IEnumerator ConfirmDiscardAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            ConfirmDiscard();
        }
        
        public void ConfirmDiscard()
        {
            if (_selectedForDiscard.Count != 2)
            {
                Debug.Log("Musíš vybrat přesně 2 karty!");
                return;
            }

            _gameController.DiscardToTalon(_selectedForDiscard);

            // Přesunout odhozené karty do talonPanelu (lícem dolů)
            foreach (var card in _selectedForDiscard)
            {
                if (_cardViews.TryGetValue(card, out var cardView))
                {
                    if (talonPanel != null)
                    {
                        // Reaktivovat (ThrowAway ji deaktivuje) a resetovat pozici
                        cardView.gameObject.SetActive(true);
                        var rect = cardView.GetComponent<RectTransform>();
                        if (rect != null) rect.anchoredPosition = Vector2.zero;

                        cardView.FlipCard(false);
                        cardView.transform.SetParent(talonPanel.transform, false);
                    }
                    else
                    {
                        _cardViews.Remove(card);
                        Destroy(cardView.gameObject);
                    }
                }
            }

            // Rozložit karty v talonPanelu
            if (talonPanel != null)
            {
                ArrangeCardsInHand(talonPanel.transform);
            }

            // Aktualizovat layout ruky
            ArrangeCardsInHand(playerHandContainer);

            _selectedForDiscard.Clear();
        }
        
        private void SetCardInteractable(CardView cardView, bool interactable)
        {
            var canvasGroup = cardView.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = cardView.gameObject.AddComponent<CanvasGroup>();
            
            canvasGroup.alpha = interactable ? 1f : 0.5f;
            canvasGroup.interactable = interactable;
            canvasGroup.blocksRaycasts = interactable;
        }
        
        private void SetCardSelected(CardView cardView, bool selected)
        {
            var rect = cardView.GetComponent<RectTransform>();
            if (rect != null)
            {
                Vector2 pos = rect.anchoredPosition;
                pos.y = selected ? 30f : 0f;
                rect.anchoredPosition = pos;
            }
        }
        
        private void PositionCardInTrick(CardView cardView, int position)
        {
            var rect = cardView.GetComponent<RectTransform>();
            if (rect == null) return;
            
            // Pozice: 0 = střed, 1 = vlevo, 2 = vpravo
            float xOffset = position switch
            {
                1 => -80f,
                2 => 80f,
                _ => 0f
            };
            
            rect.anchoredPosition = new Vector2(xOffset, 0);
        }
        
        /// <summary>
        /// Oznámí CardHandLayout komponentě, že se změnil počet karet.
        /// Layout logiku řeší CardHandLayout (SRP).
        /// </summary>
        private void ArrangeCardsInHand(Transform container)
        {
            if (container == null) return;

            var handLayout = container.GetComponent<CardHandLayout>();
            if (handLayout == null)
            {
                handLayout = container.gameObject.AddComponent<CardHandLayout>();
            }
            handLayout.SetDirty();
        }

        private IEnumerator MoveTalonCardsToHand(List<Card> talonCards)
        {
            yield return new WaitForSeconds(2f);

            foreach (var card in talonCards)
            {
                if (_cardViews.TryGetValue(card, out var cardView))
                {
                    cardView.transform.SetParent(playerHandContainer, false);

                    var dragHandler = cardView.GetComponent<CardDragHandler>();
                    dragHandler?.SetDraggable(true);
                }
            }

            ArrangeCardsInHand(playerHandContainer);
        }

        private void EnablePlayerCardDrags()
        {
            foreach (var kvp in _cardViews)
            {
                if (kvp.Value.transform.parent == playerHandContainer)
                {
                    var dragHandler = kvp.Value.GetComponent<CardDragHandler>();
                    dragHandler?.SetDraggable(true);
                }
            }
        }

        private void DisableAllCardDrags()
        {
            foreach (var kvp in _cardViews)
            {
                var dragHandler = kvp.Value.GetComponent<CardDragHandler>();
                dragHandler?.SetDraggable(false);
            }
        }
        
        private void UpdatePanelsForPhase(GamePhase phase)
        {
            HideAllPanels();

            switch (phase)
            {
                case GamePhase.Bidding:
                    biddingModal?.Show();
                    break;

                case GamePhase.TakingTalon:
                case GamePhase.DiscardingTalon:
                    if (talonPanel != null) talonPanel.SetActive(true);
                    break;

                case GamePhase.Declaring:
                    trumpSelectionModal?.Show();
                    break;
            }
        }

        private void HideAllPanels()
        {
            biddingModal?.Hide();
            trumpSelectionModal?.Hide();
            marriageButton?.HideButton();
            if (talonPanel != null) talonPanel.SetActive(false);
        }
        
        private void UpdatePhaseText(GamePhase phase)
        {
            if (gamePhaseText == null) return;
            
            gamePhaseText.text = phase switch
            {
                GamePhase.Dealing => "Rozdávání...",
                GamePhase.Bidding => "Dražba",
                GamePhase.TakingTalon => "Talon",
                GamePhase.DiscardingTalon => "Odhoď 2 karty",
                GamePhase.Declaring => "Zvol trumfy",
                GamePhase.Playing => "Hraj!",
                GamePhase.Scoring => "Počítání...",
                GamePhase.GameOver => "Konec hry",
                _ => ""
            };
        }
        
        private void HighlightActivePlayer(int playerIndex)
        {
            // Reset všech
            playerInfoPanel?.SetHighlight(false);
            foreach (var panel in enemyInfoPanels)
            {
                panel?.SetHighlight(false);
            }
            
            // Zvýraznit aktivního
            if (playerIndex == LocalPlayerIndex)
            {
                playerInfoPanel?.SetHighlight(true);
            }
            else if (playerIndex - 1 < enemyInfoPanels.Length)
            {
                enemyInfoPanels[playerIndex - 1]?.SetHighlight(true);
            }
        }
        
        private void UpdatePlayerCards(int playerIndex)
        {
            int cardCount = _gameController.State.PlayerHands[playerIndex].Count;
            
            if (playerIndex == LocalPlayerIndex)
            {
                // Player info nepotřebuje počet karet
            }
            else if (playerIndex - 1 < enemyInfoPanels.Length)
            {
                enemyInfoPanels[playerIndex - 1]?.UpdateCardCount(cardCount);
            }
        }
        
        private void ClearAllCards()
        {
            foreach (var cardView in _cardViews.Values)
            {
                if (cardView != null)
                    Destroy(cardView.gameObject);
            }
            _cardViews.Clear();
        }
        
        private Color GetSuitColor(CardSuit suit)
        {
            return suit switch
            {
                CardSuit.Hearts => Color.red,
                CardSuit.Diamonds => Color.red,
                CardSuit.Clubs => Color.black,
                CardSuit.Spades => Color.black,
                _ => Color.white
            };
        }
        
        private string GetSuitSymbol(CardSuit suit)
        {
            return suit switch
            {
                CardSuit.Hearts => "♥",
                CardSuit.Diamonds => "♦",
                CardSuit.Clubs => "♣",
                CardSuit.Spades => "♠",
                _ => ""
            };
        }
        
        private IEnumerator ClearTrickAfterDelay(List<Card> trickCards)
        {
            yield return new WaitForSeconds(trickClearDelay);

            // Zničit pouze CardViews z dokončeného štychu (ne nově zahrané karty dalšího štychu)
            foreach (var card in trickCards)
            {
                if (_cardViews.TryGetValue(card, out var cardView))
                {
                    if (cardView != null)
                    {
                        Destroy(cardView.gameObject);
                    }
                    _cardViews.Remove(card);
                }
            }
        }
        
        private IEnumerator AIPlayCoroutine(int playerIndex)
        {
            yield return new WaitForSeconds(aiThinkDelay);
            
            // AI zahraje náhodnou legální kartu
            var legalPlays = _gameController.GetLegalPlays();
            if (legalPlays.Count > 0)
            {
                var randomCard = legalPlays[Random.Range(0, legalPlays.Count)];
                _gameController.PlayCard(playerIndex, randomCard);
            }
        }
        
        private IEnumerator AIBidCoroutine(int playerIndex)
        {
            yield return new WaitForSeconds(aiThinkDelay);
            
            // AI vždy passuje v základní verzi
            // TODO: Implementovat lepší AI dražbu
            _gameController.MakeBid(playerIndex, MariasGameRules.BidOption.Pass);
        }
        
        #endregion
        
        #region Public Callbacks

        public void OnNewGame()
        {
            StartNewGame();
        }

        #endregion
    }
}
