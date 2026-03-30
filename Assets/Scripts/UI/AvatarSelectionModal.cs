using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using MariasGame.ScriptableObjects;

namespace MariasGame.UI
{
    /// <summary>
    /// Modal s dynamickým gridem buttonů pro výběr postavy.
    /// Po výběru zavře modal a vrátí vybraný objekt callbackem.
    /// </summary>
    public class AvatarSelectionModal : ModalBase
    {
        [Header("UI")]
        [SerializeField] private Transform _gridRoot;
        [SerializeField] private GameObject _gridItemPrefab;
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private Button _closeButton;

        private readonly List<AvatarGridItemView> _spawnedItems = new();

        protected override void Awake()
        {
            base.Awake();
            if (_closeButton != null)
            {
                _closeButton.onClick.RemoveAllListeners();
                _closeButton.onClick.AddListener(Hide);
            }
        }

        public void Show(IReadOnlyList<CharacterData> characters, Action<CharacterData> onSelected, int selectedId, string title = "Choose character")
        {
            if (_titleText != null) _titleText.text = title;
            Show();
            BuildGrid(characters, onSelected, selectedId);
        }

        private void BuildGrid(IReadOnlyList<CharacterData> characters, Action<CharacterData> onSelected, int selectedId)
        {
            ClearGrid();
            if (_gridItemPrefab == null || characters == null) return;
            var targetRoot = ResolveGridRoot();
            if (targetRoot == null) return;
            EnsureRootVisible(targetRoot);

            foreach (var character in characters)
            {
                if (character == null) continue;
                var go = Instantiate(_gridItemPrefab, targetRoot);
                var item = go.GetComponent<AvatarGridItemView>();
                if (item == null)
                {
                    Debug.LogError("[AvatarSelectionModal] Grid item prefab must contain AvatarGridItemView.");
                    continue;
                }

                item.Bind(character.CharacterName, character.CharacterSprite, () =>
                {
                    onSelected?.Invoke(character);
                    Hide();
                });
                item.SetSelected(character.CharacterId == selectedId);
                _spawnedItems.Add(item);
            }
        }

        private void ClearGrid()
        {
            foreach (var item in _spawnedItems)
            {
                if (item != null) Destroy(item.gameObject);
            }
            _spawnedItems.Clear();
        }

        private Transform ResolveGridRoot()
        {
            if (_gridRoot == null)
            {
                Debug.LogError("[AvatarSelectionModal] Grid root is not assigned.");
                return null;
            }

            if (_gridRoot.GetComponent<GridLayoutGroup>() != null)
                return _gridRoot;

            var namedChild = _gridRoot.Find("AvatarGrid");
            if (namedChild != null)
                return namedChild;

            var nestedGrid = _gridRoot.GetComponentInChildren<GridLayoutGroup>(true);
            if (nestedGrid != null)
                return nestedGrid.transform;

            Debug.LogError("[AvatarSelectionModal] No GridLayoutGroup found under assigned Grid Root.");
            return null;
        }

        private static void EnsureRootVisible(Transform targetRoot)
        {
            if (targetRoot == null) return;
            var current = targetRoot;
            while (current != null)
            {
                if (!current.gameObject.activeSelf)
                    current.gameObject.SetActive(true);
                current = current.parent;
            }
        }
    }
}
