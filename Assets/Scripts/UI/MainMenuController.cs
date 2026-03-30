using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using MariasGame.Core;
using MariasGame.Managers;
using MariasGame.ScriptableObjects;

namespace MariasGame.UI
{
    /// <summary>
    /// Úvodní obrazovka: výběr postavy pro všechny 3 sloty a game modu.
    /// Všechny sloty sdílejí jeden CharacterDatabase pool. Každá postava může být
    /// přiřazena pouze jednomu slotu najednou.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        private enum Slot { Player, Enemy1, Enemy2 }

        [Header("Dependencies")]
        [SerializeField] private CharacterManager _characterManager;

        [Header("UI")]
        [SerializeField] private AvatarSelectionModal _selectionModal;
        [SerializeField] private Button _playerSelectButton;
        [SerializeField] private Button _enemy1SelectButton;
        [SerializeField] private Button _enemy2SelectButton;
        [SerializeField] private Image _playerPreviewImage;
        [SerializeField] private Image _enemy1PreviewImage;
        [SerializeField] private Image _enemy2PreviewImage;
        [SerializeField] private TMP_Text _playerNameText;
        [SerializeField] private TMP_Text _enemy1NameText;
        [SerializeField] private TMP_Text _enemy2NameText;
        [SerializeField] private TMP_Dropdown _gameModeDropdown;

        [Header("Game Mode Assets")]
        [SerializeField] private List<GameModeConfig> _availableModes = new();

        [Header("Scene")]
        [SerializeField] private string _gameSceneName = "GameScene";

        private readonly List<CharacterData> _characterOptions = new();
        private readonly List<GameModeConfig> _modeOptions = new();

        private int _selectedPlayerId = -1;
        private int _selectedEnemy1Id = -1;
        private int _selectedEnemy2Id = -1;

        void Start()
        {
            PopulateCharacterOptions();
            PopulateModeOptions();
            WireSelectionButtons();
            RefreshSelectedSlotsVisuals();
        }

        public void StartGame()
        {
            if (_selectedPlayerId < 0 || _selectedEnemy1Id < 0 || _selectedEnemy2Id < 0 || _modeOptions.Count == 0)
            {
                Debug.LogError("[MainMenuController] Missing selected character data or game mode.");
                return;
            }

            var player = _characterOptions.Find(c => c.CharacterId == _selectedPlayerId);
            string playerName = player != null && !string.IsNullOrWhiteSpace(player.CharacterName)
                ? player.CharacterName : "Ty";

            GameModeConfig mode = _modeOptions[Mathf.Clamp(_gameModeDropdown != null ? _gameModeDropdown.value : 0, 0, _modeOptions.Count - 1)];

            GameLaunchContextStore.Set(new GameLaunchContext(
                playerName,
                _selectedPlayerId,
                _selectedEnemy1Id,
                _selectedEnemy2Id,
                mode));

            SceneManager.LoadScene(_gameSceneName);
        }

        private void PopulateCharacterOptions()
        {
            _characterOptions.Clear();

            var service = _characterManager != null ? _characterManager.GetService() : null;
            if (service == null) return;

            foreach (var character in service.GetAll())
            {
                if (character == null) continue;
                _characterOptions.Add(character);
            }

            if (_characterOptions.Count > 0) _selectedPlayerId  = _characterOptions[0].CharacterId;
            if (_characterOptions.Count > 1) _selectedEnemy1Id  = _characterOptions[1].CharacterId;
            if (_characterOptions.Count > 2) _selectedEnemy2Id  = _characterOptions[2].CharacterId;
            else if (_characterOptions.Count > 1) _selectedEnemy2Id = _characterOptions[1].CharacterId;
        }

        private void PopulateModeOptions()
        {
            _modeOptions.Clear();
            _gameModeDropdown?.ClearOptions();

            var options = new List<TMP_Dropdown.OptionData>();
            foreach (var mode in _availableModes)
            {
                if (mode == null) continue;
                _modeOptions.Add(mode);
                options.Add(new TMP_Dropdown.OptionData(string.IsNullOrWhiteSpace(mode.ModeName) ? mode.name : mode.ModeName));
            }

            _gameModeDropdown?.AddOptions(options);
        }

        private void WireSelectionButtons()
        {
            _playerSelectButton?.onClick.AddListener(() => OpenSelectionModal(Slot.Player));
            _enemy1SelectButton?.onClick.AddListener(() => OpenSelectionModal(Slot.Enemy1));
            _enemy2SelectButton?.onClick.AddListener(() => OpenSelectionModal(Slot.Enemy2));
        }

        private void OpenSelectionModal(Slot slot)
        {
            if (_characterOptions.Count == 0) PopulateCharacterOptions();

            if (_selectionModal == null)
            {
                Debug.LogError("[MainMenuController] Selection modal is not assigned.");
                return;
            }

            var available = AvailableFor(slot);
            int currentId = slot == Slot.Player ? _selectedPlayerId
                          : slot == Slot.Enemy1  ? _selectedEnemy1Id
                          : _selectedEnemy2Id;
            string title = slot == Slot.Player ? "Choose player" : "Choose enemy";

            _selectionModal.Show(available, c => OnCharacterSelected(slot, c), currentId, title);
        }

        private List<CharacterData> AvailableFor(Slot slot)
        {
            int excludeA = slot != Slot.Player ? _selectedPlayerId  : -1;
            int excludeB = slot != Slot.Enemy1 ? _selectedEnemy1Id  : -1;
            int excludeC = slot != Slot.Enemy2 ? _selectedEnemy2Id  : -1;

            return _characterOptions
                .Where(c => c.CharacterId != excludeA
                         && c.CharacterId != excludeB
                         && c.CharacterId != excludeC)
                .ToList();
        }

        private void OnCharacterSelected(Slot slot, CharacterData character)
        {
            if (character == null) return;
            switch (slot)
            {
                case Slot.Player:  _selectedPlayerId  = character.CharacterId; break;
                case Slot.Enemy1:  _selectedEnemy1Id  = character.CharacterId; break;
                case Slot.Enemy2:  _selectedEnemy2Id  = character.CharacterId; break;
            }
            RefreshSelectedSlotsVisuals();
        }

        private void RefreshSelectedSlotsVisuals()
        {
            var player  = _characterOptions.Find(c => c.CharacterId == _selectedPlayerId);
            var enemy1  = _characterOptions.Find(c => c.CharacterId == _selectedEnemy1Id);
            var enemy2  = _characterOptions.Find(c => c.CharacterId == _selectedEnemy2Id);

            SetSlotVisual(_playerPreviewImage, _playerNameText,
                player?.CharacterSprite, player?.CharacterName ?? "Ty");
            SetSlotVisual(_enemy1PreviewImage, _enemy1NameText,
                enemy1?.CharacterSprite, enemy1?.CharacterName ?? "Enemy 1");
            SetSlotVisual(_enemy2PreviewImage, _enemy2NameText,
                enemy2?.CharacterSprite, enemy2?.CharacterName ?? "Enemy 2");
        }

        private static void SetSlotVisual(Image image, TMP_Text text, Sprite sprite, string label)
        {
            if (image != null)
            {
                image.sprite = sprite;
                image.enabled = true;
            }
            if (text != null)
                text.text = label;
        }
    }
}
