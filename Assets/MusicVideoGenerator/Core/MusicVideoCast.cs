using UnityEngine;
using UnityEngine.Playables;
using System.Collections.Generic;
using System.Linq;

namespace MusicVideoGenerator
{
    /// <summary>
    /// Main component for organizing your music video cast.
    /// Just drag and drop GameObjects, assign roles, and generate!
    /// </summary>
    public class MusicVideoCast : MonoBehaviour
    {
        [Header("🎬 Timeline Setup")]
        [Tooltip("The PlayableDirector with your Timeline")]
        public PlayableDirector playableDirector;
        
        [Tooltip("The AudioSource with your music")]
        public AudioSource audioSource;
        
        [Header("⭐ Main Cast Members")]
        [Tooltip("Lead stars, co-stars, and featured support - the story focuses on these!")]
        public List<CastMember> mainCast = new List<CastMember>();
        
        [Header("🎭 Supporting Cast Members")]
        [Tooltip("Background and ambient elements - they add atmosphere and depth")]
        public List<CastMember> supportingCast = new List<CastMember>();
        
        [Header("✨ Special Effects")]
        [Tooltip("Spotlight elements that appear dramatically at key moments")]
        public List<CastMember> spotlightCast = new List<CastMember>();
        
        /// <summary>
        /// Get all cast members combined into a single list.
        /// </summary>
        public List<CastMember> GetAllCastMembers()
        {
            var allMembers = new List<CastMember>();
            allMembers.AddRange(mainCast);
            allMembers.AddRange(supportingCast);
            allMembers.AddRange(spotlightCast);
            return allMembers.Where(m => m != null && m.IsValid()).ToList();
        }
        
        /// <summary>
        /// Get only the main cast members.
        /// </summary>
        public List<CastMember> GetMainCast()
        {
            return mainCast.Where(m => m != null && m.IsValid()).ToList();
        }
        
        /// <summary>
        /// Get only the supporting cast members.
        /// </summary>
        public List<CastMember> GetSupportingCast()
        {
            return supportingCast.Where(m => m != null && m.IsValid()).ToList();
        }
        
        /// <summary>
        /// Get only the spotlight cast members.
        /// </summary>
        public List<CastMember> GetSpotlightCast()
        {
            return spotlightCast.Where(m => m != null && m.IsValid()).ToList();
        }
        
        /// <summary>
        /// Validate the cast setup and return validation results.
        /// </summary>
        public CastValidationResult Validate()
        {
            var result = new CastValidationResult();
            
            // Check timeline setup
            if (playableDirector == null)
            {
                result.AddError("No PlayableDirector assigned");
            }
            else if (playableDirector.playableAsset == null)
            {
                result.AddError("PlayableDirector has no Timeline asset");
            }
            
            // Check audio
            if (audioSource == null)
            {
                result.AddWarning("No AudioSource assigned - music sync may not work");
            }
            else if (audioSource.clip == null)
            {
                result.AddWarning("AudioSource has no audio clip");
            }
            
            // Check cast members
            var allMembers = GetAllCastMembers();
            if (allMembers.Count == 0)
            {
                result.AddError("No cast members assigned - add at least one GameObject");
            }
            
            // Check for main cast
            var validMainCast = GetMainCast();
            if (validMainCast.Count == 0)
            {
                result.AddWarning("No main cast members - story will be less focused. Consider adding a LeadStar or CoStar!");
            }
            
            // Check for duplicates
            var gameObjects = allMembers.Select(m => m.gameObject).ToList();
            var duplicates = gameObjects.GroupBy(go => go).Where(g => g.Count() > 1).Select(g => g.Key);
            foreach (var dup in duplicates)
            {
                result.AddWarning($"GameObject '{dup.name}' appears multiple times in the cast");
            }
            
            // Success message
            if (result.IsValid)
            {
                result.AddInfo($"Cast ready! {validMainCast.Count} main, {GetSupportingCast().Count} support, {GetSpotlightCast().Count} spotlight");
            }
            
            return result;
        }
        
        /// <summary>
        /// Get the total duration from the audio clip if available.
        /// </summary>
        public float GetSongDuration()
        {
            if (audioSource != null && audioSource.clip != null)
            {
                return audioSource.clip.length;
            }
            return 0f;
        }
        
        #region Context Menu Commands
        [ContextMenu("Clear All Cast")]
        private void ClearAllCast()
        {
            mainCast.Clear();
            supportingCast.Clear();
            spotlightCast.Clear();
            Debug.Log("[MusicVideoCast] Cleared all cast members");
        }
        
        [ContextMenu("Auto-Assign Roles By Order")]
        private void AutoAssignRoles()
        {
            var allMembers = GetAllCastMembers();
            
            for (int i = 0; i < allMembers.Count; i++)
            {
                if (i == 0)
                {
                    allMembers[i].role = CastRole.LeadStar;
                }
                else if (i == 1)
                {
                    allMembers[i].role = CastRole.CoStar;
                }
                else if (i < 4)
                {
                    allMembers[i].role = CastRole.FeaturedSupport;
                }
                else if (i < 8)
                {
                    allMembers[i].role = CastRole.BackgroundSupport;
                }
                else
                {
                    allMembers[i].role = CastRole.Ambient;
                }
            }
            
            Debug.Log($"[MusicVideoCast] Auto-assigned roles for {allMembers.Count} members");
        }
        
        [ContextMenu("Find All Children as Cast")]
        private void FindAllChildrenAsCast()
        {
            ClearAllCast();
            
            int childCount = transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                var child = transform.GetChild(i);
                var member = new CastMember
                {
                    gameObject = child.gameObject,
                    role = i < 3 ? CastRole.FeaturedSupport : CastRole.BackgroundSupport
                };
                
                if (i < 3)
                {
                    mainCast.Add(member);
                }
                else
                {
                    supportingCast.Add(member);
                }
            }
            
            Debug.Log($"[MusicVideoCast] Found {childCount} children and organized into cast");
        }
        
        [ContextMenu("Remove Null Members")]
        private void RemoveNullMembers()
        {
            int beforeCount = mainCast.Count + supportingCast.Count + spotlightCast.Count;
            
            mainCast.RemoveAll(m => m == null || !m.IsValid());
            supportingCast.RemoveAll(m => m == null || !m.IsValid());
            spotlightCast.RemoveAll(m => m == null || !m.IsValid());
            
            int afterCount = mainCast.Count + supportingCast.Count + spotlightCast.Count;
            int removed = beforeCount - afterCount;
            
            Debug.Log($"[MusicVideoCast] Removed {removed} null/invalid members");
        }
        
        [ContextMenu("Print Cast Summary")]
        private void PrintCastSummary()
        {
            Debug.Log("=== MUSIC VIDEO CAST SUMMARY ===");
            Debug.Log($"Timeline: {(playableDirector != null ? "✓" : "✗")}");
            Debug.Log($"Audio: {(audioSource != null && audioSource.clip != null ? audioSource.clip.name : "None")}");
            Debug.Log($"\nMain Cast ({mainCast.Count}):");
            foreach (var m in mainCast)
            {
                if (m != null && m.IsValid())
                {
                    Debug.Log($"  - {m.GetDisplayName()}");
                }
            }
            Debug.Log($"\nSupporting Cast ({supportingCast.Count}):");
            foreach (var m in supportingCast)
            {
                if (m != null && m.IsValid())
                {
                    Debug.Log($"  - {m.GetDisplayName()}");
                }
            }
            Debug.Log($"\nSpotlight Cast ({spotlightCast.Count}):");
            foreach (var m in spotlightCast)
            {
                if (m != null && m.IsValid())
                {
                    Debug.Log($"  - {m.GetDisplayName()}");
                }
            }
        }
        
        [ContextMenu("Auto-Populate From LODGroups")]
        private void AutoPopulateFromLODGroups()
        {
            // Clear all cast members
            mainCast.Clear();
            supportingCast.Clear();
            spotlightCast.Clear();
            
            // Find all LODGroup components in the scene
            LODGroup[] lodGroups = FindObjectsOfType<LODGroup>();
            
            if (lodGroups.Length == 0)
            {
                Debug.LogWarning("[MusicVideoCast] No LODGroup components found in the scene");
                return;
            }
            
            // Shuffle the array for random assignment
            System.Random rng = new System.Random();
            lodGroups = lodGroups.OrderBy(x => rng.Next()).ToArray();
            
            // Distribute LODGroups across the three cast categories
            int totalGroups = lodGroups.Length;
            int mainCastCount = Mathf.CeilToInt(totalGroups / 3f);
            int supportingCastCount = Mathf.CeilToInt(totalGroups / 3f);
            // Remaining go to spotlight cast
            
            int index = 0;
            
            // Assign to main cast
            for (int i = 0; i < mainCastCount && index < totalGroups; i++, index++)
            {
                mainCast.Add(new CastMember
                {
                    gameObject = lodGroups[index].gameObject,
                    role = CastRole.Ambient
                });
            }
            
            // Assign to supporting cast
            for (int i = 0; i < supportingCastCount && index < totalGroups; i++, index++)
            {
                supportingCast.Add(new CastMember
                {
                    gameObject = lodGroups[index].gameObject,
                    role = CastRole.Ambient
                });
            }
            
            // Assign remaining to spotlight cast
            while (index < totalGroups)
            {
                spotlightCast.Add(new CastMember
                {
                    gameObject = lodGroups[index].gameObject,
                    role = CastRole.Ambient
                });
                index++;
            }
            
            Debug.Log($"[MusicVideoCast] Auto-populated from {totalGroups} LODGroups: {mainCast.Count} main, {supportingCast.Count} supporting, {spotlightCast.Count} spotlight - all with Ambient role");
        }
        #endregion
    }
    
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