using MariasGame.Core;
using MariasGame.Core.Interfaces;
using MariasGame.ScriptableObjects;

namespace MariasGame.Services
{
    /// <summary>
    /// Služba pro poskytování nastavení hry.
    /// Používá GameSettingsSO a může používat ISettingsRepository pro persistenci.
    /// </summary>
    public class SettingsService : ISettingsProvider
    {
        private readonly GameSettingsSO _settingsSO;
        private readonly ISettingsRepository _repository;
        private GameSettings _cachedSettings;
        
        public SettingsService(GameSettingsSO settingsSO, ISettingsRepository repository = null)
        {
            _settingsSO = settingsSO ?? throw new System.ArgumentNullException(nameof(settingsSO));
            _repository = repository;
        }
        
        /// <summary>
        /// Získá aktuální nastavení hry.
        /// Pokud existuje repository, načte z něj, jinak použije GameSettingsSO.
        /// </summary>
        public GameSettings GetSettings()
        {
            if (_cachedSettings != null)
                return _cachedSettings;
                
            if (_repository != null && _repository.HasSavedSettings())
            {
                _cachedSettings = _repository.Load();
            }
            else
            {
                _cachedSettings = _settingsSO.ToGameSettings();
            }
            
            return _cachedSettings;
        }
        
        /// <summary>
        /// Uloží nastavení přes repository (pokud je dostupný).
        /// </summary>
        public void SaveSettings()
        {
            if (_repository == null)
                return;
                
            var settings = GetSettings();
            _repository.Save(settings);
        }
        
        /// <summary>
        /// Resetuje cache nastavení.
        /// </summary>
        public void RefreshSettings()
        {
            _cachedSettings = null;
        }
    }
}

