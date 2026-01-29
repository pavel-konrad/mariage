using System;
using System.Collections.Generic;
using UnityEngine;

namespace MariasGame.Managers
{
    /// <summary>
    /// Manager pro správu zvuků a hudby.
    /// Poskytuje centralizované API pro přehrávání zvuků.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource ambientSource;
        
        [Header("Sound Libraries")]
        [SerializeField] private SoundLibrary cardSounds;
        [SerializeField] private SoundLibrary uiSounds;
        [SerializeField] private SoundLibrary gameSounds;
        
        [Header("Settings")]
        [SerializeField] [Range(0f, 1f)] private float masterVolume = 1f;
        [SerializeField] [Range(0f, 1f)] private float musicVolume = 0.7f;
        [SerializeField] [Range(0f, 1f)] private float sfxVolume = 1f;
        [SerializeField] [Range(0f, 1f)] private float ambientVolume = 0.5f;
        
        // Events pro designéry
        public event Action<string> OnSoundPlayed;
        public event Action<string> OnMusicChanged;
        
        private Dictionary<string, AudioClip> _soundCache = new Dictionary<string, AudioClip>();
        
        void Awake()
        {
            SetupAudioSources();
        }
        
        private void SetupAudioSources()
        {
            if (musicSource == null)
            {
                var musicGO = new GameObject("MusicSource");
                musicGO.transform.SetParent(transform);
                musicSource = musicGO.AddComponent<AudioSource>();
                musicSource.loop = true;
            }
            
            if (sfxSource == null)
            {
                var sfxGO = new GameObject("SFXSource");
                sfxGO.transform.SetParent(transform);
                sfxSource = sfxGO.AddComponent<AudioSource>();
            }
            
            if (ambientSource == null)
            {
                var ambientGO = new GameObject("AmbientSource");
                ambientGO.transform.SetParent(transform);
                ambientSource = ambientGO.AddComponent<AudioSource>();
                ambientSource.loop = true;
            }
            
            ApplyVolumeSettings();
        }
        
        private void ApplyVolumeSettings()
        {
            if (musicSource != null) musicSource.volume = musicVolume * masterVolume;
            if (sfxSource != null) sfxSource.volume = sfxVolume * masterVolume;
            if (ambientSource != null) ambientSource.volume = ambientVolume * masterVolume;
        }
        
        #region Card Sounds
        
        /// <summary>
        /// Přehraje zvuk položení karty.
        /// </summary>
        public void PlayCardPlace()
        {
            PlaySound(cardSounds?.cardPlace, "CardPlace");
        }
        
        /// <summary>
        /// Přehraje zvuk lížení karty.
        /// </summary>
        public void PlayCardDraw()
        {
            PlaySound(cardSounds?.cardDraw, "CardDraw");
        }
        
        /// <summary>
        /// Přehraje zvuk míchání karet.
        /// </summary>
        public void PlayCardShuffle()
        {
            PlaySound(cardSounds?.cardShuffle, "CardShuffle");
        }
        
        /// <summary>
        /// Přehraje zvuk rozdávání karet.
        /// </summary>
        public void PlayCardDeal()
        {
            PlaySound(cardSounds?.cardDeal, "CardDeal");
        }
        
        /// <summary>
        /// Přehraje zvuk otočení karty.
        /// </summary>
        public void PlayCardFlip()
        {
            PlaySound(cardSounds?.cardFlip, "CardFlip");
        }
        
        #endregion
        
        #region Game Sounds
        
        /// <summary>
        /// Přehraje zvuk výhry štychu.
        /// </summary>
        public void PlayTrickWin()
        {
            PlaySound(gameSounds?.trickWin, "TrickWin");
        }
        
        /// <summary>
        /// Přehraje zvuk výhry hry.
        /// </summary>
        public void PlayGameWin()
        {
            PlaySound(gameSounds?.gameWin, "GameWin");
        }
        
        /// <summary>
        /// Přehraje zvuk prohry hry.
        /// </summary>
        public void PlayGameLose()
        {
            PlaySound(gameSounds?.gameLose, "GameLose");
        }
        
        /// <summary>
        /// Přehraje zvuk hlášení (Mariáš).
        /// </summary>
        public void PlayMarriageCall()
        {
            PlaySound(gameSounds?.marriageCall, "MarriageCall");
        }
        
        /// <summary>
        /// Přehraje zvuk sedmy.
        /// </summary>
        public void PlaySevenCall()
        {
            PlaySound(gameSounds?.sevenCall, "SevenCall");
        }
        
        /// <summary>
        /// Přehraje zvuk betlu.
        /// </summary>
        public void PlayBettelCall()
        {
            PlaySound(gameSounds?.bettelCall, "BettelCall");
        }
        
        /// <summary>
        /// Přehraje zvuk durchu.
        /// </summary>
        public void PlayDurchCall()
        {
            PlaySound(gameSounds?.durchCall, "DurchCall");
        }
        
        #endregion
        
        #region UI Sounds
        
        /// <summary>
        /// Přehraje zvuk kliknutí na tlačítko.
        /// </summary>
        public void PlayButtonClick()
        {
            PlaySound(uiSounds?.buttonClick, "ButtonClick");
        }
        
        /// <summary>
        /// Přehraje zvuk hoveru nad tlačítkem.
        /// </summary>
        public void PlayButtonHover()
        {
            PlaySound(uiSounds?.buttonHover, "ButtonHover");
        }
        
        /// <summary>
        /// Přehraje zvuk notifikace.
        /// </summary>
        public void PlayNotification()
        {
            PlaySound(uiSounds?.notification, "Notification");
        }
        
        #endregion
        
        #region Music Control
        
        /// <summary>
        /// Přehraje hudbu.
        /// </summary>
        public void PlayMusic(AudioClip music, bool fade = true)
        {
            if (musicSource == null || music == null) return;
            
            if (fade)
            {
                StartCoroutine(FadeMusicCoroutine(music));
            }
            else
            {
                musicSource.clip = music;
                musicSource.Play();
            }
            
            OnMusicChanged?.Invoke(music.name);
        }
        
        /// <summary>
        /// Zastaví hudbu.
        /// </summary>
        public void StopMusic(bool fade = true)
        {
            if (musicSource == null) return;
            
            if (fade)
            {
                StartCoroutine(FadeOutMusicCoroutine());
            }
            else
            {
                musicSource.Stop();
            }
        }
        
        private System.Collections.IEnumerator FadeMusicCoroutine(AudioClip newMusic)
        {
            float startVolume = musicSource.volume;
            
            // Fade out
            while (musicSource.volume > 0)
            {
                musicSource.volume -= startVolume * Time.deltaTime / 0.5f;
                yield return null;
            }
            
            musicSource.Stop();
            musicSource.clip = newMusic;
            musicSource.Play();
            
            // Fade in
            while (musicSource.volume < musicVolume * masterVolume)
            {
                musicSource.volume += startVolume * Time.deltaTime / 0.5f;
                yield return null;
            }
            
            musicSource.volume = musicVolume * masterVolume;
        }
        
        private System.Collections.IEnumerator FadeOutMusicCoroutine()
        {
            float startVolume = musicSource.volume;
            
            while (musicSource.volume > 0)
            {
                musicSource.volume -= startVolume * Time.deltaTime / 0.5f;
                yield return null;
            }
            
            musicSource.Stop();
        }
        
        #endregion
        
        #region Core Methods
        
        /// <summary>
        /// Přehraje zvuk.
        /// </summary>
        private void PlaySound(AudioClip clip, string soundName)
        {
            if (sfxSource == null || clip == null)
            {
                if (clip == null)
                    Debug.LogWarning($"[AudioManager] Sound '{soundName}' not found!");
                return;
            }
            
            sfxSource.PlayOneShot(clip);
            OnSoundPlayed?.Invoke(soundName);
        }
        
        /// <summary>
        /// Přehraje vlastní zvuk podle názvu.
        /// </summary>
        public void PlayCustomSound(string soundName)
        {
            if (_soundCache.TryGetValue(soundName, out var clip))
            {
                PlaySound(clip, soundName);
            }
            else
            {
                Debug.LogWarning($"[AudioManager] Custom sound '{soundName}' not registered!");
            }
        }
        
        /// <summary>
        /// Registruje vlastní zvuk.
        /// </summary>
        public void RegisterCustomSound(string name, AudioClip clip)
        {
            _soundCache[name] = clip;
        }
        
        #endregion
        
        #region Volume Control
        
        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            ApplyVolumeSettings();
        }
        
        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            ApplyVolumeSettings();
        }
        
        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            ApplyVolumeSettings();
        }
        
        public float GetMasterVolume() => masterVolume;
        public float GetMusicVolume() => musicVolume;
        public float GetSFXVolume() => sfxVolume;
        
        #endregion
    }
    
    /// <summary>
    /// Knihovna zvuků pro karty.
    /// </summary>
    [System.Serializable]
    public class SoundLibrary
    {
        [Header("Card Sounds")]
        public AudioClip cardPlace;
        public AudioClip cardDraw;
        public AudioClip cardShuffle;
        public AudioClip cardDeal;
        public AudioClip cardFlip;
        
        [Header("Game Sounds")]
        public AudioClip trickWin;
        public AudioClip gameWin;
        public AudioClip gameLose;
        public AudioClip marriageCall;
        public AudioClip sevenCall;
        public AudioClip bettelCall;
        public AudioClip durchCall;
        
        [Header("UI Sounds")]
        public AudioClip buttonClick;
        public AudioClip buttonHover;
        public AudioClip notification;
    }
}
