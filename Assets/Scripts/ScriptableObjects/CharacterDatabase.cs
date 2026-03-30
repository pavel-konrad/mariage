using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MariasGame.ScriptableObjects
{
    [CreateAssetMenu(fileName = "CharacterDatabase", menuName = "MariasGame/Character Database", order = 5)]
    public class CharacterDatabase : ScriptableObject
    {
        [field: SerializeField] public List<CharacterData> Characters { get; private set; } = new();

        public CharacterData GetByIndex(int index)
        {
            if (index < 0 || index >= Characters.Count) return null;
            return Characters[index];
        }

        public CharacterData GetById(int id)
            => Characters.FirstOrDefault(c => c != null && c.CharacterId == id);

        private void OnValidate()
        {
            if (Characters == null || Characters.Count == 0)
            {
                Debug.LogWarning($"[CharacterDatabase] {name}: No characters in database!");
                return;
            }

            var duplicateIds = Characters
                .Where(c => c != null)
                .GroupBy(c => c.CharacterId)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key);

            if (duplicateIds.Any())
                Debug.LogWarning($"[CharacterDatabase] {name}: Duplicate character IDs: {string.Join(", ", duplicateIds)}");
        }
    }
}
