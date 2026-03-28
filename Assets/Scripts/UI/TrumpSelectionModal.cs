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
            if (heartsButton != null)
                heartsButton.onClick.AddListener(() => SelectTrump(CardSuit.Hearts));

            if (diamondsButton != null)
                diamondsButton.onClick.AddListener(() => SelectTrump(CardSuit.Diamonds));

            if (clubsButton != null)
                clubsButton.onClick.AddListener(() => SelectTrump(CardSuit.Clubs));

            if (spadesButton != null)
                spadesButton.onClick.AddListener(() => SelectTrump(CardSuit.Spades));
        }

        protected override void OnShow()
        {
            if (titleText != null)
                titleText.text = "Zvol trumfy";

            StartCoroutine(EnsureInteractableAfterAnimation());
        }

        private IEnumerator EnsureInteractableAfterAnimation()
        {
            yield return new WaitForSeconds(0.5f);

            var cg = GetComponent<CanvasGroup>();
            if (cg != null && !cg.interactable)
            {
                Debug.LogWarning("[TrumpSelectionModal] CanvasGroup was NOT interactable after animation! Forcing interactable=true.");
                cg.interactable = true;
                cg.blocksRaycasts = true;
                cg.alpha = 1f;
            }
        }

        private void SelectTrump(CardSuit suit)
        {
            OnTrumpSelected?.Invoke(suit);
            Hide();
        }
    }
}
