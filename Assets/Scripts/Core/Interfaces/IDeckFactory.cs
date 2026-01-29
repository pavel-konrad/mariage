namespace MariasGame.Core.Interfaces
{
    /// <summary>
    /// Interface pro vytváření balíčků karet.
    /// </summary>
    public interface IDeckFactory
    {
        IDeck CreateStandardDeck();
    }
}

