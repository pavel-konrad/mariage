using MariasGame.Core;
using MariasGame.Core.Interfaces;
using MariasGame.ScriptableObjects;

namespace MariasGame.Services
{
    /// <summary>
    /// Služba pro poskytování nastavení hry.
    /// Používá GameSettingsConfig a může používat ISettingsRepository pro persistenci.
    /// </summary>
    public class SettingsService : ISettingsProvider
    {
        private readonly GameSettingsConfig _settingsConfig;
        private readonly ISettingsRepository _repository;
        private GameSettings _cachedSettings;

        public SettingsService(GameSettingsConfig settingsConfig, ISettingsRepository repository = null)
        {
            _settingsConfig = settingsConfig ?? throw new System.ArgumentNullException(nameof(settingsConfig));
            _repository = repository;
        }
        
        /// <summary>
        /// Získá aktuální nastavení hry.
        /// Pokud existuje repository, načte z něj, jinak použije GameSettingsConfig.
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
                _cachedSettings = _settingsConfig.ToGameSettings();
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

