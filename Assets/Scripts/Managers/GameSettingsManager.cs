using UnityEngine;
using MariasGame.Core;
using MariasGame.Core.Interfaces;
using MariasGame.ScriptableObjects;
using MariasGame.Services;

namespace MariasGame.Managers
{
    public class GameSettingsManager : MonoBehaviour, ISettingsProvider
    {
        [SerializeField] private GameSettingsConfig settingsConfig;

        [SerializeField] private bool autoSave = true;
        [SerializeField] private string saveKey = "GameSettings";

        private SettingsService _settingsService;
        private ISettingsRepository _repository;

        void Awake()
        {
            InitializeServices();
            LoadSettings();
        }

        void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && autoSave) SaveSettings();
        }

        void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus && autoSave) SaveSettings();
        }

        void OnDestroy()
        {
            if (autoSave) SaveSettings();
        }

        private void InitializeServices()
        {
            if (settingsConfig == null)
            {
                Debug.LogError("[GameSettingsManager] GameSettingsConfig is not assigned!");
                return;
            }
            _repository = new SettingsRepository(saveKey);
            _settingsService = new SettingsService(settingsConfig, _repository);
        }

        public void LoadSettings() => _settingsService?.RefreshSettings();

        public void SaveSettings() => _settingsService?.SaveSettings();

        public void ResetToDefault()
        {
            if (settingsConfig != null && _repository != null)
            {
                _repository.Save(settingsConfig.ToGameSettings());
                _settingsService?.RefreshSettings();
#if UNITY_EDITOR
                Debug.Log("[GameSettingsManager] Settings reset to default values from GameSettingsConfig.");
#endif
            }
        }

        public GameSettings GetSettings()
        {
            if (_settingsService != null) return _settingsService.GetSettings();
            if (settingsConfig != null) return settingsConfig.ToGameSettings();

            Debug.LogWarning("[GameSettingsManager] No settings available, returning default.");
            return GameSettings.CreateDefault();
        }

        public void SetSettingsConfig(GameSettingsConfig newConfig)
        {
            if (newConfig != null)
            {
                settingsConfig = newConfig;
                InitializeServices();
                if (autoSave) SaveSettings();
            }
        }
    }
}
