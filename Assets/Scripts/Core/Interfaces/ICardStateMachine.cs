using System;

namespace MariasGame.Core.Interfaces
{
    /// <summary>
    /// Interface pro state machine karty.
    /// Řídí přechody mezi stavy karty a validuje je.
    /// </summary>
    public interface ICardStateMachine
    {
        /// <summary>
        /// Aktuální stav karty.
        /// </summary>
        CardState CurrentState { get; }
        
        /// <summary>
        /// Událost při změně stavu karty.
        /// </summary>
        event Action<CardState, CardState> OnStateChanged;
        
        /// <summary>
        /// Změní stav karty na nový stav.
        /// </summary>
        /// <param name="newState">Nový stav</param>
        /// <returns>True, pokud byl přechod úspěšný</returns>
        bool ChangeState(CardState newState);
        
        /// <summary>
        /// Zkontroluje, zda je přechod z aktuálního stavu do nového stavu platný.
        /// </summary>
        bool CanTransitionTo(CardState newState);
    }
}

