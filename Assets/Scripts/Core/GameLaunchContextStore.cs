namespace MariasGame.Core
{
    /// <summary>
    /// Dočasné runtime úložiště pro context mezi scénami.
    /// </summary>
    public static class GameLaunchContextStore
    {
        public static GameLaunchContext Current { get; private set; }
        public static bool HasContext => Current != null;

        public static void Set(GameLaunchContext context)
        {
            Current = context;
        }

        public static void Clear()
        {
            Current = null;
        }
    }
}
