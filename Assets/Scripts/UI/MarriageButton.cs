using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MariasGame.Core;

namespace MariasGame.UI
{
    /// <summary>
    /// Tlačítko pro hlášení hlášky (Král + Dáma stejné barvy).
    /// Viditelné jen když hráč vynáší a má v ruce hlášku.
    /// </summary>
    public class MarriageButton : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Button button;
        [SerializeField] private TMP_Text buttonText;

        /// <summary>
        /// Vyvolán po kliknutí na tlačítko s barvou hlášky.
        /// </summary>
        public event Action<CardSuit> OnMarriageDeclared;

        private List<CardSuit> _possibleMarriages = new List<CardSuit>();

        void Awake()
        {
            if (button != null)
                button.onClick.AddListener(OnClick);

            gameObject.SetActive(false);
        }

        /// <summary>
        /// Aktualizuje stav tlačítka podle dostupných hlášek.
        /// Pokud žádné hlášky nejsou, tlačítko se skryje.
        /// </summary>
        public void UpdateState(List<CardSuit> possibleMarriages)
        {
            _possibleMarriages = possibleMarriages ?? new List<CardSuit>();

            if (_possibleMarriages.Count == 0)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);

            if (buttonText != null)
            {
                if (_possibleMarriages.Count == 1)
                {
                    buttonText.text = $"Hláška {GetSuitSymbol(_possibleMarriages[0])}";
                }
                else
                {
                    buttonText.text = "Hláška...";
                }
            }
        }

        /// <summary>
        /// Skryje tlačítko.
        /// </summary>
        public void HideButton()
        {
            gameObject.SetActive(false);
            _possibleMarriages.Clear();
        }

        private void OnClick()
        {
            if (_possibleMarriages.Count == 1)
            {
                // Jedna hláška — rovnou hlásit
                OnMarriageDeclared?.Invoke(_possibleMarriages[0]);
            }
            else if (_possibleMarriages.Count > 1)
            {
                // Více hlášek — hlásit první dostupnou
                // TODO: Popup pro výběr barvy, pokud hráč má víc hlášek najednou
                OnMarriageDeclared?.Invoke(_possibleMarriages[0]);
            }
        }

        private string GetSuitSymbol(CardSuit suit)
        {
            return suit switch
            {
                CardSuit.Hearts => "\u2665",
                CardSuit.Diamonds => "\u2666",
                CardSuit.Clubs => "\u2663",
                CardSuit.Spades => "\u2660",
                _ => ""
            };
        }

    }
}
