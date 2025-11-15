using UnityEngine;
using UnityEditor;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

namespace MusicVideoGenerator
{
    /// <summary>
    /// Main generator - orchestrates the entire music video creation process.
    /// </summary>
    public class MusicVideoGenerator
    {
        private const string GENERATED_CLIPS_FOLDER = "generated_clips";
        
        /// <summary>
        /// Generate a complete music video from cast and settings.
        /// </summary>
        public void Generate(MusicVideoCast cast, MusicVideoSettings settings)
        {
            // Validate
            var validation = cast.Validate();
            if (!validation.IsValid)
            {
                throw new System.Exception("Cast validation failed: " + validation.GetSummary());
            }
            
            if (!settings.HasAnyMusicData())
            {
                Debug.LogWarning("No music analysis data - animations will be basic");
            }
            
            // Setup
            var timeline = cast.playableDirector.playableAsset as TimelineAsset;
            if (timeline == null)
            {
                throw new System.Exception("PlayableDirector has no Timeline asset");
            }
            
            ConfigureTimeline(timeline, cast.playableDirector);
            
            var random = new System.Random(settings.randomSeed);
            var allCast = cast.GetAllCastMembers();
            
            if (settings.showDebugInfo)
            {
                Debug.Log($"🎬 Generating music video with seed {settings.randomSeed}");
                Debug.Log($"📋 Cast: {allCast.Count} members");
            }
            
            // Parse music data
            float songDuration = cast.GetSongDuration();
            var analysisData = MusicAnalyzer.ParseAubioData(
                settings.beatsFile,
                settings.onsetsFile,
                settings.notesFile,
                settings.pitchFile,
                settings.silenceFile,
                songDuration
            );
            
            if (settings.showDebugInfo)
            {
                Debug.Log($"🎵 Song duration: {songDuration:F2}s");
                Debug.Log($"📊 Beats: {analysisData.beatTimes.Count}, Onsets: {analysisData.onsetTimes.Count}");
            }
            
            // Add audio track
            if (cast.audioSource != null && cast.audioSource.clip != null)
            {
                TimelineUtilities.AddAudioTrack(timeline, cast.playableDirector, cast.audioSource);
                
                if (settings.showDebugInfo)
                {
                    Debug.Log($"🔊 Audio: {cast.audioSource.clip.name}");
                }
            }
            
            // Analyze music structure
            var musicSections = MusicAnalyzer.AnalyzeMusicStructure(
                analysisData,
                settings.musicAnalysisWindowSize,
                settings.intensityThreshold,
                settings.energyThreshold
            );
            
            if (settings.showDebugInfo)
            {
                Debug.Log($"🎼 Music sections: {musicSections.Count}");
            }
            
            // Generate story beats
            var storyBeats = StoryGenerator.GenerateStoryBeats(
                musicSections,
                allCast,
                settings.storyIntensity,
                random
            );
            
            if (settings.showDebugInfo)
            {
                Debug.Log($"📖 Story beats: {storyBeats.Count}");
            }
            
            // Generate animations for each cast member
            string timelineFolder = TimelineUtilities.GetTimelineFolder(timeline);
            string clipsFolder = TimelineUtilities.EnsureClipsFolderExists(timelineFolder);
            
            int trackCount = 0;
            for (int i = 0; i < allCast.Count; i++)
            {
                var member = allCast[i];
                
                if (settings.showDebugInfo)
                {
                    Debug.Log($"  🎭 Generating: {member.GetDisplayName()}");
                }
                
                // Ensure animator component
                var animator = member.gameObject.GetComponent<Animator>();
                if (animator == null)
                {
                    animator = member.gameObject.AddComponent<Animator>();
                }
                
                // Generate animation clip
                var clip = AnimationGenerator.GenerateAnimationClip(
                    member,
                    i,
                    storyBeats,
                    musicSections,
                    analysisData,
                    settings,
                    random
                );
                
                // Save clip
                string clipPath = TimelineUtilities.SaveAnimationClip(clip, clipsFolder);
                
                // Create timeline track
                var track = timeline.CreateTrack<AnimationTrack>(null, member.gameObject.name);
                cast.playableDirector.SetGenericBinding(track, member.gameObject);
                
                var timelineClip = track.CreateClip(clip);
                timelineClip.displayName = clip.name;
                timelineClip.duration = clip.length;
                
                // Apply animation delay
                if (member.animationDelay > 0f)
                {
                    timelineClip.start = member.animationDelay;
                }
                
                trackCount++;
            }
            
            // Finalize
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.SetDirty(timeline);
            EditorUtility.SetDirty(cast.playableDirector);
            EditorSceneManager.MarkSceneDirty(cast.playableDirector.gameObject.scene);
            cast.playableDirector.RebuildGraph();
            
            Debug.Log($"✨ Music video complete! {trackCount} tracks, {storyBeats.Count} story beats");
        }
        
        private void ConfigureTimeline(TimelineAsset timeline, PlayableDirector director)
        {
            timeline.editorSettings.frameRate = 60;
            director.timeUpdateMode = DirectorUpdateMode.DSPClock;
        }
    }
}