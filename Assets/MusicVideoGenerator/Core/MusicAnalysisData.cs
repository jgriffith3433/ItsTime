using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace MusicVideoGenerator
{
    /// <summary>
    /// Container for all parsed music analysis data from Aubio files.
    /// </summary>
    public class MusicAnalysisData
    {
        public List<float> beatTimes = new List<float>();
        public List<float> onsetTimes = new List<float>();
        public List<NoteEvent> notes = new List<NoteEvent>();
        public List<PitchEvent> pitchData = new List<PitchEvent>();
        public List<SilenceEvent> silenceEvents = new List<SilenceEvent>();
        
        public float maxTime = 0f;
        
        public bool HasData => beatTimes.Count > 0 || onsetTimes.Count > 0 || 
                               notes.Count > 0 || pitchData.Count > 0;
        
        /// <summary>
        /// Check if there's a beat at the given time (within tolerance).
        /// </summary>
        public bool IsBeatAt(float time, float tolerance = 0.05f)
        {
            return beatTimes.Any(bt => Mathf.Abs(bt - time) < tolerance);
        }
        
        /// <summary>
        /// Check if there's an onset at the given time (within tolerance).
        /// </summary>
        public bool IsOnsetAt(float time, float tolerance = 0.05f)
        {
            return onsetTimes.Any(ot => Mathf.Abs(ot - time) < tolerance);
        }
        
        /// <summary>
        /// Get the pitch frequency at a specific time.
        /// </summary>
        public float GetPitchAt(float time)
        {
            if (pitchData.Count == 0) return 0f;
            
            var nearest = pitchData.OrderBy(p => Mathf.Abs(p.time - time)).FirstOrDefault();
            return nearest.frequency;
        }
        
        /// <summary>
        /// Check if the given time is during a quiet section.
        /// </summary>
        public bool IsQuietAt(float time)
        {
            if (silenceEvents.Count == 0) return false;
            
            var relevantEvents = silenceEvents.Where(s => s.time <= time).OrderByDescending(s => s.time);
            var mostRecent = relevantEvents.FirstOrDefault();
            
            return mostRecent.isQuiet;
        }
    }
    
    /// <summary>
    /// Represents a musical note event.
    /// </summary>
    [System.Serializable]
    public struct NoteEvent
    {
        public int midiNote;
        public float startTime;
        public float duration;
        
        public float EndTime => startTime + duration;
    }
    
    /// <summary>
    /// Represents a pitch detection event.
    /// </summary>
    [System.Serializable]
    public struct PitchEvent
    {
        public float time;
        public float frequency;
    }
    
    /// <summary>
    /// Represents a silence detection event.
    /// </summary>
    [System.Serializable]
    public struct SilenceEvent
    {
        public float time;
        public bool isQuiet;
    }
    
    /// <summary>
    /// Music section classification.
    /// </summary>
    public enum MusicSection
    {
        Intro,
        Verse,
        Chorus,
        Bridge,
        Breakdown,
        Buildup,
        Climax,
        Outro
    }
    
    /// <summary>
    /// Represents an analyzed section of music with metadata.
    /// </summary>
    [System.Serializable]
    public struct MusicSectionData
    {
        public MusicSection section;
        public float startTime;
        public float endTime;
        public float intensity;      // 0-1: how intense/loud
        public float energy;         // 0-1: how energetic
        
        public float Duration => endTime - startTime;
        public float MidPoint => (startTime + endTime) * 0.5f;
    }
    
    /// <summary>
    /// Story moment classification for narrative structure.
    /// </summary>
    public enum StoryMoment
    {
        Introduction,   // Opening, setting the scene
        Rising,         // Building tension/interest
        Conflict,       // Tension, drama
        Climax,         // Peak moment
        Resolution,     // Winding down
        Finale          // Grand ending
    }
    
    /// <summary>
    /// Represents a story beat in the narrative.
    /// </summary>
    [System.Serializable]
    public struct StoryBeat
    {
        public float time;
        public StoryMoment moment;
        public float intensity;
        public List<int> focusedCastIndices;  // Which cast members are in focus
        public string description;
        
        public bool IsCastMemberFocused(int castIndex)
        {
            return focusedCastIndices != null && focusedCastIndices.Contains(castIndex);
        }
    }
}