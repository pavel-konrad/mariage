using System.Collections.Generic;

namespace MariasGame.Core.Events
{
    public struct CardEvent
    {
        public CardEventType Type;
        public int PlayerIndex;
        public Card Card;
        public IReadOnlyList<Card> Cards;
    }

    public enum CardEventType
    {
        CardsDealt,
        CardPlayed,
        TalonTaken,
        TalonDiscarded,
        LegalPlaysUpdated
    }
}
