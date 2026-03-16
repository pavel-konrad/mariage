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
        /// Vyčistí cache assetů.
        /// </summary>
        public void ClearCache()
        {
            _spriteCache.Clear();
        }
    }
}

