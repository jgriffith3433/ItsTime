using UnityEngine;
using UnityEditor;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using System.Linq;

namespace MusicVideoGenerator
{
    /// <summary>
    /// Utility methods for working with Unity Timeline.
    /// </summary>
    public static class TimelineUtilities
    {
        private const string GENERATED_CLIPS_FOLDER = "generated_clips";
        
        /// <summary>
        /// Get the folder path containing the timeline asset.
        /// </summary>
        public static string GetTimelineFolder(TimelineAsset timeline)
        {
            string timelinePath = AssetDatabase.GetAssetPath(timeline);
            return System.IO.Path.GetDirectoryName(timelinePath).Replace("\\", "/");
        }
        
        /// <summary>
        /// Ensure the generated clips folder exists and return its path.
        /// </summary>
        public static string EnsureClipsFolderExists(string timelineFolder)
        {
            string clipsFolder = $"{timelineFolder}/{GENERATED_CLIPS_FOLDER}";
            
            if (!System.IO.Directory.Exists(clipsFolder))
            {
                System.IO.Directory.CreateDirectory(clipsFolder);
                AssetDatabase.Refresh();
            }
            
            return clipsFolder;
        }
        
        /// <summary>
        /// Save an animation clip to the generated clips folder.
        /// </summary>
        public static string SaveAnimationClip(AnimationClip clip, string clipsFolder)
        {
            string clipPath = AssetDatabase.GenerateUniqueAssetPath($"{clipsFolder}/{clip.name}.anim");
            AssetDatabase.CreateAsset(clip, clipPath);
            return clipPath;
        }
        
        /// <summary>
        /// Add an audio track to the timeline.
        /// </summary>
        public static AudioTrack AddAudioTrack(TimelineAsset timeline, PlayableDirector director, AudioSource audioSource)
        {
            if (audioSource == null || audioSource.clip == null) return null;
            
            var audioTrack = timeline.CreateTrack<AudioTrack>(null, "Audio");
            director.SetGenericBinding(audioTrack, audioSource);
            
            var audioClip = audioTrack.CreateClip(audioSource.clip);
            audioClip.displayName = audioSource.clip.name;
            audioClip.start = 0;
            audioClip.duration = audioSource.clip.length;
            
            return audioTrack;
        }
        
        /// <summary>
        /// Clear all tracks and generated clips from a timeline.
        /// </summary>
        public static void ClearTimeline(PlayableDirector director)
        {
            if (director == null || director.playableAsset == null) return;
            
            var timeline = director.playableAsset as TimelineAsset;
            if (timeline == null) return;
            
            string timelineFolder = GetTimelineFolder(timeline);
            string clipsFolder = $"{timelineFolder}/{GENERATED_CLIPS_FOLDER}";
            
            // Delete all tracks
            var tracks = timeline.GetOutputTracks().ToList();
            foreach (var track in tracks)
            {
                timeline.DeleteTrack(track);
            }
            
            // Delete generated clips
            if (System.IO.Directory.Exists(clipsFolder))
            {
                string[] clipFiles = System.IO.Directory.GetFiles(clipsFolder, "*.anim");
                foreach (string clipFile in clipFiles)
                {
                    AssetDatabase.DeleteAsset(clipFile);
                }
                
                // Remove folder if empty
                if (System.IO.Directory.GetFiles(clipsFolder).Length == 0)
                {
                    System.IO.Directory.Delete(clipsFolder);
                }
            }
            
            // Save and refresh
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.SetDirty(timeline);
            director.RebuildGraph();
            
            Debug.Log("✅ Timeline cleared");
        }
        
        /// <summary>
        /// Get the duration of the timeline based on audio or track lengths.
        /// </summary>
        public static double GetTimelineDuration(TimelineAsset timeline)
        {
            double duration = 0;
            
            foreach (var track in timeline.GetOutputTracks())
            {
                foreach (var clip in track.GetClips())
                {
                    double clipEnd = clip.start + clip.duration;
                    if (clipEnd > duration)
                    {
                        duration = clipEnd;
                    }
                }
            }
            
            return duration;
        }
        
        /// <summary>
        /// Validate that a timeline is ready for generation.
        /// </summary>
        public static bool ValidateTimeline(PlayableDirector director, out string errorMessage)
        {
            if (director == null)
            {
                errorMessage = "No PlayableDirector";
                return false;
            }
            
            if (director.playableAsset == null)
            {
                errorMessage = "PlayableDirector has no Timeline asset";
                return false;
            }
            
            if (!(director.playableAsset is TimelineAsset))
            {
                errorMessage = "PlayableAsset is not a Timeline";
                return false;
            }
            
            errorMessage = string.Empty;
            return true;
        }
    }
}