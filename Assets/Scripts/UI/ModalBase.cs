using System.Collections;
using UnityEngine;

namespace MariasGame.UI
{
    /// <summary>
    /// Abstraktní základ pro modální UI panely.
    /// Poskytuje animaci vyjetí/zajetí (slide up/down) a fade pozadí.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class ModalBase : MonoBehaviour
    {
        [Header("Modal References")]
        [SerializeField] private RectTransform modalPanel;

        [Header("Animation")]
        [SerializeField] private float animationDuration = 0.3f;
        [SerializeField] private AnimationCurve animationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        private CanvasGroup _canvasGroup;
        private Coroutine _animationCoroutine;
        private float _hiddenYOffset;
        private bool _isShowingOrHiding; // Flag pro zabránění konfliktu s Awake

        /// <summary>Je modal právě viditelný?</summary>
        public bool IsVisible { get; private set; }

        protected virtual void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();

            // Výchozí stav: skrytý
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            
            // Pouze vypnout, pokud NEJSME uprostřed Show()
            if (!_isShowingOrHiding)
            {
                gameObject.SetActive(false);
            }
            IsVisible = false;
        }

        /// <summary>
        /// Zobrazí modal s animací.
        /// </summary>
        public void Show()
        {
            if (IsVisible) return;

            _isShowingOrHiding = true;
            gameObject.SetActive(true);
            _isShowingOrHiding = false;
            
            IsVisible = true;

            if (_animationCoroutine != null)
                StopCoroutine(_animationCoroutine);

            _animationCoroutine = StartCoroutine(AnimateShow());
            OnShow();
        }

        /// <summary>
        /// Skryje modal s animací.
        /// </summary>
        public void Hide()
        {
            if (!IsVisible) return;

            IsVisible = false;

            if (_animationCoroutine != null)
                StopCoroutine(_animationCoroutine);

            _animationCoroutine = StartCoroutine(AnimateHide());
            OnHide();
        }

        /// <summary>Voláno při zobrazení modalu. Override v subclassech.</summary>
        protected virtual void OnShow() { }

        /// <summary>Voláno při skrytí modalu. Override v subclassech.</summary>
        protected virtual void OnHide() { }

        private IEnumerator AnimateShow()
        {
            if (modalPanel == null)
            {
                // Bez panelu — jen fade
                _canvasGroup.alpha = 1f;
                _canvasGroup.interactable = true;
                _canvasGroup.blocksRaycasts = true;
                yield break;
            }

            _hiddenYOffset = -modalPanel.rect.height;
            Vector2 startPos = new Vector2(modalPanel.anchoredPosition.x, _hiddenYOffset);
            Vector2 endPos = new Vector2(modalPanel.anchoredPosition.x, 0f);

            modalPanel.anchoredPosition = startPos;
            _canvasGroup.blocksRaycasts = true;

            float elapsed = 0f;
            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                float t = animationCurve.Evaluate(Mathf.Clamp01(elapsed / animationDuration));

                modalPanel.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
                _canvasGroup.alpha = t;

                yield return null;
            }

            modalPanel.anchoredPosition = endPos;
            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = true;
        }

        private IEnumerator AnimateHide()
        {
            if (modalPanel == null)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;
                gameObject.SetActive(false);
                yield break;
            }

            _canvasGroup.interactable = false;

            Vector2 startPos = modalPanel.anchoredPosition;
            Vector2 endPos = new Vector2(modalPanel.anchoredPosition.x, -modalPanel.rect.height);

            float elapsed = 0f;
            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                float t = animationCurve.Evaluate(Mathf.Clamp01(elapsed / animationDuration));

                modalPanel.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
                _canvasGroup.alpha = 1f - t;

                yield return null;
            }

            modalPanel.anchoredPosition = endPos;
            _canvasGroup.alpha = 0f;
            _canvasGroup.blocksRaycasts = false;
            gameObject.SetActive(false);
        }

    }
}
