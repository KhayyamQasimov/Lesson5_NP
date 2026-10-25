using System.Security.Principal;
using System.Text.Json;
using System.Text.Json.Nodes;
using Shared.Shared;

namespace Server.Repositories
{
    public class Repository<T> : IRepository<T> where T : class, IEntity
    {
        private static readonly JsonSerializerOptions _options = new() { WriteIndented = true };

        private readonly List<T> _items;
        private readonly string _filePath;
        private readonly string _rootKey;
        private readonly object _lock = new();

        public Repository(string filePath, string rootKey)
        {
            _filePath = filePath;
            _rootKey = rootKey;
            _items = Load();
        }

        public virtual List<T> GetAll()
        {
            lock (_lock)
            {
                return _items.ToList();
            }
        }

        public virtual T? GetById(int id)
        {
            lock (_lock)
            {
                return _items.FirstOrDefault(x => x.Id == id);
            }
        }

        public virtual bool Add(T entity)
        {
            if (entity == null) return false;

            lock (_lock)
            {
                entity.Id = _items.Count == 0 ? 1 : _items.Max(x => x.Id) + 1;
                _items.Add(entity);
                Save();
                return true;
            }
        }

        public virtual bool Update(T entity)
        {
            if (entity == null) return false;

            lock (_lock)
            {
                var index = _items.FindIndex(x => x.Id == entity.Id);
                if (index == -1) return false;

                _items[index] = entity;
                Save();
                return true;
            }
        }

        public virtual bool Delete(int id)
        {
            lock (_lock)
            {
                var entity = _items.FirstOrDefault(x => x.Id == id);
                if (entity == null) return false;

                _items.Remove(entity);
                Save();
                return true;
            }
        }

        private List<T> Load()
        {
            if (!File.Exists(_filePath)) return new List<T>();

            var root = JsonNode.Parse(File.ReadAllText(_filePath));
            return root?[_rootKey]?.Deserialize<List<T>>() ?? new List<T>();
        }

        private void Save()
        {
            var root = File.Exists(_filePath)
                ? JsonNode.Parse(File.ReadAllText(_filePath)) as JsonObject ?? new JsonObject()
                : new JsonObject();

            root[_rootKey] = JsonSerializer.SerializeToNode(_items, _options);
            File.WriteAllText(_filePath, root.ToJsonString(_options));
        }
    }
}