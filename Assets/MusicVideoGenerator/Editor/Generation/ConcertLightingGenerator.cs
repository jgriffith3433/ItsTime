using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace MusicVideoGenerator
{
    /// <summary>
    /// Generates concert-style lighting including spotlights, lasers, and ambient lighting changes
    /// </summary>
    public static class ConcertLightingGenerator
    {
        public static void GenerateConcertLighting(
            MusicVideoCast cast,
            TimelineAsset timeline,
            PlayableDirector director,
            List<StoryBeat> storyBeats,
            List<MusicSectionData> musicSections,
            MusicAnalysisData analysisData,
            MusicVideoSettings settings,
            System.Random random,
            string clipsFolder)
        {
            // Create lighting parent
            GameObject lightingRoot = new GameObject("Concert_Lighting");
            lightingRoot.transform.SetParent(cast.transform);
            lightingRoot.transform.localPosition = Vector3.zero;
            
            // Generate different types of lights and get their clips
            var spotlights = GenerateSpotlights(lightingRoot.transform, cast, storyBeats, musicSections, analysisData, settings, random);
            var lasers = GenerateLasers(lightingRoot.transform, cast, storyBeats, musicSections, analysisData, settings, random);
            var ambient = GenerateAmbientLighting(lightingRoot.transform, musicSections, analysisData, settings);
            var followSpots = GenerateFollowSpots(lightingRoot.transform, cast, storyBeats, settings, random);
            
            // Animate all lights
            AnimateSpotlights(spotlights, timeline, director, storyBeats, musicSections, analysisData, settings, random, clipsFolder);
            AnimateLasers(lasers, timeline, director, storyBeats, musicSections, analysisData, settings, random, clipsFolder);
            AnimateAmbientLighting(ambient, timeline, director, musicSections, analysisData, settings, clipsFolder);
            AnimateFollowSpots(followSpots, timeline, director, cast, storyBeats, analysisData, settings, clipsFolder);
        }
        
        private static List<Light> GenerateSpotlights(Transform parent, MusicVideoCast cast, List<StoryBeat> storyBeats,
            List<MusicSectionData> musicSections, MusicAnalysisData analysisData, MusicVideoSettings settings, System.Random random)
        {
            int spotlightCount = 6;
            var lights = new List<Light>();
            
            for (int i = 0; i < spotlightCount; i++)
            {
                GameObject spotGO = new GameObject($"Spotlight_{i}");
                spotGO.transform.SetParent(parent);
                
                // Position spotlights in a circle above the scene using stage bounds
                float angle = (i / (float)spotlightCount) * Mathf.PI * 2f;
                float radius = Mathf.Max(settings.stageBounds.x, settings.stageBounds.z) * 0.8f; // 80% of stage bounds
                spotGO.transform.localPosition = settings.stageCenter + new Vector3(
                    Mathf.Cos(angle) * radius,
                    settings.stageBounds.y * 0.75f,
                    Mathf.Sin(angle) * radius
                );
                
                Light light = spotGO.AddComponent<Light>();
                light.type = LightType.Spot;
                light.range = Mathf.Max(settings.stageBounds.x, settings.stageBounds.y, settings.stageBounds.z) * 1.5f;
                light.spotAngle = 45f;
                light.intensity = 0f; // Start off, animate it
                light.color = GetConcertColor(i, random);
                light.shadows = LightShadows.Soft;
                
                // Add subtle visual sphere
                GameObject visualSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                visualSphere.name = "Visual";
                visualSphere.transform.SetParent(spotGO.transform);
                visualSphere.transform.localPosition = Vector3.zero;
                visualSphere.transform.localScale = Vector3.one * 0.15f; // Smaller visual
                
                Material mat = new Material(Shader.Find("Standard"));
                mat.SetFloat("_Mode", 3);
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = 3000;
                
                Color translucentColor = light.color;
                translucentColor.a = 0.05f; // Nearly invisible
                mat.color = translucentColor;
                mat.SetFloat("_Metallic", 0f);
                mat.SetFloat("_Glossiness", 0.5f);
                visualSphere.GetComponent<Renderer>().material = mat;
                
                // Point toward stage center
                spotGO.transform.LookAt(settings.stageCenter);
                
                lights.Add(light);
            }
            
            return lights;
        }
        
        private static List<Light> GenerateLasers(Transform parent, MusicVideoCast cast, List<StoryBeat> storyBeats,
            List<MusicSectionData> musicSections, MusicAnalysisData analysisData, MusicVideoSettings settings, System.Random random)
        {
            int laserCount = 8;
            var lights = new List<Light>();
            
            for (int i = 0; i < laserCount; i++)
            {
                GameObject laserGO = new GameObject($"Laser_{i}");
                laserGO.transform.SetParent(parent);
                
                // Position lasers around the edges using stage bounds
                float angle = (i / (float)laserCount) * Mathf.PI * 2f;
                float radius = Mathf.Max(settings.stageBounds.x, settings.stageBounds.z) * 0.65f;
                laserGO.transform.localPosition = settings.stageCenter + new Vector3(
                    Mathf.Cos(angle) * radius,
                    settings.stageBounds.y * 0.25f + (float)random.NextDouble() * settings.stageBounds.y * 0.5f,
                    Mathf.Sin(angle) * radius
                );
                
                Light light = laserGO.AddComponent<Light>();
                light.type = LightType.Spot;
                light.range = 15f; // Longer range
                light.spotAngle = 5f; // Slightly wider beam
                light.intensity = 0f;
                light.color = GetLaserColor(random);
                light.shadows = LightShadows.None; // No shadows for performance
                light.renderMode = LightRenderMode.ForceVertex; // Simple lighting mode
                
                // Add visual laser beam (cylinder)
                GameObject beamVisual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                beamVisual.name = "LaserBeam";
                beamVisual.transform.SetParent(laserGO.transform);
                beamVisual.transform.localPosition = new Vector3(0, 0, 7.5f); // Extend forward
                beamVisual.transform.localRotation = Quaternion.Euler(90, 0, 0);
                beamVisual.transform.localScale = new Vector3(0.08f, 7.5f, 0.08f); // Thicker, longer beam
                
                // Glowing beam material
                Material beamMat = new Material(Shader.Find("Standard"));
                beamMat.SetFloat("_Mode", 3);
                beamMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                beamMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One); // Additive for glow
                beamMat.SetInt("_ZWrite", 0);
                beamMat.EnableKeyword("_ALPHABLEND_ON");
                beamMat.renderQueue = 3000;
                
                Color beamColor = light.color;
                beamColor.a = 0.6f;
                beamMat.color = beamColor;
                beamMat.EnableKeyword("_EMISSION");
                beamMat.SetColor("_EmissionColor", light.color * 2f);
                beamVisual.GetComponent<Renderer>().material = beamMat;
                
                // Random direction
                laserGO.transform.rotation = Quaternion.Euler(
                    (float)random.NextDouble() * 180f - 90f,
                    (float)random.NextDouble() * 360f,
                    0
                );
                
                lights.Add(light);
            }
            
            return lights;
        }
        
        private static Light GenerateAmbientLighting(Transform parent, List<MusicSectionData> musicSections,
            MusicAnalysisData analysisData, MusicVideoSettings settings)
        {
            GameObject ambientGO = new GameObject("Ambient_Fill");
            ambientGO.transform.SetParent(parent);
            ambientGO.transform.localPosition = settings.stageCenter + new Vector3(0, settings.stageBounds.y * 0.5f, 0);
            
            Light light = ambientGO.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = Mathf.Max(settings.stageBounds.x, settings.stageBounds.y, settings.stageBounds.z) * 2.5f;
            light.intensity = 2f; // Base ambient light
            light.color = new Color(0.4f, 0.4f, 0.6f); // Cool blue ambient
            
            return light;
        }
        
        private static List<Light> GenerateFollowSpots(Transform parent, MusicVideoCast cast, List<StoryBeat> storyBeats,
            MusicVideoSettings settings, System.Random random)
        {
            var lights = new List<Light>();
            
            // Create 2 follow spots that track main cast members
            for (int i = 0; i < 2; i++)
            {
                GameObject followGO = new GameObject($"FollowSpot_{i}");
                followGO.transform.SetParent(parent);
                float xOffset = (i == 0 ? -1f : 1f) * settings.stageBounds.x * 0.375f;
                followGO.transform.localPosition = settings.stageCenter + new Vector3(
                    xOffset,
                    settings.stageBounds.y * 0.6f,
                    -settings.stageBounds.z * 0.25f
                );
                
                Light light = followGO.AddComponent<Light>();
                light.type = LightType.Spot;
                light.range = Mathf.Max(settings.stageBounds.x, settings.stageBounds.z) * 2f;
                light.spotAngle = 30f;
                light.intensity = 5f;
                light.color = Color.white;
                light.shadows = LightShadows.Soft;
                
                lights.Add(light);
            }
            
            return lights;
        }
        
        private static Color GetConcertColor(int index, System.Random random)
        {
            Color[] concertColors = new Color[]
            {
                new Color(1f, 0.2f, 0.2f),    // Red
                new Color(0.2f, 0.2f, 1f),    // Blue
                new Color(1f, 0.2f, 1f),      // Magenta
                new Color(0.2f, 1f, 1f),      // Cyan
                new Color(1f, 1f, 0.2f),      // Yellow
                new Color(1f, 0.5f, 0.2f),    // Orange
                new Color(0.5f, 0.2f, 1f),    // Purple
                Color.white
            };
            
            return concertColors[index % concertColors.Length];
        }
        
        private static Color GetLaserColor(System.Random random)
        {
            Color[] laserColors = new Color[]
            {
                new Color(0f, 1f, 0f),        // Green laser
                new Color(1f, 0f, 0f),        // Red laser
                new Color(0f, 0.5f, 1f),      // Blue laser
                new Color(1f, 0f, 1f),        // Magenta laser
                new Color(0f, 1f, 1f)         // Cyan laser
            };
            
            return laserColors[random.Next(laserColors.Length)];
        }
        
        // Animation methods
        private static void AnimateSpotlights(List<Light> spotlights, TimelineAsset timeline, PlayableDirector director, List<StoryBeat> storyBeats,
            List<MusicSectionData> musicSections, MusicAnalysisData analysisData, MusicVideoSettings settings, System.Random random, string clipsFolder)
        {
            float timeStep = settings.keyframeInterval;
            
            for (int i = 0; i < spotlights.Count; i++)
            {
                Light light = spotlights[i];
                
                // Add Animator component
                var animator = light.gameObject.GetComponent<Animator>();
                if (animator == null)
                {
                    animator = light.gameObject.AddComponent<Animator>();
                }
                
                AnimationClip clip = new AnimationClip { name = $"{light.gameObject.name}_Anim" };
                
                var intensityCurve = new AnimationCurve();
                var colorR = new AnimationCurve();
                var colorG = new AnimationCurve();
                var colorB = new AnimationCurve();
                
                // Each spotlight has a phase offset for variety
                float phaseOffset = i * 0.16f;
                
                for (float t = 0; t <= analysisData.maxTime; t += timeStep)
                {
                    var section = musicSections.FirstOrDefault(s => t >= s.startTime && t < s.endTime);
                    float intensity = section.intensity > 0 ? section.intensity : 0.5f;
                    
                    // Spotlights pulse on beats with lower intensity
                    float beatPulse = 0f;
                    if (analysisData.IsBeatAt(t + phaseOffset, 0.1f))
                    {
                        beatPulse = intensity * 2f; // Much lower intensity
                    }
                    else
                    {
                        // Fade out between beats
                        beatPulse = Mathf.Lerp(beatPulse, 0f, 0.95f);
                    }
                    
                    // Base intensity varies with music sections
                    float baseIntensity = intensity * 3f;
                    intensityCurve.AddKey(t, baseIntensity + beatPulse);
                    
                    // Color shifts slightly with intensity
                    Color currentColor = light.color * (0.8f + intensity * 0.4f);
                    colorR.AddKey(t, currentColor.r);
                    colorG.AddKey(t, currentColor.g);
                    colorB.AddKey(t, currentColor.b);
                }
                
                clip.SetCurve("", typeof(Light), "m_Intensity", intensityCurve);
                clip.SetCurve("", typeof(Light), "m_Color.r", colorR);
                clip.SetCurve("", typeof(Light), "m_Color.g", colorG);
                clip.SetCurve("", typeof(Light), "m_Color.b", colorB);
                
                // Save and add to timeline
                string clipPath = TimelineUtilities.SaveAnimationClip(clip, clipsFolder);
                
                var track = timeline.CreateTrack<AnimationTrack>(null, light.gameObject.name);
                director.SetGenericBinding(track, light.gameObject);
                var timelineClip = track.CreateClip(clip);
                timelineClip.displayName = clip.name;
                timelineClip.duration = clip.length;
            }
        }
        
        private static void AnimateLasers(List<Light> lasers, TimelineAsset timeline, PlayableDirector director, List<StoryBeat> storyBeats,
            List<MusicSectionData> musicSections, MusicAnalysisData analysisData, MusicVideoSettings settings, System.Random random, string clipsFolder)
        {
            float timeStep = settings.keyframeInterval;
            
            for (int i = 0; i < lasers.Count; i++)
            {
                Light laser = lasers[i];
                
                // Add Animator component
                var animator = laser.gameObject.GetComponent<Animator>();
                if (animator == null)
                {
                    animator = laser.gameObject.AddComponent<Animator>();
                }
                
                AnimationClip clip = new AnimationClip { name = $"{laser.gameObject.name}_Anim" };
                
                var intensityCurve = new AnimationCurve();
                var rotX = new AnimationCurve();
                var rotY = new AnimationCurve();
                var rotZ = new AnimationCurve();
                var rotW = new AnimationCurve();
                var colorR = new AnimationCurve();
                var colorG = new AnimationCurve();
                var colorB = new AnimationCurve();
                
                // Each laser has a personality - some are anxious, some are calm
                float laserPersonality = (float)random.NextDouble(); // 0=calm, 1=erratic
                float baseSweepSpeed = 0.3f + laserPersonality * 0.7f;
                Quaternion initialRot = laser.transform.localRotation;
                
                StoryBeat previousBeat = storyBeats[0];
                
                for (float t = 0; t <= analysisData.maxTime; t += timeStep)
                {
                    var section = musicSections.FirstOrDefault(s => t >= s.startTime && t < s.endTime);
                    var beat = storyBeats.FirstOrDefault(b => t >= b.time);
                    if (beat.time == 0) beat = storyBeats[0];
                    
                    float intensity = section.intensity > 0 ? section.intensity : 0.5f;
                    float timeSinceBeatStart = t - beat.time;
                    
                    // EMOTIONAL LASER BEHAVIOR - tells a story through light and movement
                    float emotionalIntensity = 0f;
                    float emotionalSpeed = 1f;
                    float emotionalRange = 1f; // How wide the sweep
                    Color emotionalColor = Color.white;
                    bool flickering = false;
                    
                    switch (beat.moment)
                    {
                        case StoryMoment.Introduction:
                            // "Awakening... single beams of light emerge from darkness"
                            float introFade = Mathf.Min(1f, timeSinceBeatStart / 6f);
                            emotionalIntensity = Mathf.Lerp(0.01f, 0.4f, Mathf.SmoothStep(0, 1, introFade));
                            emotionalSpeed = 0.3f; // Very slow, contemplative
                            emotionalRange = Mathf.Lerp(0f, 30f, introFade); // Start narrow, slowly widen
                            emotionalColor = Color.Lerp(Color.white * 0.3f, Color.cyan, introFade); // Dawn colors
                            break;
                            
                        case StoryMoment.Rising:
                            // "Searching... the lights search for something, sweeping with purpose"
                            emotionalIntensity = 0.4f + intensity * 0.3f;
                            emotionalSpeed = 0.8f + intensity * 0.5f; // Gradually speeding up
                            emotionalRange = 45f + Mathf.Sin(t * 1.5f) * 15f; // Hunting pattern
                            emotionalColor = Color.Lerp(Color.cyan, Color.yellow, intensity); // Building warmth
                            
                            // Occasional sweeps across the space
                            if (analysisData.IsBeatAt(t, 0.2f))
                            {
                                emotionalIntensity = 0.8f;
                            }
                            break;
                            
                        case StoryMoment.Conflict:
                            // "Panic! The lights slash chaotically, cutting through space"
                            emotionalIntensity = 0.4f + Mathf.Sin(t * 6f + i * 1.5f) * 0.3f; // Erratic flickering
                            emotionalSpeed = 2f + Mathf.Sin(t * 3f) * 1.5f; // Frantic speed changes
                            emotionalRange = 70f + Mathf.Sin(t * 5f + i) * 40f; // Wild unpredictable sweeps
                            emotionalColor = Color.Lerp(Color.red, Color.magenta, Mathf.Sin(t * 4f) * 0.5f + 0.5f); // Alarm colors
                            flickering = true;
                            break;
                            
                        case StoryMoment.Climax:
                            // "EXPLOSION! All beams converge then burst outward in celebration"
                            float climaxPhase = (t * 4f + i * 0.3f) % (Mathf.PI * 2f);
                            bool converging = Mathf.Sin(climaxPhase) > 0;
                            
                            emotionalIntensity = 0.7f + intensity * 0.3f; // MAXIMUM POWER (capped at 1.0)
                            emotionalSpeed = 3f + intensity * 2f; // FAST
                            emotionalRange = converging ? 15f : 90f; // Converge tight, then BLAST wide
                            emotionalColor = Color.white; // Pure bright white
                            
                            // Strobe on every beat
                            if (analysisData.IsBeatAt(t, 0.1f))
                            {
                                emotionalIntensity = 1f;
                            }
                            break;
                            
                        case StoryMoment.Resolution:
                            // "Release... the beams slow, spreading gently like a breath out"
                            float resolutionCalm = Mathf.Min(1f, timeSinceBeatStart / 6f);
                            emotionalIntensity = Mathf.Lerp(0.8f, 0.3f, Mathf.SmoothStep(0, 1, resolutionCalm));
                            emotionalSpeed = Mathf.Lerp(2f, 0.4f, resolutionCalm); // Gradually slowing
                            emotionalRange = Mathf.Lerp(60f, 40f, resolutionCalm); // Settling into gentle sway
                            emotionalColor = Color.Lerp(Color.yellow, new Color(0.6f, 0.8f, 1f), resolutionCalm); // Warm to cool
                            break;
                            
                        case StoryMoment.Finale:
                            // "Farewell... one last graceful sweep before the lights fade"
                            float exitProgress = timeSinceBeatStart / 5f;
                            emotionalIntensity = Mathf.Lerp(0.3f, 0.01f, Mathf.SmoothStep(0, 1, exitProgress));
                            emotionalSpeed = 0.5f - exitProgress * 0.3f; // Slowing to stillness
                            emotionalRange = 50f + Mathf.Sin(t * 0.5f) * 20f; // One final graceful arc
                            emotionalColor = Color.Lerp(new Color(0.6f, 0.8f, 1f), Color.white * 0.1f, exitProgress); // Fade to dim
                            break;
                    }
                    
                    // Apply personality to movement
                    emotionalSpeed *= 1f + laserPersonality * 0.5f;
                    
                    // Flickering in conflict/panic
                    if (flickering && random.NextDouble() < 0.3)
                    {
                        emotionalIntensity *= 0.3f; // Random dropout
                    }
                    
                    // Clamp intensity to range [0.01, 1.0]
                    emotionalIntensity = Mathf.Clamp(emotionalIntensity, 0.01f, 1f);
                    
                    // Set intensity
                    intensityCurve.AddKey(t, emotionalIntensity);
                    
                    // Set color
                    colorR.AddKey(t, emotionalColor.r);
                    colorG.AddKey(t, emotionalColor.g);
                    colorB.AddKey(t, emotionalColor.b);
                    
                    // Sweep motion - emotional arc through space
                    float sweepPattern = Mathf.Sin(t * baseSweepSpeed * emotionalSpeed) * emotionalRange;
                    float yawDrift = t * (20f * emotionalSpeed);
                    
                    // Add tremor during intense moments
                    float tremor = 0f;
                    if (beat.moment == StoryMoment.Conflict || beat.moment == StoryMoment.Climax)
                    {
                        tremor = Mathf.Sin(t * 15f + i * 2f) * 5f;
                    }
                    
                    Quaternion rotation = initialRot * Quaternion.Euler(
                        sweepPattern + tremor,
                        yawDrift,
                        Mathf.Sin(t * emotionalSpeed * 0.3f) * 10f // Subtle roll
                    );
                    
                    rotX.AddKey(t, rotation.x);
                    rotY.AddKey(t, rotation.y);
                    rotZ.AddKey(t, rotation.z);
                    rotW.AddKey(t, rotation.w);
                    
                    previousBeat = beat;
                }
                
                clip.SetCurve("", typeof(Light), "m_Intensity", intensityCurve);
                clip.SetCurve("", typeof(Light), "m_Color.r", colorR);
                clip.SetCurve("", typeof(Light), "m_Color.g", colorG);
                clip.SetCurve("", typeof(Light), "m_Color.b", colorB);
                clip.SetCurve("", typeof(Transform), "localRotation.x", rotX);
                clip.SetCurve("", typeof(Transform), "localRotation.y", rotY);
                clip.SetCurve("", typeof(Transform), "localRotation.z", rotZ);
                clip.SetCurve("", typeof(Transform), "localRotation.w", rotW);
                
                string clipPath = TimelineUtilities.SaveAnimationClip(clip, clipsFolder);
                
                var track = timeline.CreateTrack<AnimationTrack>(null, laser.gameObject.name);
                director.SetGenericBinding(track, laser.gameObject);
                var timelineClip = track.CreateClip(clip);
                timelineClip.displayName = clip.name;
                timelineClip.duration = clip.length;
            }
        }
        
        private static void AnimateAmbientLighting(Light ambient, TimelineAsset timeline, PlayableDirector director,
            List<MusicSectionData> musicSections, MusicAnalysisData analysisData, MusicVideoSettings settings, string clipsFolder)
        {
            // Add Animator component
            var animator = ambient.gameObject.GetComponent<Animator>();
            if (animator == null)
            {
                animator = ambient.gameObject.AddComponent<Animator>();
            }
            
            float timeStep = settings.keyframeInterval;
            AnimationClip clip = new AnimationClip { name = "Ambient_Lighting_Anim" };
            
            var intensityCurve = new AnimationCurve();
            var colorR = new AnimationCurve();
            var colorG = new AnimationCurve();
            var colorB = new AnimationCurve();
            
            for (float t = 0; t <= analysisData.maxTime; t += timeStep)
            {
                var section = musicSections.FirstOrDefault(s => t >= s.startTime && t < s.endTime);
                float intensity = section.intensity > 0 ? section.intensity : 0.5f;
                
                // Ambient dims during intense moments, brightens during calm
                float ambientIntensity = Mathf.Lerp(0.5f, 3f, 1f - intensity);
                intensityCurve.AddKey(t, ambientIntensity);
                
                // Color shifts based on energy
                Color ambientColor = Color.Lerp(
                    new Color(0.2f, 0.2f, 0.4f), // Dark blue (intense)
                    new Color(0.6f, 0.6f, 0.8f), // Light blue (calm)
                    1f - intensity
                );
                
                colorR.AddKey(t, ambientColor.r);
                colorG.AddKey(t, ambientColor.g);
                colorB.AddKey(t, ambientColor.b);
            }
            
            clip.SetCurve("", typeof(Light), "m_Intensity", intensityCurve);
            clip.SetCurve("", typeof(Light), "m_Color.r", colorR);
            clip.SetCurve("", typeof(Light), "m_Color.g", colorG);
            clip.SetCurve("", typeof(Light), "m_Color.b", colorB);
            
            string clipPath = TimelineUtilities.SaveAnimationClip(clip, clipsFolder);
            
            var track = timeline.CreateTrack<AnimationTrack>(null, ambient.gameObject.name);
            director.SetGenericBinding(track, ambient.gameObject);
            var timelineClip = track.CreateClip(clip);
            timelineClip.displayName = clip.name;
            timelineClip.duration = clip.length;
        }
        
        private static void AnimateFollowSpots(List<Light> followSpots, TimelineAsset timeline, PlayableDirector director, MusicVideoCast cast,
            List<StoryBeat> storyBeats, MusicAnalysisData analysisData, MusicVideoSettings settings, string clipsFolder)
        {
            // Follow spots stay on at full intensity and track main cast
            float timeStep = settings.keyframeInterval;
            
            for (int i = 0; i < followSpots.Count; i++)
            {
                Light followSpot = followSpots[i];
                
                // Add Animator component
                var animator = followSpot.gameObject.GetComponent<Animator>();
                if (animator == null)
                {
                    animator = followSpot.gameObject.AddComponent<Animator>();
                }
                
                AnimationClip clip = new AnimationClip { name = $"{followSpot.gameObject.name}_Anim" };
                
                var intensityCurve = new AnimationCurve();
                
                for (float t = 0; t <= analysisData.maxTime; t += timeStep)
                {
                    // Follow spots always on during performance
                    intensityCurve.AddKey(t, 6f);
                }
                
                clip.SetCurve("", typeof(Light), "m_Intensity", intensityCurve);
                
                string clipPath = TimelineUtilities.SaveAnimationClip(clip, clipsFolder);
                
                var track = timeline.CreateTrack<AnimationTrack>(null, followSpot.gameObject.name);
                director.SetGenericBinding(track, followSpot.gameObject);
                var timelineClip = track.CreateClip(clip);
                timelineClip.displayName = clip.name;
                timelineClip.duration = clip.length;
            }
        }
    }
}
