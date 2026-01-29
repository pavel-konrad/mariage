using UnityEngine;
using MariasGame.Core;

namespace MariasGame.UI
{
    /// <summary>
    /// Kontrolér animací karty.
    /// Řídí přehrávání animací na základě změn stavu karty.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class CardAnimationController : MonoBehaviour
    {
        [Header("Animator")]
        [SerializeField] private Animator animator;
        
        [Header("Animation Clips")]
        [SerializeField] private AnimationClip dealAnimation;
        [SerializeField] private AnimationClip discardAnimation;
        [SerializeField] private AnimationClip playAnimation;
        [SerializeField] private AnimationClip flipAnimation;
        [SerializeField] private AnimationClip selectAnimation;
        
        [Header("Animation Settings")]
        [SerializeField] private float animationSpeed = 1.0f;
        
        // Animation parameter names
        private const string PARAM_DEAL = "Deal";
        private const string PARAM_DISCARD = "Discard";
        private const string PARAM_PLAY = "Play";
        private const string PARAM_FLIP = "Flip";
        private const string PARAM_SELECT = "Select";
        private const string PARAM_SPEED = "Speed";
        
        void Awake()
        {
            if (animator == null)
                animator = GetComponent<Animator>();
            
            if (animator != null)
            {
                animator.speed = animationSpeed;
            }
        }
        
        /// <summary>
        /// Přehrává animaci přechodu mezi stavy.
        /// </summary>
        public void PlayTransition(CardState fromState, CardState toState)
        {
            if (animator == null)
            {
                Debug.LogWarning("[CardAnimationController] Animator is not assigned!");
                return;
            }
            
            // Určit typ animace na základě přechodu
            switch (toState)
            {
                case CardState.Dealing:
                    PlayDealAnimation();
                    break;
                    
                case CardState.Discarding:
                    PlayDiscardAnimation();
                    break;
                    
                case CardState.Playing:
                    PlayPlayAnimation();
                    break;
                    
                case CardState.Selected:
                    PlaySelectAnimation();
                    break;
                    
                case CardState.InHand:
                    // Pokud přecházíme z selected, můžeme přehrát animaci zrušení výběru
                    if (fromState == CardState.Selected)
                    {
                        PlayDeselectAnimation();
                    }
                    break;
            }
        }
        
        /// <summary>
        /// Přehrává animaci lížení karty.
        /// </summary>
        public void PlayDealAnimation()
        {
            if (animator != null && animator.HasParameter(PARAM_DEAL))
            {
                animator.SetTrigger(PARAM_DEAL);
            }
            else if (dealAnimation != null)
            {
                // Fallback na AnimationClip
                PlayAnimationClip(dealAnimation);
            }
        }
        
        /// <summary>
        /// Přehrává animaci odhození karty.
        /// </summary>
        public void PlayDiscardAnimation()
        {
            if (animator != null && animator.HasParameter(PARAM_DISCARD))
            {
                animator.SetTrigger(PARAM_DISCARD);
            }
            else if (discardAnimation != null)
            {
                PlayAnimationClip(discardAnimation);
            }
        }
        
        /// <summary>
        /// Přehrává animaci hraní karty.
        /// </summary>
        public void PlayPlayAnimation()
        {
            if (animator != null && animator.HasParameter(PARAM_PLAY))
            {
                animator.SetTrigger(PARAM_PLAY);
            }
            else if (playAnimation != null)
            {
                PlayAnimationClip(playAnimation);
            }
        }
        
        /// <summary>
        /// Přehrává animaci výběru karty.
        /// </summary>
        public void PlaySelectAnimation()
        {
            if (animator != null && animator.HasParameter(PARAM_SELECT))
            {
                animator.SetTrigger(PARAM_SELECT);
            }
            else if (selectAnimation != null)
            {
                PlayAnimationClip(selectAnimation);
            }
        }
        
        /// <summary>
        /// Přehrává animaci zrušení výběru karty.
        /// </summary>
        public void PlayDeselectAnimation()
        {
            // Můžeme použít reverzní animaci nebo vlastní animaci
            PlaySelectAnimation(); // Pro jednoduchost použijeme stejnou animaci
        }
        
        /// <summary>
        /// Přehrává animaci otočení karty.
        /// </summary>
        public void PlayFlipAnimation(bool faceUp)
        {
            if (animator != null)
            {
                // Pokud máme bool parametr pro faceUp
                if (animator.HasParameter("FaceUp"))
                {
                    animator.SetBool("FaceUp", faceUp);
                }
                // Pokud máme trigger pro flip
                if (animator.HasParameter(PARAM_FLIP))
                {
                    animator.SetTrigger(PARAM_FLIP);
                }
            }
            else if (flipAnimation != null)
            {
                PlayAnimationClip(flipAnimation);
            }
        }
        
        /// <summary>
        /// Přehrává AnimationClip přímo.
        /// </summary>
        private void PlayAnimationClip(AnimationClip clip)
        {
            if (clip == null || animator == null)
                return;
            
            // Pro přehrání AnimationClip potřebujeme Animation komponentu
            var animation = GetComponent<Animation>();
            if (animation == null)
            {
                animation = gameObject.AddComponent<Animation>();
            }
            
            animation.clip = clip;
            animation.Play();
        }
        
        /// <summary>
        /// Nastaví rychlost animací.
        /// </summary>
        public void SetAnimationSpeed(float speed)
        {
            animationSpeed = speed;
            if (animator != null)
            {
                animator.speed = speed;
                if (animator.HasParameter(PARAM_SPEED))
                {
                    animator.SetFloat(PARAM_SPEED, speed);
                }
            }
        }
    }
    
    /// <summary>
    /// Extension metody pro Animator.
    /// </summary>
    public static class AnimatorExtensions
    {
        public static bool HasParameter(this Animator animator, string parameterName)
        {
            foreach (AnimatorControllerParameter param in animator.parameters)
            {
                if (param.name == parameterName)
                    return true;
            }
            return false;
        }
    }
}

