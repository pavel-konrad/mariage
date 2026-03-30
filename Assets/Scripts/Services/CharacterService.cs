using System.Collections.Generic;
using System.Linq;
using MariasGame.ScriptableObjects;

namespace MariasGame.Services
{
    public class CharacterService
    {
        private readonly CharacterDatabase _database;

        public CharacterService(CharacterDatabase database)
        {
            _database = database ?? throw new System.ArgumentNullException(nameof(database));
        }

        public IReadOnlyList<CharacterData> GetAll()
            => _database.Characters.Where(c => c != null).ToList().AsReadOnly();

        public int Count => _database.Characters?.Count(c => c != null) ?? 0;

        public CharacterData GetById(int id) => _database.GetById(id);

        public CharacterData GetByIndex(int index) => _database.GetByIndex(index);
    }
}
