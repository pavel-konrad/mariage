using System.Collections.Generic;
using System.Linq;
using MariasGame.Core.Interfaces;

namespace MariasGame.Core
{
    /// <summary>
    /// Reprezentace balíčku karet.
    /// Čistá datová struktura bez Unity závislostí.
    /// Implementuje IDeck a IShuffleable.
    /// </summary>
    public class Deck : IDeck, IShuffleable
    {
        private List<Card> cards;
        
        public Deck()
        {
            cards = new List<Card>();
        }
        
        public Deck(List<Card> initialCards)
        {
            cards = new List<Card>(initialCards);
        }
        
        /// <summary>
        /// Počet karet v balíčku.
        /// </summary>
        public int Count => cards.Count;
        
        /// <summary>
        /// Zda je balíček prázdný.
        /// </summary>
        public bool IsEmpty => cards.Count == 0;
        
        /// <summary>
        /// Vrací kopii karet v balíčku (immutable).
        /// </summary>
        public IReadOnlyList<Card> Cards => cards.AsReadOnly();

        /// <summary>
        /// Ověří, zda balíček obsahuje zadanou kartu.
        /// </summary>
        public bool Contains(Card card)
        {
            return card != null && cards.Contains(card);
        }
        
        /// <summary>
        /// Přidá kartu do balíčku.
        /// </summary>
        public bool AddCard(Card card)
        {
            if (card == null) return false;

            cards.Add(card);
            return true;
        }

        /// <summary>
        /// Odebere kartu z balíčku.
        /// </summary>
        public bool RemoveCard(Card card)
        {
            if (card == null) return false;
            return cards.Remove(card);
        }
        
        /// <summary>
        /// Přidá více karet do balíčku.
        /// </summary>
        public void AddCards(IEnumerable<Card> cardsToAdd)
        {
            if (cardsToAdd != null)
            {
                cards.AddRange(cardsToAdd.Where(card => card != null));
            }
        }
        
        /// <summary>
        /// Vezme kartu ze shora balíčku.
        /// </summary>
        public Card DrawCard()
        {
            if (IsEmpty) return null;
            
            var topCard = cards[cards.Count - 1];
            cards.RemoveAt(cards.Count - 1);
            return topCard;
        }
        
        /// <summary>
        /// Vezme více karet ze shora balíčku.
        /// </summary>
        public List<Card> DrawCards(int count)
        {
            var drawnCards = new List<Card>();
            
            for (int i = 0; i < count && !IsEmpty; i++)
            {
                drawnCards.Add(DrawCard());
            }
            
            return drawnCards;
        }
        
        /// <summary>
        /// Podívá se na vrchní kartu bez jejího odebrání.
        /// </summary>
        public Card PeekTopCard()
        {
            return IsEmpty ? null : cards[cards.Count - 1];
        }
        
        /// <summary>
        /// Zamíchá balíček pomocí Fisher-Yates algoritmu.
        /// </summary>
        public void Shuffle()
        {
            if (cards == null || cards.Count <= 1) return;
            
            var random = new System.Random();
            for (int i = cards.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                var temp = cards[i];
                cards[i] = cards[j];
                cards[j] = temp;
            }
        }
        
        /// <summary>
        /// Vyčistí balíček.
        /// </summary>
        public void Clear()
        {
            cards.Clear();
        }
        
        public override string ToString()
        {
            return $"Deck ({Count} cards)";
        }
    }
}
