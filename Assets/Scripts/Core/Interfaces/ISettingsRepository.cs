using MariasGame.Core;

namespace MariasGame.Core.Interfaces
{
    /// <summary>
    /// Interface pro ukládání a načítání nastavení.
    /// Abstrakce pro persistenci (PlayerPrefs, JSON, atd.).
    /// </summary>
    public interface ISettingsRepository
    {
        void Save(GameSettings settings);
        GameSettings Load();
        bool HasSavedSettings();
    }
}

