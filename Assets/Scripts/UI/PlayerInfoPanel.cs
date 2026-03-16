using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MariasGame.Core;

namespace MariasGame.UI
{
    /// <summary>
    /// Panel s informacemi o hráči.
    /// Zobrazuje avatar, jméno, skóre a počet karet.
    /// </summary>
    public class PlayerInfoPanel : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image avatarImage;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text goldText;
        [SerializeField] private TMP_Text pointsText;
        [SerializeField] private TMP_Text cardCountText;
        
        [Header("Highlight")]
        [SerializeField] private Image highlightBorder;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Color normalColor = new Color(0.42f, 0.55f, 0.47f);
        [SerializeField] private Color highlightColor = new Color(0.52f, 0.65f, 0.57f);
        
        [Header("Declaration Icons")]
        [SerializeField] private Image declarationIcon;
        [SerializeField] private Sprite heartSprite;
        [SerializeField] private Sprite diamondSprite;
        [SerializeField] private Sprite clubSprite;
        [SerializeField] private Sprite spadeSprite;
        
        private bool _isHighlighted = false;
        
        void Awake()
        {
            // Skrýt deklarační ikonu na začátku
            if (declarationIcon != null)
                declarationIcon.gameObject.SetActive(false);
        }
        
        /// <summary>
        /// Nastaví informace o hráči.
        /// </summary>
        public void SetPlayerInfo(string playerName, Sprite avatar = null)
        {
            if (nameText != null)
                nameText.text = playerName;
            
            if (avatarImage != null && avatar != null)
                avatarImage.sprite = avatar;
        }
        
        /// <summary>
        /// Aktualizuje zobrazení skóre (bodů ze štychů + hlášek).
        /// </summary>
        public void UpdateScore(int points)
        {
            if (pointsText != null)
                pointsText.text = points.ToString();
        }
        
        /// <summary>
        /// Aktualizuje zobrazení zlata (peněz).
        /// </summary>
        public void UpdateGold(int gold)
        {
            if (goldText != null)
                goldText.text = gold.ToString();
        }
        
        /// <summary>
        /// Aktualizuje počet karet v ruce (pro AI hráče).
        /// </summary>
        public void UpdateCardCount(int count)
        {
            if (cardCountText != null)
            {
                cardCountText.text = count.ToString();
                cardCountText.gameObject.SetActive(count > 0);
            }
        }
        
        /// <summary>
        /// Nastaví zvýraznění (aktivní hráč).
        /// </summary>
        public void SetHighlight(bool highlighted)
        {
            _isHighlighted = highlighted;
            
            if (highlightBorder != null)
            {
                highlightBorder.enabled = highlighted;
            }
            
            if (backgroundImage != null)
            {
                backgroundImage.color = highlighted ? highlightColor : normalColor;
            }
        }
        
        /// <summary>
        /// Zobrazí ikonu deklarace (trumfová barva nebo hláška).
        /// </summary>
        public void ShowDeclaration(CardSuit suit)
        {
            if (declarationIcon == null) return;
            
            declarationIcon.gameObject.SetActive(true);
            declarationIcon.sprite = suit switch
            {
                CardSuit.Hearts => heartSprite,
                CardSuit.Diamonds => diamondSprite,
                CardSuit.Clubs => clubSprite,
                CardSuit.Spades => spadeSprite,
                _ => null
            };
        }
        
        /// <summary>
        /// Skryje ikonu deklarace.
        /// </summary>
        public void HideDeclaration()
        {
            if (declarationIcon != null)
                declarationIcon.gameObject.SetActive(false);
        }
        
        /// <summary>
        /// Animuje změnu skóre.
        /// </summary>
        public void AnimateScoreChange(int delta)
        {
            // TODO: Implementovat animaci +/- bodů
            // Např. zobrazit floating text s "+20" nebo "-10"
        }
    }
}
