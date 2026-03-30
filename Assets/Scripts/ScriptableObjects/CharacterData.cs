using UnityEngine;

namespace MariasGame.ScriptableObjects
{
    [CreateAssetMenu(fileName = "CharacterData", menuName = "MariasGame/Character Data", order = 4)]
    public class CharacterData : ScriptableObject
    {
        [field: SerializeField] public int CharacterId { get; private set; }
        [field: SerializeField] public string CharacterName { get; private set; }
        [field: SerializeField] public Sprite CharacterSprite { get; private set; }

        private void OnValidate()
        {
            if (CharacterSprite == null)
                Debug.LogWarning($"[CharacterData] {name}: Sprite is missing!");
            if (string.IsNullOrEmpty(CharacterName))
                Debug.LogWarning($"[CharacterData] {name}: Name is missing!");
        }
    }
}
