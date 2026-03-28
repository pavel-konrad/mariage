using UnityEngine;
using MariasGame.Core;
using MariasGame.ScriptableObjects;

namespace MariasGame.Services
{
    public class CardDataService
    {
        private readonly CardDatabase _database;

        public CardDataService(CardDatabase database)
        {
            _database = database ?? throw new System.ArgumentNullException(nameof(database));
        }

        public Sprite GetCardSprite(CardSuit suit, CardRank rank)
        {
            var cardData = _database.GetCardData(suit, rank);

            if (cardData == null)
            {
                Debug.LogWarning($"[CardDataService] Card data not found for {rank} of {suit}");
                return null;
            }

            return cardData.CardSprite;
        }
    }
}
