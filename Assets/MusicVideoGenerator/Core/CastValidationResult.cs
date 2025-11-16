using UnityEngine;
using UnityEngine.Playables;
using System.Collections.Generic;
using System.Linq;

namespace MusicVideoGenerator
{
    /// <summary>
    /// Result of validating a cast setup.
    /// </summary>
    public class CastValidationResult
    {
        public List<string> errors = new List<string>();
        public List<string> warnings = new List<string>();
        public List<string> info = new List<string>();
        
        public bool IsValid => errors.Count == 0;
        public bool HasWarnings => warnings.Count > 0;
        
        public void AddError(string message) => errors.Add(message);
        public void AddWarning(string message) => warnings.Add(message);
        public void AddInfo(string message) => info.Add(message);
        
        public string GetSummary()
        {
            if (IsValid && !HasWarnings)
            {
                return string.Join("\n", info);
            }
            
            var summary = "";
            if (errors.Count > 0)
            {
                summary += "ERRORS:\n" + string.Join("\n", errors.Select(e => "  • " + e)) + "\n\n";
            }
            if (warnings.Count > 0)
            {
                summary += "WARNINGS:\n" + string.Join("\n", warnings.Select(w => "  • " + w)) + "\n\n";
            }
            if (info.Count > 0)
            {
                summary += string.Join("\n", info);
            }
            return summary.Trim();
        }
    }
}