using UnityEngine;

namespace MariasGame.ScriptableObjects
{
    /// <summary>
    /// ScriptableObject pro herní zvuky.
    /// </summary>
    [CreateAssetMenu(fileName = "SoundData", menuName = "MariasGame/Sound Data", order = 3)]
    public class SoundDataSO : ScriptableObject
    {
        [Header("Game Sounds")]
        public AudioClip cardDeal;
        public AudioClip cardFlip;
        public AudioClip cardPlay;
        public AudioClip gameWin;
    }
}

