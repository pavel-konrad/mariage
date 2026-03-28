namespace MariasGame.Core.Events
{
    public struct GameEvent
    {
        public GameEventType Type;
        public int PlayerIndex;
        public int DeclarerIndex;
        public GamePhase Phase;
        public CardSuit TrumpSuit;
        public int MarriagePoints;
        public int BetMultiplier;
        public bool SevenWon;
        public bool HundredWon;
        public MariasGameRules.GameResult Result;
    }

    public enum GameEventType
    {
        GameStarted,
        PhaseChanged,
        PlayerTurnStarted,
        TrickWon,
        MarriageDeclared,
        TrumpDeclared,
        BetDoubled,
        SevenDeclared,
        SevenResolved,
        HundredDeclared,
        HundredResolved,
        BettelDeclared,
        DurchDeclared,
        GameEnded
    }
}
