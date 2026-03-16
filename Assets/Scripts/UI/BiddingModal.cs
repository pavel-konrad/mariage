using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MariasGame.Core;

namespace MariasGame.UI
{
    /// <summary>
    /// Modal pro dražbu. Zobrazuje tlačítka pro jednotlivé nabídky.
    /// Vyjíždí při fázi Bidding, po výběru se schová.
    /// </summary>
    public class BiddingModal : ModalBase
    {
        [Header("Bidding Buttons")]
        [SerializeField] private Button gameButton;
        [SerializeField] private Button sevenButton;
        [SerializeField] private Button hundredButton;
        [SerializeField] private Button bettelButton;
        [SerializeField] private Button durchButton;
        [SerializeField] private Button passButton;

        [Header("Button Labels")]
        [SerializeField] private TMP_Text titleText;

        /// <summary>
        /// Vyvolán po výběru nabídky hráčem.
        /// </summary>
        public event Action<MariasGameRules.BidOption> OnBidSelected;

        protected override void Awake()
        {
            base.Awake();
            SetupButtons();
        }

        private void SetupButtons()
        {
            gameButton?.onClick.AddListener(() => SelectBid(MariasGameRules.BidOption.Game));
            sevenButton?.onClick.AddListener(() => SelectBid(MariasGameRules.BidOption.Seven));
            hundredButton?.onClick.AddListener(() => SelectBid(MariasGameRules.BidOption.Hundred));
            bettelButton?.onClick.AddListener(() => SelectBid(MariasGameRules.BidOption.Bettel));
            durchButton?.onClick.AddListener(() => SelectBid(MariasGameRules.BidOption.Durch));
            passButton?.onClick.AddListener(() => SelectBid(MariasGameRules.BidOption.Pass));
        }

        protected override void OnShow()
        {
            if (titleText != null)
                titleText.text = "Dražba";
        }

        private void SelectBid(MariasGameRules.BidOption bid)
        {
            OnBidSelected?.Invoke(bid);
            Hide();
        }

    }
}
