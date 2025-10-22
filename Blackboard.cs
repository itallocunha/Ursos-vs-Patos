using System.Collections.Generic;

namespace BT
{
    public class Blackboard
    {
        private readonly Dictionary<string, object> data = new Dictionary<string, object>();
        public void Set<T>(string key, T value) => data[key] = value;
        public bool TryGet<T>(string key, out T value)
        {
            if (data.TryGetValue(key, out var o) && o is T t) { value = t; return true; }
            value = default; return false;
        }
        public bool Has(string key) => data.ContainsKey(key);
        public void Remove(string key) { if (data.ContainsKey(key)) data.Remove(key); }
    }
}
