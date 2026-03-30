using UnityEngine;

namespace MariasGame.UI
{
    /// <summary>
    /// Receiver pro legacy AnimationEvents na avatar animacích.
    /// </summary>
    public class AvatarAnimationEventReceiver : MonoBehaviour
    {
        // Voláno z AnimationEvent "OnPopAnimationStart"
        public void OnPopAnimationStart()
        {
            // Intentionally empty: event is optional for menu avatar visuals.
        }
    }
}
