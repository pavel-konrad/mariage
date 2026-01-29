using UnityEngine;
using MariasGame.Core;
using MariasGame.Core.Interfaces;
using MariasGame.ScriptableObjects;
using MariasGame.Services;

namespace MariasGame.Managers
{
    /// <summary>
    /// Manager pro správu herních nastavení.
    /// Implementuje ISettingsProvider a používá GameSettingsSO.
    /// </summary>
    public class GameSettingsManager : MonoBehaviour, ISettingsProvider
    {
        [Header("Game Settings")]
        [SerializeField] private GameSettingsSO settingsSO;
        
        [Header("Save/Load")]
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
            if (pauseStatus && autoSave)
            {
                SaveSettings();
            }
        }
        
        void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus && autoSave)
            {
                SaveSettings();
            }
        }
        
        void OnDestroy()
        {
            if (autoSave)
            {
                SaveSettings();
            }
        }
        
        /// <summary>
        /// Inicializuje služby pro správu nastavení.
        /// </summary>
        private void InitializeServices()
        {
            if (settingsSO == null)
            {
                Debug.LogError("[GameSettingsManager] GameSettingsSO is not assigned!");
                return;
            }
            
            _repository = new SettingsRepository(saveKey);
            _settingsService = new SettingsService(settingsSO, _repository);
        }
        
        /// <summary>
        /// Načte nastavení z repository.
        /// </summary>
        public void LoadSettings()
        {
            if (_settingsService != null)
            {
                _settingsService.RefreshSettings();
            }
        }
        
        /// <summary>
        /// Uloží nastavení přes repository.
        /// </summary>
        public void SaveSettings()
        {
            if (_settingsService != null)
            {
                _settingsService.SaveSettings();
            }
        }
        
        /// <summary>
        /// Resetuje nastavení na defaultní hodnoty z GameSettingsSO.
        /// </summary>
        public void ResetToDefault()
        {
            if (settingsSO != null && _repository != null)
            {
                var defaultSettings = settingsSO.ToGameSettings();
                _repository.Save(defaultSettings);
                
                if (_settingsService != null)
                {
                    _settingsService.RefreshSettings();
                }
                
                Debug.Log("[GameSettingsManager] Settings reset to default values from GameSettingsSO.");
            }
        }
        
        /// <summary>
        /// Získá aktuální nastavení hry.
        /// </summary>
        public GameSettings GetSettings()
        {
            if (_settingsService != null)
            {
                return _settingsService.GetSettings();
            }
            
            // Fallback na GameSettingsSO
            if (settingsSO != null)
            {
                return settingsSO.ToGameSettings();
            }
            
            Debug.LogWarning("[GameSettingsManager] No settings available, returning default.");
            return GameSettings.CreateDefault();
        }
        
        /// <summary>
        /// Nastaví nové GameSettingsSO.
        /// </summary>
        public void SetSettingsSO(GameSettingsSO newSettingsSO)
        {
            if (newSettingsSO != null)
            {
                settingsSO = newSettingsSO;
                InitializeServices();
                
                if (autoSave)
                {
                    SaveSettings();
                }
            }
        }
    }
}
