using System.Collections.Generic;
using UnityEngine;

namespace MariasGame.Core.Interfaces
{
    /// <summary>
    /// Interface pro poskytování avatarů (pro lidské hráče).
    /// Abstrakce pro přístup k avatarům (ScriptableObject, Database, atd.).
    /// </summary>
    public interface IAvatarProvider
    {
        AvatarData GetAvatar(int index);
        IReadOnlyList<AvatarData> GetAllAvatars();
        int AvatarCount { get; }
    }
    
    /// <summary>
    /// Data avataru (pro lidské hráče).
    /// </summary>
    [System.Serializable]
    public class AvatarData
    {
        public string AvatarName;
        public Sprite AvatarSprite;
        public int AvatarId;
    }
}

