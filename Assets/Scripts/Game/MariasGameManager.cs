using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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
        
        [Header("Action Panels")]
        [SerializeField] private GameObject biddingPanel;
        [SerializeField] private GameObject trumpSelectionPanel;
        [SerializeField] private GameObject talonPanel;
        [SerializeField] private GameObject playPanel;
        
        [Header("Status UI")]
        [SerializeField] private Text gamePhaseText;
        [SerializeField] private Image trumpIndicator;
        [SerializeField] private Text trumpText;
        
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
            
            // Vytvořit game controller
            _gameController = new MariasGameController();
            
            // Subscribnout na eventy
            SubscribeToEvents();
            
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
            
            Transform container = GetHandContainer(playerIndex);
            bool isEnemy = playerIndex != LocalPlayerIndex;
            
            foreach (var card in cards)
            {
                CreateCardView(card, container, isEnemy);
            }
            
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
        }
        
        private void HandlePlayerTurnStarted(int playerIndex)
        {
            Debug.Log($"[MariasGameManager] Na tahu: Hráč {playerIndex}");
            
            // Zvýraznit aktivního hráče
            HighlightActivePlayer(playerIndex);
            
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
        }
        
        private void HandleCardPlayed(int playerIndex, Card card)
        {
            Debug.Log($"[MariasGameManager] Hráč {playerIndex} zahrál {card}");
            
            // Přesunout kartu do trick containeru
            if (_cardViews.TryGetValue(card, out var cardView))
            {
                // Otočit kartu lícem nahoru (i pro AI)
                cardView.FlipCard(true);
                
                // Přesunout do trick area
                cardView.transform.SetParent(trickContainer);
                
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
            
            // Počkat a pak schovat karty
            StartCoroutine(ClearTrickAfterDelay());
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
            
            var cardView = Instantiate(cardViewPrefab, container);
            var cardWithState = new CardWithState(card.Suit, card.Rank, CardState.InHand);
            
            cardView.SetCard(cardWithState, _cardDataProvider, _themeProvider, isEnemy);
            
            // Přidat click handler pro hráčovy karty
            if (!isEnemy)
            {
                var button = cardView.GetComponent<Button>();
                if (button == null) 
                    button = cardView.gameObject.AddComponent<Button>();
                
                // Capture card in closure
                var capturedCard = card;
                button.onClick.AddListener(() => OnCardClicked(capturedCard));
            }
            
            _cardViews[card] = cardView;
            return cardView;
        }
        
        private void OnCardClicked(Card card)
        {
            var state = _gameController.State;
            
            // V různých fázích různé chování
            switch (state.Phase)
            {
                case GamePhase.Playing:
                    TryPlayCard(card);
                    break;
                    
                case GamePhase.DiscardingTalon:
                    ToggleCardForDiscard(card);
                    break;
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
            
            // Pokud máme 2 karty, můžeme potvrdit
            Debug.Log($"Vybráno k odhození: {_selectedForDiscard.Count}/2");
        }
        
        public void ConfirmDiscard()
        {
            if (_selectedForDiscard.Count != 2)
            {
                Debug.Log("Musíš vybrat přesně 2 karty!");
                return;
            }
            
            _gameController.DiscardToTalon(_selectedForDiscard);
            
            // Zničit odhozené karty
            foreach (var card in _selectedForDiscard)
            {
                if (_cardViews.TryGetValue(card, out var cardView))
                {
                    _cardViews.Remove(card);
                    Destroy(cardView.gameObject);
                }
            }
            
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
        
        private void UpdatePanelsForPhase(GamePhase phase)
        {
            HideAllPanels();
            
            switch (phase)
            {
                case GamePhase.Bidding:
                    if (biddingPanel != null) biddingPanel.SetActive(true);
                    break;
                    
                case GamePhase.DiscardingTalon:
                    if (talonPanel != null) talonPanel.SetActive(true);
                    break;
                    
                case GamePhase.Declaring:
                    if (trumpSelectionPanel != null) trumpSelectionPanel.SetActive(true);
                    break;
                    
                case GamePhase.Playing:
                    if (playPanel != null) playPanel.SetActive(true);
                    break;
            }
        }
        
        private void HideAllPanels()
        {
            if (biddingPanel != null) biddingPanel.SetActive(false);
            if (trumpSelectionPanel != null) trumpSelectionPanel.SetActive(false);
            if (talonPanel != null) talonPanel.SetActive(false);
            if (playPanel != null) playPanel.SetActive(false);
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
        
        private IEnumerator ClearTrickAfterDelay()
        {
            yield return new WaitForSeconds(trickClearDelay);
            
            // Zničit CardViews ve trick containeru
            if (trickContainer != null)
            {
                var cardsToRemove = new List<Card>();
                
                foreach (var kvp in _cardViews)
                {
                    if (kvp.Value != null && kvp.Value.transform.parent == trickContainer)
                    {
                        cardsToRemove.Add(kvp.Key);
                        Destroy(kvp.Value.gameObject);
                    }
                }
                
                foreach (var card in cardsToRemove)
                {
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
        
        #region Bidding UI Callbacks
        
        public void OnBidGame()
        {
            if (_gameController.State.CurrentPlayerIndex == LocalPlayerIndex)
                _gameController.MakeBid(LocalPlayerIndex, MariasGameRules.BidOption.Game);
        }
        
        public void OnBidSeven()
        {
            if (_gameController.State.CurrentPlayerIndex == LocalPlayerIndex)
                _gameController.MakeBid(LocalPlayerIndex, MariasGameRules.BidOption.Seven);
        }
        
        public void OnBidHundred()
        {
            if (_gameController.State.CurrentPlayerIndex == LocalPlayerIndex)
                _gameController.MakeBid(LocalPlayerIndex, MariasGameRules.BidOption.Hundred);
        }
        
        public void OnBidBettel()
        {
            if (_gameController.State.CurrentPlayerIndex == LocalPlayerIndex)
                _gameController.MakeBid(LocalPlayerIndex, MariasGameRules.BidOption.Bettel);
        }
        
        public void OnBidDurch()
        {
            if (_gameController.State.CurrentPlayerIndex == LocalPlayerIndex)
                _gameController.MakeBid(LocalPlayerIndex, MariasGameRules.BidOption.Durch);
        }
        
        public void OnBidPass()
        {
            if (_gameController.State.CurrentPlayerIndex == LocalPlayerIndex)
                _gameController.MakeBid(LocalPlayerIndex, MariasGameRules.BidOption.Pass);
        }
        
        #endregion
        
        #region Trump Selection UI Callbacks
        
        public void OnSelectHearts()
        {
            if (_gameController.State.DeclarerIndex == LocalPlayerIndex)
                _gameController.DeclareTrump(CardSuit.Hearts);
        }
        
        public void OnSelectDiamonds()
        {
            if (_gameController.State.DeclarerIndex == LocalPlayerIndex)
                _gameController.DeclareTrump(CardSuit.Diamonds);
        }
        
        public void OnSelectClubs()
        {
            if (_gameController.State.DeclarerIndex == LocalPlayerIndex)
                _gameController.DeclareTrump(CardSuit.Clubs);
        }
        
        public void OnSelectSpades()
        {
            if (_gameController.State.DeclarerIndex == LocalPlayerIndex)
                _gameController.DeclareTrump(CardSuit.Spades);
        }
        
        #endregion
        
        #region Play UI Callbacks
        
        public void OnDeclareMarriage()
        {
            var marriages = _gameController.GetPossibleMarriages();
            if (marriages.Count > 0)
            {
                // Hlásit první dostupnou hlášku
                // TODO: Nechat hráče vybrat, pokud má více
                _gameController.DeclareMarriage(marriages[0]);
            }
        }
        
        public void OnNewGame()
        {
            StartNewGame();
        }
        
        #endregion
    }
}
