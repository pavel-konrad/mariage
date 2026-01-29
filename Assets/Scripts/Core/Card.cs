using System;

namespace MariasGame.Core
{
    /// <summary>
    /// Reprezentace karty v hře Mariáš.
    /// Čistá datová třída bez Unity závislostí.
    /// </summary>
    [Serializable]
    public class Card : IEquatable<Card>
    {
        public CardSuit Suit { get; }
        public CardRank Rank { get; }
        public int Value { get; }
        
        public Card(CardSuit suit, CardRank rank)
        {
            Suit = suit;
            Rank = rank;
            Value = CalculateValue(rank);
        }
        
        private int CalculateValue(CardRank rank)
        {
            return rank switch
            {
                CardRank.Seven => 7,
                CardRank.Eight => 8,
                CardRank.Nine => 9,
                CardRank.Ten => 10,
                CardRank.Jack => 11,
                CardRank.Queen => 12,
                CardRank.King => 13,
                CardRank.Ace => 14,
                _ => 0
            };
        }
        
        public bool Equals(Card other)
        {
            if (other == null) return false;
            return Suit == other.Suit && Rank == other.Rank;
        }
        
        public override bool Equals(object obj)
        {
            return Equals(obj as Card);
        }
        
        public override int GetHashCode()
        {
            return HashCode.Combine(Suit, Rank);
        }
        
        public override string ToString()
        {
            return $"{Rank} of {Suit}";
        }
        
    }

    public enum CardSuit
    {
        Hearts,   // Srdce ♥
        Diamonds, // Káry ♦
        Clubs,    // Trefy ♣
        Spades    // Piky ♠
    }

    public enum CardRank
    {
        Seven,  // 7
        Eight,  // 8
        Nine,   // 9
        Ten,    // 10
        Jack,   // J
        Queen,  // Q
        King,   // K
        Ace     // A
    }
}