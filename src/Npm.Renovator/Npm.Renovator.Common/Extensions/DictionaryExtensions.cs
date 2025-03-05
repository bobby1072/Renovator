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
}