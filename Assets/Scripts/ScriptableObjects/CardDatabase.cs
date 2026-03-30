using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using MariasGame.Core;

namespace MariasGame.ScriptableObjects
{
    [CreateAssetMenu(fileName = "CardDatabase", menuName = "MariasGame/Card Database", order = 2)]
    public class CardDatabase : ScriptableObject
    {
        [field: FormerlySerializedAs("gameName"), SerializeField] public string GameName { get; set; } = "Marias";
        [field: FormerlySerializedAs("deckName"), SerializeField] public string DeckName { get; set; } = "Standard32";
        [field: FormerlySerializedAs("cards"), SerializeField] public List<CardData> Cards { get; set; } = new List<CardData>();
        [field: FormerlySerializedAs("gameSounds"), SerializeField] public SoundData GameSounds { get; set; }

        public CardData GetCardData(CardSuit suit, CardRank rank)
            => Cards.FirstOrDefault(c => c != null && c.Suit == suit && c.Rank == rank && c.IsInGame);

        public List<CardData> GetCardsInGame()
            => Cards.Where(c => c != null && c.IsInGame).ToList();

        private void OnValidate()
        {
            int totalCards = Cards == null ? 0 : Cards.Count;
            int nullCardsCount = Cards == null ? 0 : Cards.Count(c => c == null);
            int cardsInGameCount = Cards == null ? 0 : Cards.Count(c => c != null && c.IsInGame);
            int missingSpritesCount = Cards == null ? 0 : Cards.Count(c => c != null && c.IsInGame && c.CardSprite == null);

            if (Cards == null || Cards.Count == 0)
            {
                Debug.LogWarning($"[CardDatabase] {name}: No cards in database!");
                return;
            }

            var duplicates = Cards
                .Where(c => c != null)
                .GroupBy(c => new { c.Suit, c.Rank })
                .Where(g => g.Count() > 1)
                .Select(g => g.Key);

            if (duplicates.Any())
                Debug.LogWarning($"[CardDatabase] {name}: Duplicate cards found: {string.Join(", ", duplicates)}");

            int cardsInGame = GetCardsInGame().Count;
            if (cardsInGame != 32)
                Debug.LogWarning($"[CardDatabase] {name}: Expected 32 cards in game, found {cardsInGame}");

            foreach (var card in Cards.Where(c => c != null && c.IsInGame))
            {
                if (card.CardSprite == null)
                    Debug.LogWarning($"[CardDatabase] {name}: Card {card.Suit} {card.Rank} is missing sprite!");
            }
        }
    }
}
