using System.Collections;
using System.Collections.Generic;
using OrbCreationExtensions;

namespace Frangiclave
{
    public static class HashtableExtensions
    {
        public static string GetStringOrLogError(this Hashtable hash, string key, HashSet<string> errors)
        {
            if (hash.ContainsKey(key))
            {
                if (hash[key] is string)
                {
                    return hash.GetString(key);
                }
                errors.Add($"Invalid type for '{key}', should be string");
            }
            else
            {
                errors.Add($"Missing '{key}'");                
            }
            return null;
        }

    }
}