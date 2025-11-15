using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace MusicVideoGenerator
{
    /// <summary>
    /// Analyzes music data and classifies it into sections and story moments.
    /// </summary>
    public static class MusicAnalyzer
    {
        /// <summary>
        /// Parse Aubio data files and create analysis data.
        /// </summary>
        public static MusicAnalysisData ParseAubioData(
            TextAsset beatsFile,
            TextAsset onsetsFile,
            TextAsset notesFile,
            TextAsset pitchFile,
            TextAsset silenceFile,
            float audioClipLength)
        {
            var data = new MusicAnalysisData();
            
            if (beatsFile != null) ParseBeats(beatsFile.text, data);
            if (onsetsFile != null) ParseOnsets(onsetsFile.text, data);
            if (notesFile != null) ParseNotes(notesFile.text, data);
            if (pitchFile != null) ParsePitch(pitchFile.text, data);
            if (silenceFile != null) ParseSilence(silenceFile.text, data);
            
            // Determine max time
            data.maxTime = audioClipLength > 0 ? audioClipLength : CalculateMaxTime(data);
            
            return data;
        }
        
        /// <summary>
        /// Analyze music structure and classify into sections.
        /// </summary>
        public static List<MusicSectionData> AnalyzeMusicStructure(
            MusicAnalysisData data,
            float windowSize,
            float intensityThreshold,
            float energyThreshold)
        {
            var sections = new List<MusicSectionData>();
            
            if (!data.HasData) return sections;
            
            List<float> intensities = new List<float>();
            List<float> energies = new List<float>();
            List<float> windowTimes = new List<float>();
            
            // Calculate intensity and energy for each window
            for (float t = 0; t < data.maxTime; t += windowSize)
            {
                float intensity = CalculateIntensity(data, t, t + windowSize);
                float energy = CalculateEnergy(data, t, t + windowSize);
                intensities.Add(intensity);
                energies.Add(energy);
                windowTimes.Add(t);
            }
            
            float avgIntensity = intensities.Count > 0 ? intensities.Average() : 0f;
            float maxIntensity = intensities.Count > 0 ? intensities.Max() : 1f;
            
            // Classify each section
            for (int i = 0; i < intensities.Count; i++)
            {
                float startTime = windowTimes[i];
                float endTime = Mathf.Min(startTime + windowSize, data.maxTime);
                float intensity = intensities[i];
                float energy = energies[i];
                float normalizedIntensity = maxIntensity > 0 ? intensity / maxIntensity : 0f;
                
                MusicSection section = ClassifySection(
                    i,
                    intensities,
                    avgIntensity,
                    intensities.Count,
                    intensityThreshold
                );
                
                sections.Add(new MusicSectionData
                {
                    section = section,
                    startTime = startTime,
                    endTime = endTime,
                    intensity = normalizedIntensity,
                    energy = energy
                });
            }
            
            return sections;
        }
        
        private static void ParseBeats(string text, MusicAnalysisData data)
        {
            foreach (string line in text.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries))
            {
                if (float.TryParse(line.Trim(), out float time))
                {
                    data.beatTimes.Add(time);
                }
            }
            data.beatTimes.Sort();
        }
        
        private static void ParseOnsets(string text, MusicAnalysisData data)
        {
            foreach (string line in text.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries))
            {
                if (float.TryParse(line.Trim(), out float time))
                {
                    data.onsetTimes.Add(time);
                }
            }
            data.onsetTimes.Sort();
        }
        
        private static void ParseNotes(string text, MusicAnalysisData data)
        {
            foreach (string line in text.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries))
            {
                string[] parts = line.Split(new[] { ' ', '\t' }, System.StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 3 &&
                    float.TryParse(parts[0], out float midiNote) &&
                    float.TryParse(parts[1], out float startTime) &&
                    float.TryParse(parts[2], out float duration))
                {
                    data.notes.Add(new NoteEvent
                    {
                        midiNote = (int)midiNote,
                        startTime = startTime,
                        duration = duration
                    });
                }
            }
        }
        
        private static void ParsePitch(string text, MusicAnalysisData data)
        {
            foreach (string line in text.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries))
            {
                string[] parts = line.Split(new[] { ' ', '\t' }, System.StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2 &&
                    float.TryParse(parts[0], out float time) &&
                    float.TryParse(parts[1], out float frequency))
                {
                    data.pitchData.Add(new PitchEvent { time = time, frequency = frequency });
                }
            }
        }
        
        private static void ParseSilence(string text, MusicAnalysisData data)
        {
            foreach (string line in text.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries))
            {
                if (line.Contains("QUIET:") || line.Contains("NOISY:"))
                {
                    bool isQuiet = line.Contains("QUIET:");
                    string[] parts = line.Split(':');
                    if (parts.Length >= 2 && float.TryParse(parts[1].Trim(), out float time))
                    {
                        data.silenceEvents.Add(new SilenceEvent { time = time, isQuiet = isQuiet });
                    }
                }
            }
        }
        
        private static float CalculateMaxTime(MusicAnalysisData data)
        {
            float max = 0f;
            if (data.beatTimes.Count > 0) max = Mathf.Max(max, data.beatTimes[data.beatTimes.Count - 1]);
            if (data.onsetTimes.Count > 0) max = Mathf.Max(max, data.onsetTimes[data.onsetTimes.Count - 1]);
            if (data.notes.Count > 0) max = Mathf.Max(max, data.notes.Max(n => n.startTime));
            if (data.pitchData.Count > 0) max = Mathf.Max(max, data.pitchData[data.pitchData.Count - 1].time);
            if (data.silenceEvents.Count > 0) max = Mathf.Max(max, data.silenceEvents[data.silenceEvents.Count - 1].time);
            return max;
        }
        
        private static float CalculateIntensity(MusicAnalysisData data, float startTime, float endTime)
        {
            int onsetCount = data.onsetTimes.Count(t => t >= startTime && t < endTime);
            int beatCount = data.beatTimes.Count(t => t >= startTime && t < endTime);
            
            var pitchesInWindow = data.pitchData.Where(p => p.time >= startTime && p.time < endTime && p.frequency > 0).ToList();
            float pitchVariance = 0f;
            
            if (pitchesInWindow.Count > 1)
            {
                float avgPitch = pitchesInWindow.Average(p => p.frequency);
                pitchVariance = pitchesInWindow.Sum(p => Mathf.Abs(p.frequency - avgPitch)) / pitchesInWindow.Count;
            }
            
            return onsetCount * 2f + beatCount * 1f + pitchVariance * 0.01f;
        }
        
        private static float CalculateEnergy(MusicAnalysisData data, float startTime, float endTime)
        {
            int noteCount = data.notes.Count(n => n.startTime >= startTime && n.startTime < endTime);
            var pitchesInWindow = data.pitchData.Where(p => p.time >= startTime && p.time < endTime && p.frequency > 0).ToList();
            float avgPitch = pitchesInWindow.Count > 0 ? pitchesInWindow.Average(p => p.frequency) : 0f;
            
            return noteCount * 0.5f + avgPitch * 0.001f;
        }
        
        private static MusicSection ClassifySection(
            int index,
            List<float> intensities,
            float avgIntensity,
            int totalSections,
            float threshold)
        {
            if (totalSections == 0) return MusicSection.Verse;
            
            float intensity = intensities[index];
            float relativePosition = (float)index / totalSections;
            
            // Intro at start
            if (relativePosition < 0.1f && intensity < avgIntensity * threshold)
            {
                return MusicSection.Intro;
            }
            
            // Outro at end
            if (relativePosition > 0.85f)
            {
                return MusicSection.Outro;
            }
            
            // Climax for high intensity
            if (intensity > avgIntensity * 1.5f * threshold)
            {
                return MusicSection.Climax;
            }
            
            // Buildup when increasing
            if (index > 0 && intensities[index] > intensities[index - 1] * 1.2f)
            {
                return MusicSection.Buildup;
            }
            
            // Breakdown when decreasing
            if (index > 0 && intensities[index] < intensities[index - 1] * 0.6f)
            {
                return MusicSection.Breakdown;
            }
            
            // Chorus for moderate-high intensity
            if (intensity > avgIntensity * 1.2f * threshold)
            {
                return MusicSection.Chorus;
            }
            
            // Bridge for dramatic changes
            if (index > 0 && Mathf.Abs(intensities[index] - intensities[index - 1]) > avgIntensity * 0.3f)
            {
                return MusicSection.Bridge;
            }
            
            // Default to verse
            return MusicSection.Verse;
        }
    }
}