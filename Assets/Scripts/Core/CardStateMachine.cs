using System;
using System.Collections.Generic;
using MariasGame.Core.Interfaces;

namespace MariasGame.Core
{
    /// <summary>
    /// State machine pro správu stavů karty.
    /// Validuje přechody mezi stavy a umožňuje flexibilní přidávání nových stavů.
    /// </summary>
    public class CardStateMachine : ICardStateMachine
    {
        private CardState _currentState;
        private readonly Dictionary<CardState, HashSet<CardState>> _validTransitions;
        
        public CardState CurrentState => _currentState;
        public event Action<CardState, CardState> OnStateChanged;
        
        public CardStateMachine(CardState initialState = CardState.InDeck)
        {
            _currentState = initialState;
            _validTransitions = InitializeValidTransitions();
        }
        
        /// <summary>
        /// Inicializuje platné přechody mezi stavy.
        /// </summary>
        private Dictionary<CardState, HashSet<CardState>> InitializeValidTransitions()
        {
            var transitions = new Dictionary<CardState, HashSet<CardState>>
            {
                // Z balíčku můžeme jít do dealing
                [CardState.InDeck] = new HashSet<CardState> { CardState.Dealing },
                
                // Z dealing můžeme jít do ruky
                [CardState.Dealing] = new HashSet<CardState> { CardState.InHand },
                
                // Z ruky můžeme jít do selected, playing, nebo discarding
                [CardState.InHand] = new HashSet<CardState> 
                { 
                    CardState.Selected, 
                    CardState.Playing, 
                    CardState.Discarding,
                    CardState.InHand // Zpět do ruky (např. při zrušení výběru)
                },
                
                // Z selected můžeme jít zpět do ruky, nebo do playing
                [CardState.Selected] = new HashSet<CardState> 
                { 
                    CardState.InHand, 
                    CardState.Playing 
                },
                
                // Z playing můžeme jít na stůl
                [CardState.Playing] = new HashSet<CardState> { CardState.OnTable },
                
                // Z discarding můžeme jít do discard pile
                [CardState.Discarding] = new HashSet<CardState> { CardState.InDiscardPile },
                
                // Z discard pile můžeme jít zpět do balíčku (např. při reshuffle)
                [CardState.InDiscardPile] = new HashSet<CardState> { CardState.InDeck },
                
                // Ze stolu můžeme jít do discard pile (konec kola)
                [CardState.OnTable] = new HashSet<CardState> { CardState.InDiscardPile }
            };
            
            return transitions;
        }
        
        /// <summary>
        /// Změní stav karty na nový stav.
        /// </summary>
        public bool ChangeState(CardState newState)
        {
            if (!CanTransitionTo(newState))
            {
                return false;
            }
            
            var oldState = _currentState;
            _currentState = newState;
            OnStateChanged?.Invoke(oldState, newState);
            
            return true;
        }
        
        /// <summary>
        /// Zkontroluje, zda je přechod z aktuálního stavu do nového stavu platný.
        /// </summary>
        public bool CanTransitionTo(CardState newState)
        {
            // Stejný stav je vždy platný (žádná změna)
            if (_currentState == newState)
            {
                return true;
            }
            
            // Zkontroluj, zda existuje platný přechod
            if (_validTransitions.TryGetValue(_currentState, out var validStates))
            {
                return validStates.Contains(newState);
            }
            
            return false;
        }
    }
}

