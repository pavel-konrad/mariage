using System;
using System.Collections.Generic;
using System.Linq;

namespace MariasGame.Core
{
    /// <summary>
    /// Herní stav pro Mariáš.
    /// Drží veškerý state hry - lze serializovat pro save/load.
    /// </summary>
    [Serializable]
    public class MariasGameState
    {
        #region Game Setup
        
        /// <summary>Typ aktuální hry.</summary>
        public MariasGameRules.GameType GameType { get; set; } = MariasGameRules.GameType.Normal;
        
        /// <summary>Index hráče, který hraje (forhont).</summary>
        public int DeclarerIndex { get; set; }
        
        /// <summary>Trumfová barva (null pro betl/durch).</summary>
        public CardSuit? TrumpSuit { get; set; }
        
        /// <summary>Násobič sázky (fléky).</summary>
        public int BetMultiplier { get; set; } = 1;
        
        #endregion
        
        #region Players
        
        /// <summary>Ruky hráčů.</summary>
        public List<List<Card>> PlayerHands { get; set; } = new List<List<Card>>();
        
        /// <summary>Jména hráčů.</summary>
        public List<string> PlayerNames { get; set; } = new List<string>();
        
        /// <summary>Body hráčů ze štychů.</summary>
        public int[] PlayerTrickPoints { get; set; } = new int[3];
        
        /// <summary>Body z hlášek.</summary>
        public int[] PlayerMarriagePoints { get; set; } = new int[3];
        
        /// <summary>Počet vyhraných štychů.</summary>
        public int[] PlayerTrickCount { get; set; } = new int[3];
        
        #endregion
        
        #region Current Round
        
        /// <summary>Aktuální štych (karty zahrané v tomto kole).</summary>
        public List<Card> CurrentTrick { get; set; } = new List<Card>();
        
        /// <summary>Index hráče, který je na tahu.</summary>
        public int CurrentPlayerIndex { get; set; }
        
        /// <summary>Index hráče, který vynášel tento štych.</summary>
        public int TrickLeaderIndex { get; set; }
        
        /// <summary>Číslo aktuálního štychu (1-10).</summary>
        public int TrickNumber { get; set; } = 1;
        
        #endregion
        
        #region Game Phase
        
        /// <summary>Aktuální fáze hry.</summary>
        public GamePhase Phase { get; set; } = GamePhase.Dealing;
        
        /// <summary>Talon (2 karty pro forhonta).</summary>
        public List<Card> Talon { get; set; } = new List<Card>();
        
        /// <summary>Karty odhozené forhentem do talonu.</summary>
        public List<Card> DiscardedTalon { get; set; } = new List<Card>();
        
        #endregion
        
        #region Declarations
        
        /// <summary>Hlášené páry (barvy).</summary>
        public List<CardSuit> DeclaredMarriages { get; set; } = new List<CardSuit>();
        
        /// <summary>Zda byla hlášena sedma.</summary>
        public bool SevenDeclared { get; set; }
        
        /// <summary>Zda byla hlášena stovka.</summary>
        public bool HundredDeclared { get; set; }
        
        #endregion
        
        #region Game End
        
        /// <summary>Zda hra skončila.</summary>
        public bool IsGameOver { get; set; }
        
        /// <summary>Index vítěze (null pokud remíza nebo hra neskončila).</summary>
        public int? WinnerIndex { get; set; }
        
        /// <summary>Výsledek hry.</summary>
        public MariasGameRules.GameResult Result { get; set; }
        
        #endregion
        
        #region History
        
        /// <summary>Historie štychů (pro replay/AI analýzu).</summary>
        public List<TrickHistory> TrickHistory { get; set; } = new List<TrickHistory>();
        
        #endregion
        
        #region Methods
        
        /// <summary>
        /// Vytvoří nový herní stav pro začátek hry.
        /// </summary>
        public static MariasGameState CreateNew(List<string> playerNames)
        {
            if (playerNames.Count != 3)
                throw new ArgumentException("Mariáš vyžaduje přesně 3 hráče.");
            
            return new MariasGameState
            {
                PlayerNames = playerNames,
                PlayerHands = new List<List<Card>> { new(), new(), new() },
                PlayerTrickPoints = new int[3],
                PlayerMarriagePoints = new int[3],
                PlayerTrickCount = new int[3],
                Phase = GamePhase.Dealing
            };
        }
        
        /// <summary>
        /// Resetuje stav pro novou hru (zachová hráče).
        /// </summary>
        public void Reset()
        {
            GameType = MariasGameRules.GameType.Normal;
            DeclarerIndex = 0;
            TrumpSuit = null;
            BetMultiplier = 1;
            
            for (int i = 0; i < 3; i++)
            {
                PlayerHands[i].Clear();
                PlayerTrickPoints[i] = 0;
                PlayerMarriagePoints[i] = 0;
                PlayerTrickCount[i] = 0;
            }
            
            CurrentTrick.Clear();
            CurrentPlayerIndex = 0;
            TrickLeaderIndex = 0;
            TrickNumber = 1;
            
            Phase = GamePhase.Dealing;
            Talon.Clear();
            DiscardedTalon.Clear();
            
            DeclaredMarriages.Clear();
            SevenDeclared = false;
            HundredDeclared = false;
            
            IsGameOver = false;
            WinnerIndex = null;
            Result = null;
            
            TrickHistory.Clear();
        }
        
        /// <summary>
        /// Získá ruku aktuálního hráče.
        /// </summary>
        public List<Card> GetCurrentPlayerHand()
        {
            return PlayerHands[CurrentPlayerIndex];
        }
        
        /// <summary>
        /// Přesune na dalšího hráče.
        /// </summary>
        public void NextPlayer()
        {
            CurrentPlayerIndex = (CurrentPlayerIndex + 1) % 3;
        }
        
        /// <summary>
        /// Zkontroluje, zda je konec štychu.
        /// </summary>
        public bool IsTrickComplete()
        {
            return CurrentTrick.Count == 3;
        }
        
        /// <summary>
        /// Zkontroluje, zda je konec hry.
        /// </summary>
        public bool IsLastTrick()
        {
            return TrickNumber >= 10;
        }
        
        #endregion
    }
    
    /// <summary>
    /// Fáze hry v Mariáši.
    /// </summary>
    public enum GamePhase
    {
        /// <summary>Rozdávání karet.</summary>
        Dealing,
        
        /// <summary>Dražba - určení kdo hraje.</summary>
        Bidding,
        
        /// <summary>Forhont si bere talon.</summary>
        TakingTalon,
        
        /// <summary>Forhont odhazuje 2 karty.</summary>
        DiscardingTalon,
        
        /// <summary>Volba trumfů a hlášení.</summary>
        Declaring,
        
        /// <summary>Hraní štychů.</summary>
        Playing,
        
        /// <summary>Konec hry - počítání bodů.</summary>
        Scoring,
        
        /// <summary>Hra skončila.</summary>
        GameOver
    }
    
    /// <summary>
    /// Historie jednoho štychu.
    /// </summary>
    [Serializable]
    public class TrickHistory
    {
        /// <summary>Číslo štychu.</summary>
        public int TrickNumber { get; set; }
        
        /// <summary>Karty ve štychu (v pořadí hraní).</summary>
        public List<Card> Cards { get; set; } = new List<Card>();
        
        /// <summary>Indexy hráčů, kteří hráli (v pořadí).</summary>
        public List<int> PlayerIndices { get; set; } = new List<int>();
        
        /// <summary>Index vítěze štychu.</summary>
        public int WinnerIndex { get; set; }
        
        /// <summary>Body ve štychu.</summary>
        public int Points { get; set; }
    }
}
