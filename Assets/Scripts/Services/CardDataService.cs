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
        private readonly IAssetLoader _assetLoader;
        
        public CardDataService(CardDatabaseSO database, IAssetLoader assetLoader)
        {
            _database = database ?? throw new System.ArgumentNullException(nameof(database));
            _assetLoader = assetLoader ?? throw new System.ArgumentNullException(nameof(assetLoader));
        }
        
        /// <summary>
        /// Získá CardData pro konkrétní kartu.
        /// </summary>
        public CardData GetCardData(CardSuit suit, CardRank rank)
        {
            var cardDataSO = _database.GetCardData(suit, rank);
            
            if (cardDataSO == null)
            {
                UnityEngine.Debug.LogWarning($"[CardDataService] Card data not found for {rank} of {suit}");
                return new CardData();
            }
            
            return new CardData
            {
                Sprite = cardDataSO.cardSprite,
                Sound = cardDataSO.GetRandomSound(), // Náhodný výběr z variations
                Animation = cardDataSO.cardAnimation
            };
        }
    }
}

