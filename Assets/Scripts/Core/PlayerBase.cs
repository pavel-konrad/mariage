using System;
using System.Collections.Generic;
using MariasGame.Core.Interfaces;

namespace MariasGame.Core
{
    /// <summary>
    /// Základní implementace hráče.
    /// Čistá třída bez Unity závislostí.
    /// </summary>
    public abstract class PlayerBase : IPlayer
    {
        private readonly int _id;
        private readonly string _name;
        private int _avatarIndex;
        private int _cash;
        private int _currentBet;
        private bool _isActive;
        protected List<Card> hand;
        
        public PlayerBase(int id, string name, int startingCash = 1000, int avatarIndex = 0)
        {
            _id = id;
            _name = name ?? throw new ArgumentNullException(nameof(name));
            _avatarIndex = avatarIndex;
            _cash = startingCash;
            _currentBet = 0;
            _isActive = true;
            hand = new List<Card>();
        }
        
        // IPlayer implementation
        public int Id => _id;
        public string Name => _name;
        public int AvatarIndex
        {
            get => _avatarIndex;
            set => _avatarIndex = value;
        }
        public int Cash => _cash;
        public bool IsActive
        {
            get => _isActive;
            set => _isActive = value;
        }
        public abstract bool IsHuman { get; }
        
        // Hand implementation
        public IReadOnlyList<Card> Hand => hand.AsReadOnly();
        public int HandCount => hand.Count;
        public bool HasCards => hand.Count > 0;
        
        public void AddCard(Card card)
        {
            if (card != null)
            {
                hand.Add(card);
            }
        }
        
        public bool RemoveCard(Card card)
        {
            return hand.Remove(card);
        }
        
        public void ClearHand()
        {
            hand.Clear();
        }
        
        // Bet management
        public int CurrentBet => _currentBet;
        
        public void SetBet(int betAmount)
        {
            _currentBet = Math.Max(0, Math.Min(betAmount, _cash));
        }
        
        public int PlaceBet()
        {
            int betAmount = Math.Min(_currentBet, _cash);
            _cash -= betAmount;
            return betAmount;
        }
        
        public void ResetBet()
        {
            _currentBet = 0;
        }
        
        public bool CanAffordBet(int betAmount)
        {
            return betAmount <= _cash;
        }
        
        public void AddCash(int amount)
        {
            _cash += Math.Max(0, amount);
        }
        
        public override string ToString()
        {
            return $"{Name} (ID: {Id}, Avatar: {AvatarIndex}, Cash: {Cash}, CurrentBet: {CurrentBet}, Hand: {HandCount} cards)";
        }
    }
}

