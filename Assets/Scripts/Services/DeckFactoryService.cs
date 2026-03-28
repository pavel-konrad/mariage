using System.Collections.Generic;
using System.Linq;
using MariasGame.Core;
using MariasGame.Core.Interfaces;
using MariasGame.ScriptableObjects;

namespace MariasGame.Services
{
    /// <summary>
    /// Služba pro vytváření balíčků karet.
    /// Vytváří IDeck instance z CardDatabase.
    /// </summary>
    public class DeckFactoryService : IDeckFactory
    {
        private readonly CardDatabase _database;

        public DeckFactoryService(CardDatabase database)
        {
            _database = database ?? throw new System.ArgumentNullException(nameof(database));
        }

        public IDeck CreateStandardDeck()
        {
            var cardsInGame = _database.GetCardsInGame();
            var cards = new List<Card>();

            foreach (var cardData in cardsInGame)
            {
                var card = new Card(cardData.Suit, cardData.Rank);
                cards.Add(card);
            }
            
            var deck = new Deck(cards);
            return deck;
        }
    }
}

