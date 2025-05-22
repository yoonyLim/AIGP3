using System.Collections.Generic;

public class Blackboard
{
    private Dictionary<string, object> data = new Dictionary<string, object>();

    public void Set<T>(string key, T value) => data[key] = value;
    public T Get<T>(string key) => data.TryGetValue(key, out object value) ? (T)value : default;
}