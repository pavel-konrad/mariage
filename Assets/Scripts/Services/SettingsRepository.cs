using UnityEngine;
using MariasGame.Core;
using MariasGame.Core.Interfaces;

namespace MariasGame.Services
{
    /// <summary>
    /// Repository pro ukládání a načítání nastavení z PlayerPrefs.
    /// Nezávislý na MonoBehaviour.
    /// </summary>
    public class SettingsRepository : ISettingsRepository
    {
        private readonly string _saveKey;
        
        public SettingsRepository(string saveKey = "GameSettings")
        {
            _saveKey = saveKey;
        }
        
        /// <summary>
        /// Uloží nastavení do PlayerPrefs.
        /// </summary>
        public void Save(GameSettings settings)
        {
            if (settings == null)
            {
                Debug.LogError("[SettingsRepository] Cannot save null settings!");
                return;
            }
            
            try
            {
                string json = JsonUtility.ToJson(settings, true);
                PlayerPrefs.SetString(_saveKey, json);
                PlayerPrefs.Save();
                Debug.Log("[SettingsRepository] Settings saved to PlayerPrefs.");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SettingsRepository] Error saving settings: {e.Message}");
            }
        }
        
        /// <summary>
        /// Načte nastavení z PlayerPrefs.
        /// </summary>
        public GameSettings Load()
        {
            if (!HasSavedSettings())
            {
                Debug.Log("[SettingsRepository] No saved settings found, returning default.");
                return GameSettings.CreateDefault();
            }
            
            try
            {
                string json = PlayerPrefs.GetString(_saveKey);
                var settings = JsonUtility.FromJson<GameSettings>(json);
                
                if (settings == null)
                {
                    Debug.LogWarning("[SettingsRepository] Failed to deserialize settings, returning default.");
                    return GameSettings.CreateDefault();
                }
                
                Debug.Log("[SettingsRepository] Settings loaded from PlayerPrefs.");
                return settings;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SettingsRepository] Error loading settings: {e.Message}");
                return GameSettings.CreateDefault();
            }
        }
        
        /// <summary>
        /// Zkontroluje, zda existují uložená nastavení.
        /// </summary>
        public bool HasSavedSettings()
        {
            return PlayerPrefs.HasKey(_saveKey);
        }
    }
}

