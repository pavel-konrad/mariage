using System;
using System.Collections.Generic;
using UnityEngine;

namespace MariasGame.Managers
{
    /// <summary>
    /// Manager pro správu vizuálních efektů.
    /// Poskytuje centralizované API pro spouštění VFX.
    /// </summary>
    public class VFXManager : MonoBehaviour
    {
        [Header("VFX Prefabs")]
        [SerializeField] private VFXLibrary vfxLibrary;
        
        [Header("Settings")]
        [SerializeField] private Transform vfxContainer;
        [SerializeField] private bool enableVFX = true;
        [SerializeField] [Range(0f, 1f)] private float vfxIntensity = 1f;
        
        // Events pro designéry
        public event Action<string, Vector3> OnVFXSpawned;
        public event Action<string> OnVFXCompleted;
        
        private Dictionary<string, GameObject> _activeVFX = new Dictionary<string, GameObject>();
        private int _vfxIdCounter = 0;
        
        void Awake()
        {
            if (vfxContainer == null)
            {
                var containerGO = new GameObject("VFXContainer");
                containerGO.transform.SetParent(transform);
                vfxContainer = containerGO.transform;
            }
        }
        
        #region Card VFX
        
        /// <summary>
        /// Spustí efekt položení karty.
        /// </summary>
        public void PlayCardPlaceEffect(Vector3 position)
        {
            SpawnVFX(vfxLibrary?.cardPlaceEffect, position, "CardPlace");
        }
        
        /// <summary>
        /// Spustí efekt lížení karty.
        /// </summary>
        public void PlayCardDrawEffect(Vector3 position)
        {
            SpawnVFX(vfxLibrary?.cardDrawEffect, position, "CardDraw");
        }
        
        /// <summary>
        /// Spustí efekt otočení karty.
        /// </summary>
        public void PlayCardFlipEffect(Vector3 position)
        {
            SpawnVFX(vfxLibrary?.cardFlipEffect, position, "CardFlip");
        }
        
        /// <summary>
        /// Spustí efekt výběru karty (highlight).
        /// </summary>
        public GameObject PlayCardSelectEffect(Vector3 position)
        {
            return SpawnVFX(vfxLibrary?.cardSelectEffect, position, "CardSelect", autoDestroy: false);
        }
        
        /// <summary>
        /// Spustí efekt hratelné karty (glow).
        /// </summary>
        public GameObject PlayPlayableCardEffect(Vector3 position)
        {
            return SpawnVFX(vfxLibrary?.playableCardEffect, position, "PlayableCard", autoDestroy: false);
        }
        
        #endregion
        
        #region Game VFX
        
        /// <summary>
        /// Spustí efekt výhry štychu.
        /// </summary>
        public void PlayTrickWinEffect(Vector3 position)
        {
            SpawnVFX(vfxLibrary?.trickWinEffect, position, "TrickWin");
        }
        
        /// <summary>
        /// Spustí efekt výhry hry.
        /// </summary>
        public void PlayGameWinEffect(Vector3 position)
        {
            SpawnVFX(vfxLibrary?.gameWinEffect, position, "GameWin");
        }
        
        /// <summary>
        /// Spustí efekt prohry hry.
        /// </summary>
        public void PlayGameLoseEffect(Vector3 position)
        {
            SpawnVFX(vfxLibrary?.gameLoseEffect, position, "GameLose");
        }
        
        /// <summary>
        /// Spustí efekt hlášení (Mariáš).
        /// </summary>
        public void PlayMarriageEffect(Vector3 position)
        {
            SpawnVFX(vfxLibrary?.marriageEffect, position, "Marriage");
        }
        
        /// <summary>
        /// Spustí efekt sedmy (poslední štych).
        /// </summary>
        public void PlaySevenEffect(Vector3 position)
        {
            SpawnVFX(vfxLibrary?.sevenEffect, position, "Seven");
        }
        
        /// <summary>
        /// Spustí efekt betlu.
        /// </summary>
        public void PlayBettelEffect(Vector3 position)
        {
            SpawnVFX(vfxLibrary?.bettelEffect, position, "Bettel");
        }
        
        /// <summary>
        /// Spustí efekt durchu.
        /// </summary>
        public void PlayDurchEffect(Vector3 position)
        {
            SpawnVFX(vfxLibrary?.durchEffect, position, "Durch");
        }
        
        /// <summary>
        /// Spustí konfety efekt (pro velké výhry).
        /// </summary>
        public void PlayConfettiEffect(Vector3 position)
        {
            SpawnVFX(vfxLibrary?.confettiEffect, position, "Confetti");
        }
        
        #endregion
        
        #region UI VFX
        
        /// <summary>
        /// Spustí efekt zvýraznění tlačítka.
        /// </summary>
        public GameObject PlayButtonHighlightEffect(Vector3 position)
        {
            return SpawnVFX(vfxLibrary?.buttonHighlightEffect, position, "ButtonHighlight", autoDestroy: false);
        }
        
        /// <summary>
        /// Spustí efekt notifikace.
        /// </summary>
        public void PlayNotificationEffect(Vector3 position)
        {
            SpawnVFX(vfxLibrary?.notificationEffect, position, "Notification");
        }
        
        /// <summary>
        /// Spustí efekt změny skóre.
        /// </summary>
        public void PlayScoreChangeEffect(Vector3 position, bool isPositive)
        {
            var prefab = isPositive ? vfxLibrary?.scoreIncreaseEffect : vfxLibrary?.scoreDecreaseEffect;
            SpawnVFX(prefab, position, isPositive ? "ScoreIncrease" : "ScoreDecrease");
        }
        
        #endregion
        
        #region Core Methods
        
        /// <summary>
        /// Spustí VFX prefab na dané pozici.
        /// </summary>
        private GameObject SpawnVFX(GameObject prefab, Vector3 position, string effectName, bool autoDestroy = true)
        {
            if (!enableVFX || prefab == null)
            {
                if (prefab == null && enableVFX)
                    Debug.LogWarning($"[VFXManager] VFX prefab '{effectName}' not assigned!");
                return null;
            }
            
            var vfx = Instantiate(prefab, position, Quaternion.identity, vfxContainer);
            var vfxId = $"{effectName}_{_vfxIdCounter++}";
            
            // Aplikovat intenzitu (pokud má ParticleSystem)
            var particleSystems = vfx.GetComponentsInChildren<ParticleSystem>();
            foreach (var ps in particleSystems)
            {
                var main = ps.main;
                var currentStart = main.startSize;
                main.startSize = new ParticleSystem.MinMaxCurve(currentStart.constant * vfxIntensity);
            }
            
            _activeVFX[vfxId] = vfx;
            OnVFXSpawned?.Invoke(effectName, position);
            
            if (autoDestroy)
            {
                // Auto-destroy po delší době (nebo když ParticleSystem dokončí)
                float duration = GetVFXDuration(vfx);
                StartCoroutine(DestroyVFXAfterDelay(vfxId, duration));
            }
            
            return vfx;
        }
        
        /// <summary>
        /// Získá délku VFX.
        /// </summary>
        private float GetVFXDuration(GameObject vfx)
        {
            var ps = vfx.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                return ps.main.duration + ps.main.startLifetime.constant;
            }
            return 2f; // Default 2 seconds
        }
        
        /// <summary>
        /// Zničí VFX po zpoždění.
        /// </summary>
        private System.Collections.IEnumerator DestroyVFXAfterDelay(string vfxId, float delay)
        {
            yield return new WaitForSeconds(delay);
            
            if (_activeVFX.TryGetValue(vfxId, out var vfx))
            {
                _activeVFX.Remove(vfxId);
                if (vfx != null)
                {
                    Destroy(vfx);
                }
                OnVFXCompleted?.Invoke(vfxId);
            }
        }
        
        /// <summary>
        /// Zničí konkrétní VFX instanci.
        /// </summary>
        public void DestroyVFX(GameObject vfx)
        {
            if (vfx == null) return;
            
            // Najít a odstranit z dictionary
            string keyToRemove = null;
            foreach (var kvp in _activeVFX)
            {
                if (kvp.Value == vfx)
                {
                    keyToRemove = kvp.Key;
                    break;
                }
            }
            
            if (keyToRemove != null)
            {
                _activeVFX.Remove(keyToRemove);
            }
            
            Destroy(vfx);
        }
        
        /// <summary>
        /// Zničí všechny aktivní VFX.
        /// </summary>
        public void ClearAllVFX()
        {
            foreach (var vfx in _activeVFX.Values)
            {
                if (vfx != null)
                {
                    Destroy(vfx);
                }
            }
            _activeVFX.Clear();
        }
        
        #endregion
        
        #region Settings
        
        public void SetVFXEnabled(bool enabled)
        {
            enableVFX = enabled;
            if (!enabled)
            {
                ClearAllVFX();
            }
        }
        
        public void SetVFXIntensity(float intensity)
        {
            vfxIntensity = Mathf.Clamp01(intensity);
        }
        
        public bool IsVFXEnabled() => enableVFX;
        public float GetVFXIntensity() => vfxIntensity;
        
        #endregion
    }
    
    /// <summary>
    /// Knihovna VFX prefabů.
    /// </summary>
    [System.Serializable]
    public class VFXLibrary
    {
        [Header("Card Effects")]
        public GameObject cardPlaceEffect;
        public GameObject cardDrawEffect;
        public GameObject cardFlipEffect;
        public GameObject cardSelectEffect;
        public GameObject playableCardEffect;
        
        [Header("Game Effects")]
        public GameObject trickWinEffect;
        public GameObject gameWinEffect;
        public GameObject gameLoseEffect;
        public GameObject marriageEffect;
        public GameObject sevenEffect;
        public GameObject bettelEffect;
        public GameObject durchEffect;
        public GameObject confettiEffect;
        
        [Header("UI Effects")]
        public GameObject buttonHighlightEffect;
        public GameObject notificationEffect;
        public GameObject scoreIncreaseEffect;
        public GameObject scoreDecreaseEffect;
    }
}
