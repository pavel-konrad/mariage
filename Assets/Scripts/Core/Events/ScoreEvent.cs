namespace MariasGame.Core.Events
{
    public struct ScoreEvent
    {
        public ScoreEventType Type;
        public int PlayerIndex;
        public int NewScore;
        public MariasGameRules.GameResult Result;
    }

    public enum ScoreEventType
    {
        ScoreChanged,
        RoundEnded
    }
}
