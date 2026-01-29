using System;
using MariasGame.Core.Interfaces;

namespace MariasGame.Core
{
    /// <summary>
    /// Rozšíření Card o state machine pro správu stavů.
    /// Tato třída kombinuje logiku karty s jejími stavy.
    /// </summary>
    public class CardWithState : Card
    {
        private readonly ICardStateMachine _stateMachine;
        
        /// <summary>
        /// Aktuální stav karty.
        /// </summary>
        public CardState State => _stateMachine.CurrentState;
        
        /// <summary>
        /// Událost při změně stavu karty.
        /// </summary>
        public event Action<CardState, CardState> OnStateChanged
        {
            add => _stateMachine.OnStateChanged += value;
            remove => _stateMachine.OnStateChanged -= value;
        }
        
        public CardWithState(CardSuit suit, CardRank rank, CardState initialState = CardState.InDeck)
            : base(suit, rank)
        {
            _stateMachine = new CardStateMachine(initialState);
        }
        
        /// <summary>
        /// Změní stav karty na nový stav.
        /// </summary>
        public bool ChangeState(CardState newState)
        {
            return _stateMachine.ChangeState(newState);
        }
        
        /// <summary>
        /// Zkontroluje, zda může karta přejít do nového stavu.
        /// </summary>
        public bool CanTransitionTo(CardState newState)
        {
            return _stateMachine.CanTransitionTo(newState);
        }
    }
}

