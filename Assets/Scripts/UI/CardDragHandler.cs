using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MariasGame.UI
{
    /// <summary>
    /// Vertikální "zásobníkový" drag na kartě.
    /// Tap = nadzvednutí, táhnutí nahoru přes práh = okamžité zahrání/odhození.
    /// Karta reaguje už během držení (ne až po puštění).
    /// </summary>
    public class CardDragHandler : MonoBehaviour,
        IPointerDownHandler, IPointerUpHandler,
        IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("Tap")]
        [SerializeField] private float tapThreshold = 15f;

        [Header("Drag")]
        [SerializeField] private float playThreshold = 150f;
        [SerializeField] private float snapBackDuration = 0.15f;

        [Header("Throw")]
        [SerializeField] private float throwDuration = 0.2f;
        [SerializeField] private float throwDistance = 800f;

        // Events
        public event Action OnTapped;
        public event Action OnPlayTriggered;

        // State
        private RectTransform _rectTransform;
        private Canvas _rootCanvas;
        private Vector2 _pointerStartPos;
        private Vector2 _startAnchoredPos;
        private bool _isDragging;
        private bool _canDrag;
        private bool _triggered;
        private Coroutine _snapCoroutine;

        void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _rootCanvas = GetComponentInParent<Canvas>()?.rootCanvas;
        }

        public void SetDraggable(bool canDrag)
        {
            _canDrag = canDrag;
        }

        #region Pointer

        public void OnPointerDown(PointerEventData eventData)
        {
            _pointerStartPos = eventData.position;
            _startAnchoredPos = _rectTransform.anchoredPosition;
            _isDragging = false;
            _triggered = false;

            if (_snapCoroutine != null)
            {
                StopCoroutine(_snapCoroutine);
                _snapCoroutine = null;
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!_isDragging && !_triggered)
            {
                float dist = Vector2.Distance(_pointerStartPos, eventData.position);
                if (dist < tapThreshold)
                {
                    OnTapped?.Invoke();
                }
            }
        }

        #endregion

        #region Drag

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!_canDrag)
            {
                eventData.pointerDrag = null;
                return;
            }

            _isDragging = true;
            _triggered = false;
        }

        public void OnDrag(PointerEventData eventData)
{
    if (!_isDragging || _triggered) return;

    float scaleFactor = _rootCanvas != null ? _rootCanvas.scaleFactor : 1f;

    // Absolutní vzdálenost prstu od startu
    float dragY = (eventData.position.y - _pointerStartPos.y) / scaleFactor;

    // jen nahoru
    dragY = Mathf.Max(0f, dragY);

    float newY = _startAnchoredPos.y + dragY;
    _rectTransform.anchoredPosition = new Vector2(
        _startAnchoredPos.x,
        newY
    );

    if (dragY >= playThreshold)
    {
        _triggered = true;
        _isDragging = false;

        Vector2 throwFrom = _rectTransform.anchoredPosition;
        Transform parentBefore = transform.parent;
        OnPlayTriggered?.Invoke();

        // Pokud hra kartu přesunula (Playing → trickContainer), neodhazovat
        if (transform.parent != parentBefore)
            return;

        // Karta zůstala na místě (DiscardingTalon) → animace odhození
        StartCoroutine(ThrowAway(throwFrom));
    }
}


        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_isDragging) return;
            _isDragging = false;

            if (!_triggered)
            {
                // Nedosáhl prahu → snap zpět
                _snapCoroutine = StartCoroutine(SnapBack());
            }
        }

        #endregion

        /// <summary>
        /// Animace "odhození" — karta zrychlí směrem nahoru a zmizí.
        /// </summary>
        private IEnumerator ThrowAway(Vector2 from)
        {
            Vector2 to = new Vector2(from.x, from.y + throwDistance);
            float elapsed = 0f;

            while (elapsed < throwDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / throwDuration;
                // Ease-in pro efekt zrychlení
                float easedT = t * t;
                _rectTransform.anchoredPosition = Vector2.Lerp(from, to, easedT);
                yield return null;
            }

            _rectTransform.anchoredPosition = to;
            // Skrýt kartu (CardHandLayout přeskočí neaktivní děti)
            gameObject.SetActive(false);
        }

        private IEnumerator SnapBack()
        {
            Vector2 from = _rectTransform.anchoredPosition;
            float elapsed = 0f;

            while (elapsed < snapBackDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / snapBackDuration;
                _rectTransform.anchoredPosition = Vector2.Lerp(from, _startAnchoredPos, t);
                yield return null;
            }

            _rectTransform.anchoredPosition = _startAnchoredPos;
            _snapCoroutine = null;
        }
    }
}
