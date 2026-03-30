using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MariasGame.UI
{
    /// <summary>
    /// Jedna položka v gridu výběru avatara.
    /// </summary>
    public class AvatarGridItemView : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private Image _avatarImage;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private GameObject _selectedMarker;

        public Sprite AvatarSprite => _avatarImage != null ? _avatarImage.sprite : null;

        public void Bind(string avatarName, Sprite avatarSprite, Action onSelected)
        {
            gameObject.SetActive(true);
            if (_nameText != null) _nameText.text = avatarName;
            if (_avatarImage != null)
            {
                _avatarImage.sprite = avatarSprite;
                _avatarImage.enabled = true;
            }

            if (_button != null)
            {
                _button.enabled = true;
                _button.interactable = true;
                _button.onClick.RemoveAllListeners();
                _button.onClick.AddListener(() => onSelected?.Invoke());
            }
        }

        public void SetSelected(bool selected)
        {
            if (_selectedMarker != null)
                _selectedMarker.SetActive(selected);
        }
    }
}
