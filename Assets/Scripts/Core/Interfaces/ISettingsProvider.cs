using MariasGame.Core;

namespace MariasGame.Core.Interfaces
{
    /// <summary>
    /// Interface pro poskytování nastavení hry.
    /// Abstrakce pro přístup k nastavení (Manager, Service, atd.).
    /// </summary>
    public interface ISettingsProvider
    {
        GameSettings GetSettings();
    }
}

