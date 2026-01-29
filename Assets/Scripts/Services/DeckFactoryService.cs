using System.Collections.Generic;
using System.Linq;
using MariasGame.Core;
using MariasGame.Core.Interfaces;
using MariasGame.ScriptableObjects;

namespace MariasGame.Services
{
    /// <summary>
    /// Služba pro vytváření balíčků karet.
    /// Vytváří IDeck instance z CardDatabaseSO.
    /// </summary>
    public class DeckFactoryService : IDeckFactory
    {
        private readonly CardDatabaseSO _database;
        
        public DeckFactoryService(CardDatabaseSO database)
        {
            _database = database ?? throw new System.ArgumentNullException(nameof(database));
        }
        
        /// <summary>
        /// Vytvoří standardní balíček na základě CardDatabaseSO.
        /// </summary>
        public IDeck CreateStandardDeck()
        {
            var cardsInGame = _database.GetCardsInGame();
            var cards = new List<Card>();
            
            foreach (var cardDataSO in cardsInGame)
            {
                var card = new Card(cardDataSO.suit, cardDataSO.rank);
                cards.Add(card);
            }
            
            var deck = new Deck(cards);
            return deck;
        }
    }
}

