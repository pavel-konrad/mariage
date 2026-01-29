using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MariasGame.Core;

namespace MariasGame.ScriptableObjects
{
    /// <summary>
    /// ScriptableObject pro databázi karet.
    /// Obsahuje seznam všech karet a herních zvuků.
    /// </summary>
    [CreateAssetMenu(fileName = "CardDatabase", menuName = "MariasGame/Card Database", order = 2)]
    public class CardDatabaseSO : ScriptableObject
    {
        [Header("Database Info")]
        public string gameName = "Marias";
        public string deckName = "Standard32";
        
        [Header("Cards")]
        public List<CardDataSO> cards = new List<CardDataSO>();
        
        [Header("Game Sounds")]
        public SoundDataSO gameSounds;
        
        /// <summary>
        /// Získá CardDataSO pro konkrétní kartu.
        /// </summary>
        public CardDataSO GetCardData(CardSuit suit, CardRank rank)
        {
            return cards.FirstOrDefault(c => c != null && c.suit == suit && c.rank == rank && c.isInGame);
        }
        
        /// <summary>
        /// Získá všechny karty, které jsou v hře.
        /// </summary>
        public List<CardDataSO> GetCardsInGame()
        {
            return cards.Where(c => c != null && c.isInGame).ToList();
        }
        
        /// <summary>
        /// Validace v Editoru - kontrola kompletního balíčku (32 karet pro Mariáš).
        /// </summary>
        private void OnValidate()
        {
            if (cards == null || cards.Count == 0)
            {
                Debug.LogWarning($"[CardDatabaseSO] {name}: No cards in database!");
                return;
            }
            
            // Kontrola duplicit
            var duplicates = cards
                .Where(c => c != null)
                .GroupBy(c => new { c.suit, c.rank })
                .Where(g => g.Count() > 1)
                .Select(g => g.Key);
                
            if (duplicates.Any())
            {
                Debug.LogWarning($"[CardDatabaseSO] {name}: Duplicate cards found: {string.Join(", ", duplicates)}");
            }
            
            // Kontrola počtu karet v hře
            int cardsInGame = GetCardsInGame().Count;
            if (cardsInGame != 32)
            {
                Debug.LogWarning($"[CardDatabaseSO] {name}: Expected 32 cards in game, found {cardsInGame}");
            }
            
            // Kontrola chybějících assetů
            foreach (var card in cards.Where(c => c != null && c.isInGame))
            {
                if (card.cardSprite == null)
                {
                    Debug.LogWarning($"[CardDatabaseSO] {name}: Card {card.suit} {card.rank} is missing sprite!");
                }
            }
        }
    }
}

