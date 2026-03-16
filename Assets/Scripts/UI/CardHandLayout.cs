using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MariasGame.UI
{
    /// <summary>
    /// Dynamický layout pro karty v ruce.
    /// Automaticky rozestupuje karty - čím více karet, tím menší mezera.
    /// </summary>
    [ExecuteAlways]
    public class CardHandLayout : MonoBehaviour
    {
        [Header("Layout Settings")]
        [SerializeField] private float cardWidth = 80f;
        [SerializeField] private float maxSpacing = 90f;      // Maximální mezera (málo karet)
        [SerializeField] private float minSpacing = 20f;      // Minimální mezera (hodně karet) - overlap
        [SerializeField] private int cardsForMinSpacing = 12; // Při tomto počtu karet je min spacing
        
        [Header("Alignment")]
        [SerializeField] private bool centerCards = true;
        
        [Header("Animation")]
        [SerializeField] private bool animateLayout = true;
        [SerializeField] private float animationSpeed = 10f;
        
        private RectTransform _rectTransform;
        private List<RectTransform> _cards = new List<RectTransform>();
        private bool _isDirty = true;
        
        void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }
        
        void OnEnable()
        {
            _isDirty = true;
        }
        
        void Update()
        {
            if (_isDirty || transform.childCount != _cards.Count)
            {
                RefreshCardList();
                _isDirty = false;
            }
            
            UpdateLayout();
        }
        
        /// <summary>
        /// Označí layout jako potřebující aktualizaci.
        /// </summary>
        public void SetDirty()
        {
            _isDirty = true;
        }
        
        /// <summary>
        /// Aktualizuje seznam karet.
        /// </summary>
        private void RefreshCardList()
        {
            _cards.Clear();
            
            for (int i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                if (child.gameObject.activeSelf)
                {
                    var rect = child.GetComponent<RectTransform>();
                    if (rect != null)
                    {
                        _cards.Add(rect);
                    }
                }
            }
        }
        
        /// <summary>
        /// Aktualizuje pozice karet.
        /// </summary>
        private void UpdateLayout()
        {
            if (_cards.Count == 0) return;

            // Ochrana proti nulovým hodnotám (např. po přidání komponenty za běhu)
            if (cardWidth <= 0f) cardWidth = 80f;
            if (maxSpacing <= 0f) maxSpacing = 90f;
            if (minSpacing <= 0f) minSpacing = 20f;
            if (cardsForMinSpacing <= 0) cardsForMinSpacing = 12;

            float spacing = CalculateSpacing(_cards.Count);
            float totalWidth = (_cards.Count - 1) * spacing + cardWidth;

            float startX = centerCards ? -totalWidth / 2f + cardWidth / 2f : 0f;
            
            for (int i = 0; i < _cards.Count; i++)
            {
                var card = _cards[i];
                float targetX = startX + i * spacing;
                Vector2 targetPos = new Vector2(targetX, 0f);
                
                if (animateLayout && Application.isPlaying)
                {
                    // Plynulá animace
                    card.anchoredPosition = Vector2.Lerp(
                        card.anchoredPosition, 
                        targetPos, 
                        Time.deltaTime * animationSpeed
                    );
                }
                else
                {
                    // Okamžité nastavení (v editoru nebo bez animace)
                    card.anchoredPosition = targetPos;
                }
                
                // Nastavit sibling index pro správné překrývání (poslední karta nahoře)
                // card.SetSiblingIndex(i); // Odkomentovat pokud chcete, aby se karty překrývaly zleva doprava
            }
        }
        
        /// <summary>
        /// Vypočítá optimální spacing podle počtu karet.
        /// </summary>
        private float CalculateSpacing(int cardCount)
        {
            if (cardCount <= 1) return maxSpacing;
            if (cardCount >= cardsForMinSpacing) return minSpacing;
            
            // Lineární interpolace mezi max a min spacing
            float t = (float)(cardCount - 1) / (cardsForMinSpacing - 1);
            return Mathf.Lerp(maxSpacing, minSpacing, t);
        }
        
        /// <summary>
        /// Nastaví šířku karty.
        /// </summary>
        public void SetCardWidth(float width)
        {
            cardWidth = width;
            _isDirty = true;
        }
        
        /// <summary>
        /// Získá aktuální spacing.
        /// </summary>
        public float GetCurrentSpacing()
        {
            return CalculateSpacing(_cards.Count);
        }
        
#if UNITY_EDITOR
        void OnValidate()
        {
            _isDirty = true;
        }
#endif
    }
}
