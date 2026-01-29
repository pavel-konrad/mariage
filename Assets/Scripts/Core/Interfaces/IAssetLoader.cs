using UnityEngine;

namespace MariasGame.Core.Interfaces
{
    /// <summary>
    /// Interface pro načítání Unity assetů.
    /// </summary>
    public interface IAssetLoader
    {
        Sprite LoadSprite(string spritePath);
        AudioClip LoadSound(string soundPath);
        AnimationClip LoadAnimation(string animationPath);
    }
}

