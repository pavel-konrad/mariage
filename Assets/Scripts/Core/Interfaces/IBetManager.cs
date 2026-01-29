namespace MariasGame.Core.Interfaces
{
    /// <summary>
    /// Interface pro správu sázek hráče.
    /// Definuje operace pro práci se sázkami.
    /// </summary>
    public interface IBetManager
    {
        int CurrentBet { get; }
        
        void SetBet(int betAmount);
        int PlaceBet();
        void ResetBet();
        bool CanAffordBet(int betAmount);
        void AddCash(int amount);
    }
}

