using System.Collections.Generic;
using UnityEngine;
using MariasGame.Core.Interfaces;

namespace MariasGame.Services
{
    /// <summary>
    /// Služba pro načítání Unity assetů.
    /// Cachuje načtené assety pro lepší výkon.
    /// </summary>
    public class AssetLoaderService : IAssetLoader
    {
        private readonly Dictionary<string, Sprite> _spriteCache = new Dictionary<string, Sprite>();
        private readonly Dictionary<string, AudioClip> _soundCache = new Dictionary<string, AudioClip>();
        private readonly Dictionary<string, AnimationClip> _animationCache = new Dictionary<string, AnimationClip>();
        
        /// <summary>
        /// Načte sprite z Unity Resources.
        /// </summary>
        public Sprite LoadSprite(string spritePath)
        {
            if (string.IsNullOrEmpty(spritePath))
                return null;
                
            if (_spriteCache.TryGetValue(spritePath, out var cachedSprite))
                return cachedSprite;
                
            var sprite = Resources.Load<Sprite>(spritePath);
            if (sprite != null)
            {
                _spriteCache[spritePath] = sprite;
            }
            else
            {
                Debug.LogWarning($"[AssetLoaderService] Sprite not found at path: {spritePath}");
            }
            
            return sprite;
        }
        
        /// <summary>
        /// Načte sound z Unity Resources.
        /// </summary>
        public AudioClip LoadSound(string soundPath)
        {
            if (string.IsNullOrEmpty(soundPath))
                return null;
                
            if (_soundCache.TryGetValue(soundPath, out var cachedSound))
                return cachedSound;
                
            var sound = Resources.Load<AudioClip>(soundPath);
            if (sound != null)
            {
                _soundCache[soundPath] = sound;
            }
            else
            {
                Debug.LogWarning($"[AssetLoaderService] Sound not found at path: {soundPath}");
            }
            
            return sound;
        }
        
        /// <summary>
        /// Načte animaci z Unity Resources.
        /// </summary>
        public AnimationClip LoadAnimation(string animationPath)
        {
            if (string.IsNullOrEmpty(animationPath))
                return null;
                
            if (_animationCache.TryGetValue(animationPath, out var cachedAnimation))
                return cachedAnimation;
                
            var animation = Resources.Load<AnimationClip>(animationPath);
            if (animation != null)
            {
                _animationCache[animationPath] = animation;
            }
            else
            {
                Debug.LogWarning($"[AssetLoaderService] Animation not found at path: {animationPath}");
            }
            
            return animation;
        }
        
        /// <summary>
        /// Vyčistí cache assetů.
        /// </summary>
        public void ClearCache()
        {
            _spriteCache.Clear();
            _soundCache.Clear();
            _animationCache.Clear();
        }
    }
}

