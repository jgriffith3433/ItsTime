using UnityEngine;
using UnityEngine.Timeline;
using System.Collections.Generic;
using System.Linq;

namespace MusicVideoGenerator
{
    /// <summary>
    /// Generates animation clips for cast members based on their roles and story beats.
    /// </summary>
    public static class AnimationGenerator
    {
        private const float ORBIT_BASE_SPEED = 30f;
        private const float ORBIT_RADIUS_SCALE = 0.3f;
        private const float CLIMAX_POSITION_PULL = 0.2f;
        private const float FOCUS_SCALE_MULTIPLIER = 1.3f;
        private const float FOCUS_SPEED_MULTIPLIER = 1.5f;
        private const float BEAT_DETECTION_WINDOW = 0.05f;
        private const float ONSET_DETECTION_WINDOW = 0.05f;
        
        /// <summary>
        /// Generate an animation clip for a cast member.
        /// </summary>
        public static AnimationClip GenerateAnimationClip(
            CastMember castMember,
            int castIndex,
            List<StoryBeat> storyBeats,
            List<MusicSectionData> musicSections,
            MusicAnalysisData analysisData,
            MusicVideoSettings settings,
            System.Random random)
        {
            var clip = new AnimationClip { name = $"{castMember.gameObject.name}_Animation" };
            var config = castMember.GetBehaviorConfig();
            var initialTransform = castMember.GetInitialTransform();
            
            var curves = InitializeCurves();
            var state = InitializeAnimationState(initialTransform, config);
            
            Vector3 rotationAxis = DetermineRotationAxis(random);
            float timeStep = settings.keyframeInterval;
            
            // Generate keyframes
            for (float t = 0; t <= analysisData.maxTime; t += timeStep)
            {
                var section = GetSectionAtTime(musicSections, t);
                var beat = GetBeatAtTime(storyBeats, t);
                
                bool isFocused = beat.IsCastMemberFocused(castIndex);
                float intensity = section.intensity * config.energyMultiplier;
                
                UpdateAnimationState(
                    ref state,
                    castMember,
                    config,
                    section,
                    beat,
                    isFocused,
                    intensity,
                    t,
                    timeStep,
                    rotationAxis,
                    initialTransform,
                    analysisData,
                    settings,
                    random
                );
                
                AddKeyframesToCurves(curves, state, initialTransform, t);
            }
            
            // Optimize if needed
            if (settings.optimizeCurves)
            {
                OptimizeAllCurves(curves, settings.curveOptimizationTolerance);
            }
            
            // Apply curves to clip
            ApplyCurvesToClip(clip, curves, config);
            
            return clip;
        }
        
        private static AnimationCurves InitializeCurves()
        {
            return new AnimationCurves
            {
                posX = new AnimationCurve(),
                posY = new AnimationCurve(),
                posZ = new AnimationCurve(),
                rotX = new AnimationCurve(),
                rotY = new AnimationCurve(),
                rotZ = new AnimationCurve(),
                rotW = new AnimationCurve(),
                scaleX = new AnimationCurve(),
                scaleY = new AnimationCurve(),
                scaleZ = new AnimationCurve(),
                colorR = new AnimationCurve(),
                colorG = new AnimationCurve(),
                colorB = new AnimationCurve()
            };
        }
        
        private static AnimationState InitializeAnimationState(TransformData initialTransform, RoleBehaviorConfig config)
        {
            return new AnimationState
            {
                position = initialTransform.position,
                rotation = initialTransform.rotation,
                scale = initialTransform.scale,
                color = config.primaryColor,
                accumulatedRotationAngle = 0f,
                hueShift = 0f
            };
        }
        
        private static Vector3 DetermineRotationAxis(System.Random random)
        {
            return new Vector3(
                (float)random.NextDouble(),
                (float)random.NextDouble(),
                (float)random.NextDouble()
            ).normalized;
        }
        
        private static MusicSectionData GetSectionAtTime(List<MusicSectionData> sections, float time)
        {
            return sections.FirstOrDefault(s => time >= s.startTime && time < s.endTime);
        }
        
        private static StoryBeat GetBeatAtTime(List<StoryBeat> beats, float time)
        {
            return beats.OrderBy(b => Mathf.Abs(b.time - time)).FirstOrDefault();
        }
        
        private static void UpdateAnimationState(
            ref AnimationState state,
            CastMember castMember,
            RoleBehaviorConfig config,
            MusicSectionData section,
            StoryBeat beat,
            bool isFocused,
            float intensity,
            float time,
            float timeStep,
            Vector3 rotationAxis,
            TransformData initialTransform,
            MusicAnalysisData analysisData,
            MusicVideoSettings settings,
            System.Random random)
        {
            // POSITION
            if (config.canMove)
            {
                Vector3 targetPos = CalculateTargetPosition(
                    config,
                    initialTransform.position,
                    section,
                    isFocused,
                    intensity,
                    time,
                    random
                );
                
                // Apply movement bounds
                if (settings.constrainMovement)
                {
                    targetPos = ClampToBounds(targetPos, initialTransform.position, settings.movementBounds);
                }
                
                // Apply global movement intensity multiplier
                Vector3 movementOffset = (targetPos - initialTransform.position) * settings.movementIntensityMultiplier;
                targetPos = initialTransform.position + movementOffset;
                
                state.position = Vector3.Lerp(state.position, targetPos, config.movementIntensity * timeStep);
            }
            
            // ROTATION
            if (config.canRotate)
            {
                float rotSpeed = config.rotationSpeed * (1f + intensity);
                if (isFocused) rotSpeed *= FOCUS_SPEED_MULTIPLIER;
                
                state.accumulatedRotationAngle += rotSpeed * timeStep;
                Quaternion deltaRotation = Quaternion.AngleAxis(state.accumulatedRotationAngle, rotationAxis);
                state.rotation = initialTransform.rotation * deltaRotation;
            }
            
            // SCALE
            if (config.canScale)
            {
                float scaleMultiplier = Mathf.Lerp(config.scaleMin, config.scaleMax, intensity);
                if (isFocused) scaleMultiplier *= FOCUS_SCALE_MULTIPLIER;
                
                // Beat pulse
                if (settings.enableBeatPulse && analysisData.IsBeatAt(time))
                {
                    scaleMultiplier += settings.beatPulseIntensity * intensity;
                }
                
                // Scale relative to initial scale, not Vector3.one
                Vector3 targetScale = initialTransform.scale * scaleMultiplier;
                
                state.scale = Vector3.Lerp(
                    state.scale,
                    targetScale,
                    settings.scaleSpeed * timeStep
                );
            }
            
            // COLOR
            if (config.canChangeColor)
            {
                Color targetColor = CalculateTargetColor(config, beat, intensity, isFocused);
                
                // Onset flash
                if (settings.enableOnsetFlashes && analysisData.IsOnsetAt(time))
                {
                    targetColor = Color.Lerp(targetColor, Color.white, settings.flashIntensity);
                }
                
                state.color = Color.Lerp(
                    state.color,
                    targetColor,
                    config.colorTransitionSpeed * timeStep
                );
            }
        }
        
        private static Vector3 CalculateTargetPosition(
            RoleBehaviorConfig config,
            Vector3 initialPosition,
            MusicSectionData section,
            bool isFocused,
            float intensity,
            float time,
            System.Random random)
        {
            Vector3 offset = Vector3.zero;
            
            switch (section.section)
            {
                case MusicSection.Intro:
                    // Ease in from the side
                    float introProgress = (time - section.startTime) / Mathf.Max(0.01f, section.Duration);
                    offset = Vector3.Lerp(Vector3.right * config.movementRange, Vector3.zero, introProgress);
                    break;
                
                case MusicSection.Outro:
                    // Ease out to the side
                    float outroProgress = (time - section.startTime) / Mathf.Max(0.01f, section.Duration);
                    offset = Vector3.Lerp(Vector3.zero, Vector3.left * config.movementRange, outroProgress);
                    break;
                
                case MusicSection.Climax:
                    if (isFocused)
                    {
                        // Move toward center
                        offset = -initialPosition * intensity * CLIMAX_POSITION_PULL;
                    }
                    else
                    {
                        offset = CalculateOrbitOffset(time, intensity, config);
                    }
                    break;
                
                case MusicSection.Breakdown:
                    // Return to starting position
                    offset = Vector3.zero;
                    break;
                
                default:
                    // Orbital movement
                    offset = CalculateOrbitOffset(time, intensity, config);
                    break;
            }
            
            return initialPosition + offset;
        }
        
        private static Vector3 CalculateOrbitOffset(float time, float intensity, RoleBehaviorConfig config)
        {
            float orbitSpeed = intensity * ORBIT_BASE_SPEED * Mathf.Deg2Rad * config.movementIntensity;
            float orbitRadius = config.movementRange * (1f - intensity * 0.3f) * ORBIT_RADIUS_SCALE;
            
            float angle = time * orbitSpeed;
            
            return new Vector3(
                Mathf.Cos(angle) * orbitRadius,
                Mathf.Sin(time * 0.5f) * orbitRadius * 0.3f,
                Mathf.Sin(angle) * orbitRadius
            );
        }
        
        private static Vector3 ClampToBounds(Vector3 position, Vector3 center, Vector3 bounds)
        {
            Vector3 halfBounds = bounds * 0.5f;
            return new Vector3(
                Mathf.Clamp(position.x, center.x - halfBounds.x, center.x + halfBounds.x),
                Mathf.Clamp(position.y, center.y - halfBounds.y, center.y + halfBounds.y),
                Mathf.Clamp(position.z, center.z - halfBounds.z, center.z + halfBounds.z)
            );
        }
        
        private static Color CalculateTargetColor(
            RoleBehaviorConfig config,
            StoryBeat beat,
            float intensity,
            bool isFocused)
        {
            Color baseColor = Color.Lerp(config.primaryColor, config.secondaryColor, intensity);
            
            // Adjust based on story moment
            switch (beat.moment)
            {
                case StoryMoment.Conflict:
                    baseColor = Color.Lerp(baseColor, Color.red, 0.3f);
                    break;
                case StoryMoment.Climax:
                    baseColor *= 1.5f;
                    break;
                case StoryMoment.Resolution:
                    baseColor = Color.Lerp(baseColor, Color.blue, 0.2f);
                    break;
            }
            
            if (isFocused)
            {
                baseColor *= 1.3f;
            }
            
            baseColor.r = Mathf.Clamp01(baseColor.r);
            baseColor.g = Mathf.Clamp01(baseColor.g);
            baseColor.b = Mathf.Clamp01(baseColor.b);
            
            return baseColor;
        }
        
        private static void AddKeyframesToCurves(
            AnimationCurves curves,
            AnimationState state,
            TransformData initialTransform,
            float time)
        {
            curves.posX.AddKey(time, state.position.x);
            curves.posY.AddKey(time, state.position.y);
            curves.posZ.AddKey(time, state.position.z);
            
            curves.rotX.AddKey(time, state.rotation.x);
            curves.rotY.AddKey(time, state.rotation.y);
            curves.rotZ.AddKey(time, state.rotation.z);
            curves.rotW.AddKey(time, state.rotation.w);
            
            curves.scaleX.AddKey(time, state.scale.x);
            curves.scaleY.AddKey(time, state.scale.y);
            curves.scaleZ.AddKey(time, state.scale.z);
            
            curves.colorR.AddKey(time, state.color.r);
            curves.colorG.AddKey(time, state.color.g);
            curves.colorB.AddKey(time, state.color.b);
        }
        
        private static void OptimizeAllCurves(AnimationCurves curves, float tolerance)
        {
            OptimizeCurve(ref curves.posX, tolerance);
            OptimizeCurve(ref curves.posY, tolerance);
            OptimizeCurve(ref curves.posZ, tolerance);
            OptimizeCurve(ref curves.rotX, tolerance);
            OptimizeCurve(ref curves.rotY, tolerance);
            OptimizeCurve(ref curves.rotZ, tolerance);
            OptimizeCurve(ref curves.rotW, tolerance);
            OptimizeCurve(ref curves.scaleX, tolerance);
            OptimizeCurve(ref curves.scaleY, tolerance);
            OptimizeCurve(ref curves.scaleZ, tolerance);
        }
        
        private static void OptimizeCurve(ref AnimationCurve curve, float tolerance)
        {
            if (curve.keys.Length <= 2) return;
            
            var optimizedKeys = new List<Keyframe> { curve.keys[0] };
            
            for (int i = 1; i < curve.keys.Length - 1; i++)
            {
                float prevValue = curve.keys[i - 1].value;
                float currValue = curve.keys[i].value;
                float nextValue = curve.keys[i + 1].value;
                float expectedValue = Mathf.Lerp(prevValue, nextValue, 0.5f);
                
                if (Mathf.Abs(currValue - expectedValue) > tolerance)
                {
                    optimizedKeys.Add(curve.keys[i]);
                }
            }
            
            optimizedKeys.Add(curve.keys[curve.keys.Length - 1]);
            curve = new AnimationCurve(optimizedKeys.ToArray());
        }
        
        private static void ApplyCurvesToClip(AnimationClip clip, AnimationCurves curves, RoleBehaviorConfig config)
        {
            if (config.canMove && curves.posX.keys.Length > 0)
            {
                clip.SetCurve("", typeof(Transform), "localPosition.x", curves.posX);
                clip.SetCurve("", typeof(Transform), "localPosition.y", curves.posY);
                clip.SetCurve("", typeof(Transform), "localPosition.z", curves.posZ);
            }
            
            if (config.canRotate && curves.rotX.keys.Length > 0)
            {
                clip.SetCurve("", typeof(Transform), "localRotation.x", curves.rotX);
                clip.SetCurve("", typeof(Transform), "localRotation.y", curves.rotY);
                clip.SetCurve("", typeof(Transform), "localRotation.z", curves.rotZ);
                clip.SetCurve("", typeof(Transform), "localRotation.w", curves.rotW);
            }
            
            if (config.canScale && curves.scaleX.keys.Length > 0)
            {
                clip.SetCurve("", typeof(Transform), "localScale.x", curves.scaleX);
                clip.SetCurve("", typeof(Transform), "localScale.y", curves.scaleY);
                clip.SetCurve("", typeof(Transform), "localScale.z", curves.scaleZ);
            }
            
            if (config.canChangeColor && curves.colorR.keys.Length > 0)
            {
                clip.SetCurve("", typeof(Renderer), "material._Color.r", curves.colorR);
                clip.SetCurve("", typeof(Renderer), "material._Color.g", curves.colorG);
                clip.SetCurve("", typeof(Renderer), "material._Color.b", curves.colorB);
            }
        }
        
        private struct AnimationCurves
        {
            public AnimationCurve posX, posY, posZ;
            public AnimationCurve rotX, rotY, rotZ, rotW;
            public AnimationCurve scaleX, scaleY, scaleZ;
            public AnimationCurve colorR, colorG, colorB;
        }
        
        private struct AnimationState
        {
            public Vector3 position;
            public Quaternion rotation;
            public Vector3 scale;
            public Color color;
            public float accumulatedRotationAngle;
            public float hueShift;
        }
    }
}