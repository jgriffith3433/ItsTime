using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace MusicVideoGenerator
{
    /// <summary>
    /// Generates story beats and assigns cast focus based on music structure.
    /// </summary>
    public static class StoryGenerator
    {
        /// <summary>
        /// Generate story beats from music sections and cast configuration.
        /// </summary>
        public static List<StoryBeat> GenerateStoryBeats(
            List<MusicSectionData> musicSections,
            List<CastMember> cast,
            float storyIntensity,
            System.Random random)
        {
            var storyBeats = new List<StoryBeat>();
            
            foreach (var section in musicSections)
            {
                StoryMoment moment = MapSectionToStoryMoment(section.section);
                List<int> focusedCast = SelectFocusedCast(cast, moment, section.intensity, random);
                
                storyBeats.Add(new StoryBeat
                {
                    time = section.startTime,
                    moment = moment,
                    intensity = section.intensity * storyIntensity,
                    focusedCastIndices = focusedCast,
                    description = $"{moment} - {section.section} (Intensity: {section.intensity:F2})"
                });
            }
            
            return storyBeats;
        }
        
        /// <summary>
        /// Map music section to story moment.
        /// </summary>
        private static StoryMoment MapSectionToStoryMoment(MusicSection section)
        {
            switch (section)
            {
                case MusicSection.Intro:
                    return StoryMoment.Introduction;
                
                case MusicSection.Verse:
                    return StoryMoment.Rising;
                
                case MusicSection.Bridge:
                case MusicSection.Buildup:
                    return StoryMoment.Conflict;
                
                case MusicSection.Climax:
                case MusicSection.Chorus:
                    return StoryMoment.Climax;
                
                case MusicSection.Breakdown:
                    return StoryMoment.Resolution;
                
                case MusicSection.Outro:
                    return StoryMoment.Finale;
                
                default:
                    return StoryMoment.Rising;
            }
        }
        
        /// <summary>
        /// Select which cast members should be in focus based on story moment and intensity.
        /// </summary>
        private static List<int> SelectFocusedCast(
            List<CastMember> cast,
            StoryMoment moment,
            float intensity,
            System.Random random)
        {
            var focusedIndices = new List<int>();
            
            // Different story moments favor different cast members
            switch (moment)
            {
                case StoryMoment.Introduction:
                    // Start with main cast
                    focusedIndices.AddRange(GetMainCastIndices(cast, random, 1, 2));
                    break;
                
                case StoryMoment.Rising:
                    // Build up - add more cast
                    focusedIndices.AddRange(GetMainCastIndices(cast, random, 1, 3));
                    if (intensity > 0.5f)
                    {
                        focusedIndices.AddRange(GetSupportingCastIndices(cast, random, 1, 2));
                    }
                    break;
                
                case StoryMoment.Conflict:
                    // Tension - mix of main and support
                    focusedIndices.AddRange(GetMainCastIndices(cast, random, 2, 3));
                    focusedIndices.AddRange(GetSupportingCastIndices(cast, random, 1, 3));
                    break;
                
                case StoryMoment.Climax:
                    // Peak - everyone!
                    focusedIndices.AddRange(GetMainCastIndices(cast, random, cast.Count, cast.Count));
                    focusedIndices.AddRange(GetSupportingCastIndices(cast, random, 2, 5));
                    focusedIndices.AddRange(GetSpotlightCastIndices(cast, random, 1, 2));
                    break;
                
                case StoryMoment.Resolution:
                    // Winding down - main cast
                    focusedIndices.AddRange(GetMainCastIndices(cast, random, 1, 2));
                    break;
                
                case StoryMoment.Finale:
                    // Grand ending - all main cast
                    focusedIndices.AddRange(GetMainCastIndices(cast, random, cast.Count, cast.Count));
                    if (intensity > 0.7f)
                    {
                        focusedIndices.AddRange(GetSpotlightCastIndices(cast, random, 1, 3));
                    }
                    break;
            }
            
            return focusedIndices.Distinct().ToList();
        }
        
        private static List<int> GetMainCastIndices(List<CastMember> cast, System.Random random, int min, int max)
        {
            var mainCast = cast
                .Select((member, index) => new { member, index })
                .Where(x => x.member.IsMainCast())
                .ToList();
            
            return SelectRandomIndices(mainCast.Select(x => x.index).ToList(), random, min, max);
        }
        
        private static List<int> GetSupportingCastIndices(List<CastMember> cast, System.Random random, int min, int max)
        {
            var supportingCast = cast
                .Select((member, index) => new { member, index })
                .Where(x => x.member.IsSupportingCast())
                .ToList();
            
            return SelectRandomIndices(supportingCast.Select(x => x.index).ToList(), random, min, max);
        }
        
        private static List<int> GetSpotlightCastIndices(List<CastMember> cast, System.Random random, int min, int max)
        {
            var spotlightCast = cast
                .Select((member, index) => new { member, index })
                .Where(x => x.member.role == CastRole.Spotlight)
                .ToList();
            
            return SelectRandomIndices(spotlightCast.Select(x => x.index).ToList(), random, min, max);
        }
        
        private static List<int> SelectRandomIndices(List<int> indices, System.Random random, int min, int max)
        {
            if (indices.Count == 0) return new List<int>();
            
            int count = Mathf.Clamp(random.Next(min, max + 1), 0, indices.Count);
            var shuffled = indices.OrderBy(x => random.Next()).Take(count).ToList();
            
            return shuffled;
        }
    }
}