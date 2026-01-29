namespace MariasGame.Core
{
    /// <summary>
    /// Stavy karty v průběhu hry.
    /// Karta může být v různých stavech, které určují její chování a animace.
    /// </summary>
    public enum CardState
    {
        /// <summary>
        /// Karta je v balíčku (deck).
        /// </summary>
        InDeck,
        
        /// <summary>
        /// Karta se právě líže (animace z balíčku do ruky).
        /// </summary>
        Dealing,
        
        /// <summary>
        /// Karta je v ruce hráče.
        /// </summary>
        InHand,
        
        /// <summary>
        /// Karta je vybrána v ruce (UI stav).
        /// </summary>
        Selected,
        
        /// <summary>
        /// Karta je právě hraná (animace z ruky na stůl).
        /// </summary>
        Playing,
        
        /// <summary>
        /// Karta se právě odhazuje (animace z ruky do discard pile).
        /// </summary>
        Discarding,
        
        /// <summary>
        /// Karta je v discard pile.
        /// </summary>
        InDiscardPile,
        
        /// <summary>
        /// Karta je na stole (hrána).
        /// </summary>
        OnTable
    }
}

