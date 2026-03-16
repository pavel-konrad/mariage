using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MariasGame.Core;

namespace MariasGame.UI
{
    /// <summary>
    /// Modal pro výběr trumfové barvy.
    /// Vyjíždí při fázi Declaring, po výběru se schová.
    /// </summary>
    public class TrumpSelectionModal : ModalBase
    {
        [Header("Suit Buttons")]
        [SerializeField] private Button heartsButton;
        [SerializeField] private Button diamondsButton;
        [SerializeField] private Button clubsButton;
        [SerializeField] private Button spadesButton;

        [Header("Labels")]
        [SerializeField] private TMP_Text titleText;

        /// <summary>
        /// Vyvolán po výběru trumfové barvy.
        /// </summary>
        public event Action<CardSuit> OnTrumpSelected;

        protected override void Awake()
        {
            base.Awake();
            SetupButtons();
        }

        private void SetupButtons()
        {
            Debug.Log($"[TrumpSelectionModal] SetupButtons called. Hearts: {heartsButton != null}, Diamonds: {diamondsButton != null}, Clubs: {clubsButton != null}, Spades: {spadesButton != null}");
            
            if (heartsButton != null)
            {
                heartsButton.onClick.AddListener(() => {
                    Debug.Log("[TrumpSelectionModal] Hearts button clicked!");
                    SelectTrump(CardSuit.Hearts);
                });
            }
            if (diamondsButton != null)
            {
                diamondsButton.onClick.AddListener(() => {
                    Debug.Log("[TrumpSelectionModal] Diamonds button clicked!");
                    SelectTrump(CardSuit.Diamonds);
                });
            }
            if (clubsButton != null)
            {
                clubsButton.onClick.AddListener(() => {
                    Debug.Log("[TrumpSelectionModal] Clubs button clicked!");
                    SelectTrump(CardSuit.Clubs);
                });
            }
            if (spadesButton != null)
            {
                spadesButton.onClick.AddListener(() => {
                    Debug.Log("[TrumpSelectionModal] Spades button clicked!");
                    SelectTrump(CardSuit.Spades);
                });
            }
        }

        protected override void OnShow()
        {
            if (titleText != null)
                titleText.text = "Zvol trumfy";

            // Safety: ensure CanvasGroup becomes interactable after animation
            StartCoroutine(EnsureInteractableAfterAnimation());
        }

        private IEnumerator EnsureInteractableAfterAnimation()
        {
            // Wait longer than animation duration (0.3s default)
            yield return new WaitForSeconds(0.5f);

            var cg = GetComponent<CanvasGroup>();
            if (cg != null)
            {
                if (!cg.interactable)
                {
                    Debug.LogWarning("[TrumpSelectionModal] CanvasGroup was NOT interactable after animation! Forcing interactable=true.");
                    cg.interactable = true;
                    cg.blocksRaycasts = true;
                    cg.alpha = 1f;
                }

                // Validate buttons are reachable
                Debug.Log($"[TrumpSelectionModal] State after show: interactable={cg.interactable}, alpha={cg.alpha}, blocksRaycasts={cg.blocksRaycasts}");
            }
        }

        private void SelectTrump(CardSuit suit)
        {
            Debug.Log($"[TrumpSelectionModal] SelectTrump({suit}) called. Has subscribers: {OnTrumpSelected != null}");
            OnTrumpSelected?.Invoke(suit);
            Hide();
        }

    }
}
