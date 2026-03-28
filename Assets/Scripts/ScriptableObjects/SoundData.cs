using UnityEngine;

namespace MariasGame.ScriptableObjects
{
    [CreateAssetMenu(fileName = "SoundData", menuName = "MariasGame/Sound Data", order = 3)]
    public class SoundData : ScriptableObject
    {
        [field: SerializeField] public AudioClip CardDeal { get; private set; }
        [field: SerializeField] public AudioClip CardFlip { get; private set; }
        [field: SerializeField] public AudioClip CardPlay { get; private set; }
        [field: SerializeField] public AudioClip GameWin { get; private set; }
    }
}
