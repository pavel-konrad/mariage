namespace MariasGame.Core.Interfaces
{
    /// <summary>
    /// Základní interface pro hráče.
    /// Definuje základní vlastnosti hráče.
    /// </summary>
    public interface IPlayer
    {
        int Id { get; }
        string Name { get; }
        int AvatarIndex { get; set; }
        int Cash { get; }
        bool IsActive { get; set; }
        bool IsHuman { get; }
    }
}

