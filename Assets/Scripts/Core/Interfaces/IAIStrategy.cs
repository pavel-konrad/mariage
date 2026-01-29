using System.Collections.Generic;

namespace MariasGame.Core.Interfaces
{
    /// <summary>
    /// Interface pro AI strategii v Mariáši.
    /// Definuje logiku pro výběr karty a rozhodování v dražbě.
    /// </summary>
    public interface IAIStrategy
    {
        /// <summary>
        /// Vybere kartu k zahrání ze seznamu legálních tahů.
        /// </summary>
        /// <param name="legalPlays">Karty, které je legální zahrát (již vyfiltrované MariasGameRules).</param>
        /// <param name="gameState">Aktuální stav hry (trumfy, štych, skóre, ...).</param>
        Card ChooseCardToPlay(IReadOnlyList<Card> legalPlays, MariasGameState gameState);

        /// <summary>
        /// Rozhodne o nabídce v dražbě.
        /// </summary>
        /// <param name="gameState">Aktuální stav hry.</param>
        /// <returns>Typ nabídky (Game, Seven, Hundred, Bettel, Durch, Pass).</returns>
        MariasGameRules.BidOption ChooseBid(MariasGameState gameState);

        /// <summary>
        /// Vybere trumfovou barvu (pokud je hráč forhont).
        /// </summary>
        /// <param name="hand">Karty v ruce hráče.</param>
        CardSuit ChooseTrumpSuit(IReadOnlyList<Card> hand);

        /// <summary>
        /// Vybere 2 karty k odhození do talonu.
        /// </summary>
        /// <param name="hand">12 karet v ruce (10 + 2 z talonu).</param>
        /// <param name="gameState">Aktuální stav hry.</param>
        List<Card> ChooseCardsToDiscard(IReadOnlyList<Card> hand, MariasGameState gameState);
    }
}
