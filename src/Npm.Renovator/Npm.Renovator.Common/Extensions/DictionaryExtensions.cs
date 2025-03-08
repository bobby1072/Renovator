namespace Npm.Renovator.Common.Extensions;

public static class DictionaryExtensions
{
    public static bool IsStringSequenceEqual(this Dictionary<string, string> dict, Dictionary<string, string> otherDict)
    {
        if(dict.Count != otherDict.Count) return false;
        foreach (var mainObj in dict)
        {
            if(!otherDict.TryGetValue(mainObj.Key, out var value)) return false;
            if(!value.Equals(mainObj.Value)) return false;
        }
        
        return true;
    }

    public static Dictionary<TKey, TValue> Clone<TKey, TValue>(this Dictionary<TKey, TValue> dict) where TKey : notnull
    {
        return dict.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }
}