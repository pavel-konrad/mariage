using UnityEngine;

namespace MariasGame.ScriptableObjects
{
    public enum AIDifficulty { Easy, Medium, Hard }

    [CreateAssetMenu(fileName = "GameModeConfig", menuName = "Marias/GameModeConfig")]
    public class GameModeConfig : ScriptableObject
    {
        [field: SerializeField] public string ModeName { get; private set; }

        [Tooltip("Hráč (index 0) je vždy forhont. Přeskočí dražbu.")]
        [field: SerializeField] public bool HumanAlwaysDeclarer { get; private set; }

        [Tooltip("Povolí dražbu mezi hráči.")]
        [field: SerializeField] public bool HasBidding { get; private set; }

        [Tooltip("Povolí typy hry Betl a Durch.")]
        [field: SerializeField] public bool HasBettelDurch { get; private set; }

        [Tooltip("Povolí zdvojení sázky (Flek). Hard přidává Re.")]
        [field: SerializeField] public bool HasFlekRe { get; private set; }

        [field: SerializeField] public AIDifficulty AIDifficulty { get; private set; }
    }
}
