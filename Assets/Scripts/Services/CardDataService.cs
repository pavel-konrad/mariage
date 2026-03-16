using UnityEngine;
using MariasGame.Core;
using MariasGame.Core.Interfaces;
using MariasGame.ScriptableObjects;
using CardData = MariasGame.Core.Interfaces.CardData;

namespace MariasGame.Services
{
    /// <summary>
    /// Služba pro poskytování dat karet.
    /// Načítá data z CardDatabaseSO a poskytuje CardData.
    /// </summary>
    public class CardDataService : ICardDataProvider
    {
        private readonly CardDatabaseSO _database;

        public CardDataService(CardDatabaseSO database)
        {
            _database = database ?? throw new System.ArgumentNullException(nameof(database));
        }

        /// <summary>
        /// Získá CardData pro konkrétní kartu.
        /// </summary>
        public CardData GetCardData(CardSuit suit, CardRank rank)
        {
            var cardDataSO = _database.GetCardData(suit, rank);

            if (cardDataSO == null)
            {
                Debug.LogWarning($"[CardDataService] Card data not found for {rank} of {suit}");
                return new CardData();
            }

            return new CardData
            {
                Sprite = cardDataSO.cardSprite
            };
        }
    }
}

