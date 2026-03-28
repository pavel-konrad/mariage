using System.Collections.Generic;

namespace MariasGame.Core
{
    public class RoundResult
    {
        public int RoundNumber;
        public int DeclarerIndex;
        public int WinnerIndex;                    // -1 = obránci vyhráli
        public int[] ScoreDeltas;                  // skóre změna za toto kolo (per player)
        public MariasGameRules.GameResult GameResult;
    }

    /// <summary>
    /// Drží kumulativní stav session (více kol).
    /// Pure C# třída bez Unity závislostí.
    /// </summary>
    public class GameSessionData
    {
        public int RoundsPlayed { get; private set; }
        public int[] CumulativeScores { get; } = new int[3];
        public IReadOnlyList<RoundResult> RoundHistory => _roundHistory;

        private readonly List<RoundResult> _roundHistory = new();

        public void RecordRoundResult(MariasGameRules.GameResult result, int declarerIndex, int winnerIndex)
        {
            var deltas = new int[3];
            for (int i = 0; i < 3; i++)
            {
                deltas[i] = result.GetTotalScore(i, declarerIndex);
                CumulativeScores[i] += deltas[i];
            }

            _roundHistory.Add(new RoundResult
            {
                RoundNumber   = ++RoundsPlayed,
                DeclarerIndex = declarerIndex,
                WinnerIndex   = winnerIndex,
                ScoreDeltas   = deltas,
                GameResult    = result
            });
        }

        public void Reset()
        {
            RoundsPlayed = 0;
            for (int i = 0; i < 3; i++) CumulativeScores[i] = 0;
            _roundHistory.Clear();
        }
    }
}
