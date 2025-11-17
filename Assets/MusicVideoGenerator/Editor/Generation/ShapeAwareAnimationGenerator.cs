using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace MusicVideoGenerator
{
    /// <summary>
    /// Generates shape-aware animations that respect the geometric properties of compound shapes.
    /// </summary>
    public static class ShapeAwareAnimationGenerator
    {
        /// <summary>
        /// Generate an animation clip that respects the shape's geometry
        /// </summary>
        public static AnimationClip GenerateShapeAwareClip(
            CastMember castMember,
            int castIndex,
            List<StoryBeat> storyBeats,
            List<MusicSectionData> musicSections,
            MusicAnalysisData analysisData,
            MusicVideoSettings settings,
            System.Random random)
        {
            var clip = new AnimationClip { name = $"{castMember.gameObject.name}_ShapeAware" };
            var config = castMember.GetBehaviorConfig();
            var initialTransform = castMember.GetInitialTransform();
            
            // Check if this has shape identity
            var shapeIdentity = castMember.gameObject.GetComponent<ShapeIdentity>();
            
            if (shapeIdentity != null)
            {
                // Shape-aware animation
                GenerateShapeSpecificAnimation(
                    clip,
                    shapeIdentity,
                    castMember,
                    castIndex,
                    config,
                    initialTransform,
                    storyBeats,
                    musicSections,
                    analysisData,
                    settings,
                    random
                );
            }
            else
            {
                // Fallback to simple animation for primitives
                GenerateSimplePrimitiveAnimation(
                    clip,
                    castMember,
                    castIndex,
                    config,
                    initialTransform,
                    storyBeats,
                    musicSections,
                    analysisData,
                    settings,
                    random
                );
            }
            
            return clip;
        }
        
        private static void GenerateShapeSpecificAnimation(
            AnimationClip clip,
            ShapeIdentity shape,
            CastMember castMember,
            int castIndex,
            RoleBehaviorConfig config,
            TransformData initialTransform,
            List<StoryBeat> storyBeats,
            List<MusicSectionData> musicSections,
            MusicAnalysisData analysisData,
            MusicVideoSettings settings,
            System.Random random)
        {
            float timeStep = settings.keyframeInterval;
            
            switch (shape.shapeType)
            {
                case ShapeType.OrbitalSystem:
                    AnimateOrbitalSystem(clip, shape, castMember, castIndex, config, initialTransform, 
                        storyBeats, musicSections, analysisData, settings, random, timeStep);
                    break;
                    
                case ShapeType.StarBurst:
                    AnimateStarBurst(clip, shape, castMember, castIndex, config, initialTransform,
                        storyBeats, musicSections, analysisData, settings, random, timeStep);
                    break;
                    
                case ShapeType.Helix:
                    AnimateHelix(clip, shape, castMember, castIndex, config, initialTransform,
                        storyBeats, musicSections, analysisData, settings, random, timeStep);
                    break;
                    
                case ShapeType.Ring:
                case ShapeType.Torus:
                case ShapeType.ChainLinks:
                    AnimateRingStructure(clip, shape, castMember, castIndex, config, initialTransform,
                        storyBeats, musicSections, analysisData, settings, random, timeStep);
                    break;
                    
                case ShapeType.CrystalCluster:
                    AnimateCrystalCluster(clip, shape, castMember, castIndex, config, initialTransform,
                        storyBeats, musicSections, analysisData, settings, random, timeStep);
                    break;
                    
                case ShapeType.SpiralGalaxy:
                case ShapeType.FibonacciSpiral:
                    AnimateSpiralStructure(clip, shape, castMember, castIndex, config, initialTransform,
                        storyBeats, musicSections, analysisData, settings, random, timeStep);
                    break;
                    
                case ShapeType.Mandala:
                case ShapeType.GeometricFlower:
                    AnimateMandalaPulse(clip, shape, castMember, castIndex, config, initialTransform,
                        storyBeats, musicSections, analysisData, settings, random, timeStep);
                    break;
                    
                case ShapeType.Gyroscope:
                    AnimateGyroscope(clip, shape, castMember, castIndex, config, initialTransform,
                        storyBeats, musicSections, analysisData, settings, random, timeStep);
                    break;
                    
                case ShapeType.Atom:
                case ShapeType.Molecular:
                    AnimateMolecularStructure(clip, shape, castMember, castIndex, config, initialTransform,
                        storyBeats, musicSections, analysisData, settings, random, timeStep);
                    break;
                    
                case ShapeType.Tower:
                case ShapeType.PyramidArray:
                    AnimateTowerStructure(clip, shape, castMember, castIndex, config, initialTransform,
                        storyBeats, musicSections, analysisData, settings, random, timeStep);
                    break;
                    
                default:
                    // Generic compound animation
                    AnimateGenericCompound(clip, shape, castMember, castIndex, config, initialTransform,
                        storyBeats, musicSections, analysisData, settings, random, timeStep);
                    break;
            }
        }
        
        #region Shape-Specific Animation Methods
        
        private static void AnimateOrbitalSystem(AnimationClip clip, ShapeIdentity shape, CastMember castMember, 
            int castIndex, RoleBehaviorConfig config, TransformData initialTransform, List<StoryBeat> storyBeats,
            List<MusicSectionData> musicSections, MusicAnalysisData analysisData, MusicVideoSettings settings,
            System.Random random, float timeStep)
        {
            // Parent rotates slowly, children orbit around it
            var posX = new AnimationCurve();
            var posY = new AnimationCurve();
            var posZ = new AnimationCurve();
            var rotX = new AnimationCurve();
            var rotY = new AnimationCurve();
            var rotZ = new AnimationCurve();
            var rotW = new AnimationCurve();
            
            Vector3 rotationAxis = shape.rotationAxis.normalized;
            float accumulatedAngle = 0f;
            
            for (float t = 0; t <= analysisData.maxTime; t += timeStep)
            {
                var section = GetSectionAtTime(musicSections, t);
                var beat = GetBeatAtTime(storyBeats, t);
                bool isFocused = beat.IsCastMemberFocused(castIndex);
                float intensity = section.intensity * config.energyMultiplier;
                
                // Gentle orbital rotation
                float rotSpeed = 20f * intensity * shape.animationSpeed;
                accumulatedAngle += rotSpeed * timeStep;
                
                Quaternion rotation = initialTransform.rotation * Quaternion.AngleAxis(accumulatedAngle, rotationAxis);
                
                // Orbital movement around stage center
                float angleOffset = castIndex * 0.5f;
                float orbitRadius = Mathf.Max(settings.stageBounds.x, settings.stageBounds.z) * 0.5f;
                Vector3 targetPos = settings.stageCenter;
                targetPos.x += Mathf.Cos(t * 0.2f + angleOffset) * orbitRadius;
                targetPos.z += Mathf.Sin(t * 0.2f + angleOffset) * orbitRadius;
                targetPos.y += Mathf.Sin(t * 0.3f) * 1f;
                
                if (settings.constrainMovement)
                {
                    targetPos = ClampToBounds(targetPos, settings.stageCenter, settings.movementBounds);
                }
                
                // Build curves
                posX.AddKey(t, targetPos.x);
                posY.AddKey(t, targetPos.y);
                posZ.AddKey(t, targetPos.z);
                rotX.AddKey(t, rotation.x);
                rotY.AddKey(t, rotation.y);
                rotZ.AddKey(t, rotation.z);
                rotW.AddKey(t, rotation.w);
            }
            
            // Apply curves to clip
            clip.SetCurve("", typeof(Transform), "localPosition.x", posX);
            clip.SetCurve("", typeof(Transform), "localPosition.y", posY);
            clip.SetCurve("", typeof(Transform), "localPosition.z", posZ);
            clip.SetCurve("", typeof(Transform), "localRotation.x", rotX);
            clip.SetCurve("", typeof(Transform), "localRotation.y", rotY);
            clip.SetCurve("", typeof(Transform), "localRotation.z", rotZ);
            clip.SetCurve("", typeof(Transform), "localRotation.w", rotW);
        }
        
        private static void AnimateStarBurst(AnimationClip clip, ShapeIdentity shape, CastMember castMember,
            int castIndex, RoleBehaviorConfig config, TransformData initialTransform, List<StoryBeat> storyBeats,
            List<MusicSectionData> musicSections, MusicAnalysisData analysisData, MusicVideoSettings settings,
            System.Random random, float timeStep)
        {
            // Parent stays mostly still, spikes pulse in/out on beats
            Transform[] children = shape.GetShapeElements().Select(r => r.transform).ToArray();
            
            for (float t = 0; t <= analysisData.maxTime; t += timeStep)
            {
                var section = GetSectionAtTime(musicSections, t);
                var beat = GetBeatAtTime(storyBeats, t);
                bool isFocused = beat.IsCastMemberFocused(castIndex);
                float intensity = section.intensity * config.energyMultiplier;
                
                // Pulsate spikes on beats
                float pulseFactor = 1f;
                if (analysisData.IsBeatAt(t, 0.1f))
                {
                    pulseFactor = 1f + intensity * 0.5f;
                }
                
                // Animate each spike outward
                for (int i = 1; i < children.Length; i++) // Skip center (index 0)
                {
                    Transform spike = children[i];
                    string path = GetRelativePath(castMember.gameObject.transform, spike);
                    
                    Vector3 direction = spike.localPosition.normalized;
                    float originalDist = spike.localPosition.magnitude;
                    Vector3 targetPos = direction * originalDist * pulseFactor;
                    
                    AddPositionKeyframes(clip, path, targetPos, t);
                }
                
                // Parent gentle rotation and orbit
                Quaternion rotation = initialTransform.rotation * Quaternion.Euler(0, t * 10f, 0);
                
                float angleOffset = castIndex * 0.6f;
                float orbitRadius = Mathf.Max(settings.stageBounds.x, settings.stageBounds.z) * 0.55f;
                Vector3 parentPos = settings.stageCenter;
                parentPos.x += Mathf.Cos(t * 0.25f + angleOffset) * orbitRadius;
                parentPos.z += Mathf.Sin(t * 0.25f + angleOffset) * orbitRadius;
                parentPos.y += Mathf.Cos(t * 0.35f) * 1.2f;
                
                AddRotationKeyframes(clip, "", rotation, t);
                AddPositionKeyframes(clip, "", parentPos, t);
            }
            
            FinalizeAllCurves(clip);
        }
        
        private static void AnimateHelix(AnimationClip clip, ShapeIdentity shape, CastMember castMember,
            int castIndex, RoleBehaviorConfig config, TransformData initialTransform, List<StoryBeat> storyBeats,
            List<MusicSectionData> musicSections, MusicAnalysisData analysisData, MusicVideoSettings settings,
            System.Random random, float timeStep)
        {
            // Helix: Epic DNA strand - travels in circular paths while spinning with dramatic scale changes
            float baseSpinSpeed = 40f * shape.animationSpeed;
            float orbitRadius = Mathf.Max(settings.stageBounds.x, settings.stageBounds.z) * 0.6f; // Use stage bounds
            
            for (float t = 0; t <= analysisData.maxTime; t += timeStep)
            {
                var section = GetSectionAtTime(musicSections, t);
                var beat = GetBeatAtTime(storyBeats, t);
                bool isFocused = beat.focusedCastIndices != null && beat.IsCastMemberFocused(castIndex);
                float intensity = section.intensity > 0 ? section.intensity : 0.5f;
                
                // Multi-axis rotation for dynamic look
                float angleY = t * baseSpinSpeed;
                float angleX = t * 15f; // Slight tilt rotation
                float angleZ = Mathf.Sin(t * 0.5f) * 20f; // Wobble
                Quaternion rotation = initialTransform.rotation * Quaternion.Euler(angleX, angleY, angleZ);
                
                // Orbit in a figure-8 pattern when focused, otherwise gentle circle
                Vector3 pos = settings.stageCenter;
                float angleOffset = castIndex * 0.5f; // Spread cast members around
                if (isFocused)
                {
                    // Figure-8 pattern (lemniscate)
                    float orbitSpeed = 0.3f;
                    pos.x += Mathf.Sin(t * orbitSpeed + angleOffset) * orbitRadius;
                    pos.z += Mathf.Sin(t * orbitSpeed * 2f + angleOffset) * orbitRadius * 0.5f;
                    pos.y += Mathf.Cos(t * orbitSpeed) * 1.5f;
                }
                else
                {
                    // Gentle circular orbit
                    pos.x += Mathf.Sin(t * 0.2f + angleOffset) * orbitRadius * 0.5f;
                    pos.z += Mathf.Cos(t * 0.2f + angleOffset) * orbitRadius * 0.5f;
                    pos.y += Mathf.Sin(t * 0.4f) * 0.8f;
                }
                
                // Dynamic scale - expands dramatically on beats
                Vector3 scale = initialTransform.scale * 1.5f; // Make 50% bigger
                float basePulse = 1f + Mathf.Sin(t * 3f) * 0.12f * intensity;
                scale *= basePulse;
                if (analysisData.IsBeatAt(t, 0.08f))
                {
                    scale *= isFocused ? 1.4f : 1.15f; // Huge pulse on beat when focused
                }
                
                // Pulsating glow light
                float lightIntensity = 1.0f + intensity * 1.0f;
                if (analysisData.IsBeatAt(t, 0.08f))
                {
                    lightIntensity = isFocused ? 3.5f : 2.5f; // Strong radiation on beat
                }
                GetOrCreateCurve(clip, "", "m_Intensity").AddKey(t, lightIntensity);
                
                AddRotationKeyframes(clip, "", rotation, t);
                AddPositionKeyframes(clip, "", pos, t);
                AddScaleKeyframes(clip, "", scale, t);
            }
            
            FinalizeAllCurves(clip);
        }
        
        private static void AnimateRingStructure(AnimationClip clip, ShapeIdentity shape, CastMember castMember,
            int castIndex, RoleBehaviorConfig config, TransformData initialTransform, List<StoryBeat> storyBeats,
            List<MusicSectionData> musicSections, MusicAnalysisData analysisData, MusicVideoSettings settings,
            System.Random random, float timeStep)
        {
            // Ring: Elegant orbital motion with complex tumbling - like a spinning portal
            float travelRadius = Mathf.Max(settings.stageBounds.x, settings.stageBounds.z) * 0.8f; // Use stage bounds
            
            for (float t = 0; t <= analysisData.maxTime; t += timeStep)
            {
                var section = GetSectionAtTime(musicSections, t);
                var beat = GetBeatAtTime(storyBeats, t);
                bool isFocused = beat.focusedCastIndices != null && beat.IsCastMemberFocused(castIndex);
                float intensity = section.intensity > 0 ? section.intensity : 0.5f;
                
                // Complex tumbling on 3 axes - looks like a gyroscopic portal
                float speedMult = isFocused ? 1.5f : 1f;
                Quaternion rotation = initialTransform.rotation * 
                    Quaternion.Euler(t * 30f * speedMult, t * 50f * speedMult, t * 20f * speedMult);
                
                // Travel in a wide arc across the scene
                Vector3 pos = settings.stageCenter;
                float angleOffset = castIndex * 0.7f; // Spread cast members around
                if (isFocused)
                {
                    // Spiral outward and back in
                    float spiralProgress = (t * 0.25f) % (Mathf.PI * 2f);
                    float currentRadius = travelRadius * (1f + Mathf.Sin(spiralProgress) * 0.5f);
                    pos.x += Mathf.Cos(t * 0.35f + angleOffset) * currentRadius;
                    pos.z += Mathf.Sin(t * 0.35f + angleOffset) * currentRadius;
                    pos.y += Mathf.Sin(t * 0.5f) * 2f;
                }
                else
                {
                    // Slow arc across the scene
                    pos.x += Mathf.Sin(t * 0.15f + angleOffset) * travelRadius * 0.7f;
                    pos.z += Mathf.Cos(t * 0.15f + angleOffset) * travelRadius * 0.7f;
                    pos.y += Mathf.Sin(t * 0.3f) * 1f;
                }
                
                // Dramatic expand/contract like a portal opening
                Vector3 scale = initialTransform.scale;
                float portalPulse = 1f + Mathf.Sin(t * 2f) * 0.15f;
                scale *= portalPulse;
                if (analysisData.IsBeatAt(t, 0.08f) && isFocused)
                {
                    scale *= 1.35f; // Portal surge on beat
                }
                
                // Pulsating glow light
                float lightIntensity = 1.0f + intensity * 1.0f;
                if (analysisData.IsBeatAt(t, 0.08f))
                {
                    lightIntensity = isFocused ? 3.5f : 2.5f;
                }
                GetOrCreateCurve(clip, "", "m_Intensity").AddKey(t, lightIntensity);
                
                AddRotationKeyframes(clip, "", rotation, t);
                AddPositionKeyframes(clip, "", pos, t);
                AddScaleKeyframes(clip, "", scale, t);
            }
            
            FinalizeAllCurves(clip);
        }
        
        private static void AnimateCrystalCluster(AnimationClip clip, ShapeIdentity shape, CastMember castMember,
            int castIndex, RoleBehaviorConfig config, TransformData initialTransform, List<StoryBeat> storyBeats,
            List<MusicSectionData> musicSections, MusicAnalysisData analysisData, MusicVideoSettings settings,
            System.Random random, float timeStep)
        {
            // Crystals grow/shrink on beats
            Transform[] crystals = shape.GetShapeElements().Select(r => r.transform).ToArray();
            
            for (float t = 0; t <= analysisData.maxTime; t += timeStep)
            {
                var section = GetSectionAtTime(musicSections, t);
                float intensity = section.intensity * config.energyMultiplier;
                
                float growthFactor = 1f + Mathf.Sin(t * 2f) * 0.2f * intensity;
                
                // Scale each crystal
                foreach (var crystal in crystals)
                {
                    string path = GetRelativePath(castMember.gameObject.transform, crystal);
                    Vector3 scale = crystal.localScale * growthFactor;
                    AddScaleKeyframes(clip, path, scale, t);
                }
                
                // Parent gentle rotation and orbit
                Quaternion rotation = initialTransform.rotation * Quaternion.Euler(0, t * 5f, 0);
                
                float angleOffset = castIndex * 0.65f;
                float orbitRadius = Mathf.Max(settings.stageBounds.x, settings.stageBounds.z) * 0.58f;
                Vector3 parentPos = settings.stageCenter;
                parentPos.x += Mathf.Sin(t * 0.22f + angleOffset) * orbitRadius;
                parentPos.z += Mathf.Cos(t * 0.22f + angleOffset) * orbitRadius;
                parentPos.y += Mathf.Sin(t * 0.32f) * 1.1f;
                
                AddRotationKeyframes(clip, "", rotation, t);
                AddPositionKeyframes(clip, "", parentPos, t);
            }
            
            FinalizeAllCurves(clip);
        }
        
        private static void AnimateSpiralStructure(AnimationClip clip, ShapeIdentity shape, CastMember castMember,
            int castIndex, RoleBehaviorConfig config, TransformData initialTransform, List<StoryBeat> storyBeats,
            List<MusicSectionData> musicSections, MusicAnalysisData analysisData, MusicVideoSettings settings,
            System.Random random, float timeStep)
        {
            // Spiral: Hypnotic vortex - travels in expanding/contracting spiral with dramatic scale pulses
            float maxRadius = Mathf.Max(settings.stageBounds.x, settings.stageBounds.z) * 1.0f; // Use stage bounds
            
            for (float t = 0; t <= analysisData.maxTime; t += timeStep)
            {
                var section = GetSectionAtTime(musicSections, t);
                var beat = GetBeatAtTime(storyBeats, t);
                bool isFocused = beat.focusedCastIndices != null && beat.IsCastMemberFocused(castIndex);
                float intensity = section.intensity > 0 ? section.intensity : 0.5f;
                
                // Hypnotic spin with slight wobble
                float spinSpeed = isFocused ? 60f : 45f;
                float wobble = Mathf.Sin(t * 1.5f) * 10f;
                Quaternion rotation = initialTransform.rotation * Quaternion.Euler(wobble, t * spinSpeed, wobble * 0.5f);
                
                // Move in a 3D spiral path - expanding and contracting
                Vector3 pos = settings.stageCenter;
                float angleOffset = castIndex * 0.9f; // Spread cast members around
                if (isFocused)
                {
                    // Expanding/contracting spiral
                    float spiralRadius = maxRadius * Mathf.Abs(Mathf.Sin(t * 0.3f));
                    float spiralAngle = t * 0.8f + angleOffset;
                    pos.x += Mathf.Cos(spiralAngle) * spiralRadius;
                    pos.z += Mathf.Sin(spiralAngle) * spiralRadius;
                    pos.y += Mathf.Sin(t * 0.6f) * 2.5f;
                }
                else
                {
                    // Gentle spiral drift
                    float driftRadius = maxRadius * 0.4f;
                    pos.x += Mathf.Cos(t * 0.25f + angleOffset) * driftRadius;
                    pos.z += Mathf.Sin(t * 0.25f + angleOffset) * driftRadius;
                    pos.y += Mathf.Cos(t * 0.5f) * 1f;
                }
                
                // Dramatic breathing + vortex expansion on beats
                float breathCycle = 1f + Mathf.Sin(t * 2.5f) * 0.3f;
                Vector3 scale = initialTransform.scale * 1.5f * breathCycle; // Make 50% bigger
                if (analysisData.IsBeatAt(t, 0.08f))
                {
                    scale *= isFocused ? 1.5f : 1.25f; // MASSIVE vortex explosion
                }
                
                // Pulsating glow light
                float lightIntensity = 1.2f + intensity * 1.2f;
                if (analysisData.IsBeatAt(t, 0.08f))
                {
                    lightIntensity = isFocused ? 4.5f : 3.0f; // Maximum radiation burst
                }
                GetOrCreateCurve(clip, "", "m_Intensity").AddKey(t, lightIntensity);
                
                AddRotationKeyframes(clip, "", rotation, t);
                AddPositionKeyframes(clip, "", pos, t);
                AddScaleKeyframes(clip, "", scale, t);
            }
            
            FinalizeAllCurves(clip);
        }
        
        private static void AnimateMandalaPulse(AnimationClip clip, ShapeIdentity shape, CastMember castMember,
            int castIndex, RoleBehaviorConfig config, TransformData initialTransform, List<StoryBeat> storyBeats,
            List<MusicSectionData> musicSections, MusicAnalysisData analysisData, MusicVideoSettings settings,
            System.Random random, float timeStep)
        {
            // Mandala pulses from center outward
            for (float t = 0; t <= analysisData.maxTime; t += timeStep)
            {
                var section = GetSectionAtTime(musicSections, t);
                float intensity = section.intensity * config.energyMultiplier;
                
                // Gentle rotation and orbit
                Quaternion rotation = initialTransform.rotation * Quaternion.Euler(0, 0, t * 15f);
                
                float angleOffset = castIndex * 0.55f;
                float orbitRadius = Mathf.Max(settings.stageBounds.x, settings.stageBounds.z) * 0.62f;
                Vector3 parentPos = settings.stageCenter;
                parentPos.x += Mathf.Cos(t * 0.23f + angleOffset) * orbitRadius;
                parentPos.z += Mathf.Sin(t * 0.23f + angleOffset) * orbitRadius;
                parentPos.y += Mathf.Cos(t * 0.33f) * 1.0f;
                
                // Pulsing scale
                float scaleFactor = 1f + Mathf.Sin(t * 3f) * 0.2f * intensity;
                Vector3 scale = initialTransform.scale * 1.5f * scaleFactor; // Make 50% bigger
                
                AddRotationKeyframes(clip, "", rotation, t);
                AddPositionKeyframes(clip, "", parentPos, t);
                AddScaleKeyframes(clip, "", scale, t);
            }
            
            FinalizeAllCurves(clip);
        }
        
        private static void AnimateGyroscope(AnimationClip clip, ShapeIdentity shape, CastMember castMember,
            int castIndex, RoleBehaviorConfig config, TransformData initialTransform, List<StoryBeat> storyBeats,
            List<MusicSectionData> musicSections, MusicAnalysisData analysisData, MusicVideoSettings settings,
            System.Random random, float timeStep)
        {
            // Gyroscope: Technical marvel - orbits while spinning on multiple axes with dramatic tilts
            float orbitRadius = Mathf.Max(settings.stageBounds.x, settings.stageBounds.z) * 0.9f; // Use stage bounds
            
            for (float t = 0; t <= analysisData.maxTime; t += timeStep)
            {
                var section = GetSectionAtTime(musicSections, t);
                var beat = GetBeatAtTime(storyBeats, t);
                bool isFocused = beat.focusedCastIndices != null && beat.IsCastMemberFocused(castIndex);
                float intensity = section.intensity > 0 ? section.intensity : 0.5f;
                
                // Complex multi-axis precession with dramatic tilts
                float tiltAmount = isFocused ? 45f : 20f;
                float tilt = Mathf.Sin(t * 0.8f) * tiltAmount;
                Quaternion rotation = initialTransform.rotation * 
                    Quaternion.Euler(tilt + t * 40f, t * 60f, tilt * 0.5f + t * 30f);
                
                // Orbital movement in an elliptical path
                Vector3 pos = settings.stageCenter;
                float angleOffset = castIndex * 0.6f; // Spread cast members around
                if (isFocused)
                {
                    // Elliptical orbit with height variation
                    pos.x += Mathf.Cos(t * 0.4f + angleOffset) * orbitRadius;
                    pos.z += Mathf.Sin(t * 0.4f + angleOffset) * orbitRadius * 0.6f;
                    pos.y += Mathf.Sin(t * 0.6f) * 2f + Mathf.Cos(t * 0.3f) * 1f;
                }
                else
                {
                    // Slow circular drift
                    pos.x += Mathf.Sin(t * 0.18f + angleOffset) * orbitRadius * 0.5f;
                    pos.z += Mathf.Cos(t * 0.18f + angleOffset) * orbitRadius * 0.5f;
                    pos.y += Mathf.Sin(t * 0.35f) * 0.8f;
                }
                
                // Pulsing scale with beat reactions
                Vector3 scale = initialTransform.scale * 1.5f; // Make 50% bigger
                if (isFocused && analysisData.IsBeatAt(t, 0.08f))
                {
                    scale *= 1.25f; // Dramatic pulse
                }
                else if (analysisData.IsBeatAt(t, 0.08f))
                {
                    scale *= 1.1f;
                }
                
                AddRotationKeyframes(clip, "", rotation, t);
                AddPositionKeyframes(clip, "", pos, t);
                AddScaleKeyframes(clip, "", scale, t);
            }
            
            FinalizeAllCurves(clip);
        }
        
        private static void AnimateMolecularStructure(AnimationClip clip, ShapeIdentity shape, CastMember castMember,
            int castIndex, RoleBehaviorConfig config, TransformData initialTransform, List<StoryBeat> storyBeats,
            List<MusicSectionData> musicSections, MusicAnalysisData analysisData, MusicVideoSettings settings,
            System.Random random, float timeStep)
        {
            // Atoms vibrate, molecule rotates
            Transform[] atoms = shape.GetShapeElements().Select(r => r.transform).ToArray();
            
            for (float t = 0; t <= analysisData.maxTime; t += timeStep)
            {
                var section = GetSectionAtTime(musicSections, t);
                float intensity = section.intensity * config.energyMultiplier;
                
                // Molecule rotation
                Quaternion rotation = initialTransform.rotation * 
                    Quaternion.Euler(t * 20f, t * 15f, 0);
                
                // Vibrate atoms
                foreach (var atom in atoms.Skip(1)) // Skip center
                {
                    string path = GetRelativePath(castMember.gameObject.transform, atom);
                    Vector3 vibration = atom.localPosition * (1f + Mathf.Sin(t * 10f) * 0.05f * intensity);
                    AddPositionKeyframes(clip, path, vibration, t);
                }
                
                AddRotationKeyframes(clip, "", rotation, t);
                
                float angleOffset = castIndex * 0.7f;
                float orbitRadius = Mathf.Max(settings.stageBounds.x, settings.stageBounds.z) * 0.6f;
                Vector3 parentPos = settings.stageCenter;
                parentPos.x += Mathf.Sin(t * 0.24f + angleOffset) * orbitRadius;
                parentPos.z += Mathf.Cos(t * 0.24f + angleOffset) * orbitRadius;
                parentPos.y += Mathf.Sin(t * 0.34f) * 1.15f;
                
                AddPositionKeyframes(clip, "", parentPos, t);
            }
            
            FinalizeAllCurves(clip);
        }
        
        private static void AnimateTowerStructure(AnimationClip clip, ShapeIdentity shape, CastMember castMember,
            int castIndex, RoleBehaviorConfig config, TransformData initialTransform, List<StoryBeat> storyBeats,
            List<MusicSectionData> musicSections, MusicAnalysisData analysisData, MusicVideoSettings settings,
            System.Random random, float timeStep)
        {
            // Tower sways gently
            for (float t = 0; t <= analysisData.maxTime; t += timeStep)
            {
                var section = GetSectionAtTime(musicSections, t);
                float intensity = section.intensity * config.energyMultiplier;
                
                // Swaying rotation
                float sway = Mathf.Sin(t * 0.8f) * 15f * intensity;
                Quaternion rotation = initialTransform.rotation * Quaternion.Euler(0, 0, sway);
                
                float angleOffset = castIndex * 0.75f;
                float orbitRadius = Mathf.Max(settings.stageBounds.x, settings.stageBounds.z) * 0.65f;
                Vector3 parentPos = settings.stageCenter;
                parentPos.x += Mathf.Cos(t * 0.21f + angleOffset) * orbitRadius;
                parentPos.z += Mathf.Sin(t * 0.21f + angleOffset) * orbitRadius;
                parentPos.y += Mathf.Cos(t * 0.31f) * 1.3f;
                
                AddRotationKeyframes(clip, "", rotation, t);
                AddPositionKeyframes(clip, "", parentPos, t);
            }
            
            FinalizeAllCurves(clip);
        }
        
        private static void AnimateGenericCompound(AnimationClip clip, ShapeIdentity shape, CastMember castMember,
            int castIndex, RoleBehaviorConfig config, TransformData initialTransform, List<StoryBeat> storyBeats,
            List<MusicSectionData> musicSections, MusicAnalysisData analysisData, MusicVideoSettings settings,
            System.Random random, float timeStep)
        {
            // Generic: Wave-like motion with rhythmic pulses - like floating through space
            float waveAmplitude = Mathf.Max(settings.stageBounds.x, settings.stageBounds.z) * 0.7f; // Use stage bounds
            
            for (float t = 0; t <= analysisData.maxTime; t += timeStep)
            {
                var section = GetSectionAtTime(musicSections, t);
                var beat = GetBeatAtTime(storyBeats, t);
                bool isFocused = beat.focusedCastIndices != null && beat.IsCastMemberFocused(castIndex);
                float intensity = section.intensity > 0 ? section.intensity : 0.5f;
                
                // Smooth, elegant rotation with slight variations
                float wobbleX = Mathf.Sin(t * 0.7f) * 15f;
                float wobbleZ = Mathf.Cos(t * 0.9f) * 15f;
                Quaternion rotation = initialTransform.rotation * 
                    Quaternion.Euler(wobbleX + t * 25f, t * 35f, wobbleZ);
                
                // Wave-like movement pattern
                Vector3 pos = settings.stageCenter;
                float angleOffset = castIndex * 0.4f; // Spread cast members around
                if (isFocused)
                {
                    // S-curve path through space
                    pos.x += Mathf.Sin(t * 0.3f + angleOffset) * waveAmplitude;
                    pos.y += Mathf.Sin(t * 0.4f) * waveAmplitude * 0.7f;
                    pos.z += Mathf.Cos(t * 0.3f + angleOffset) * waveAmplitude * 0.8f;
                }
                else
                {
                    // Gentle sine wave drift
                    pos.x += Mathf.Sin(t * 0.2f + angleOffset) * waveAmplitude * 0.4f;
                    pos.y += Mathf.Cos(t * 0.25f) * waveAmplitude * 0.3f;
                    pos.z += Mathf.Sin(t * 0.15f + angleOffset) * waveAmplitude * 0.4f;
                }
                
                // Rhythmic scale pulse
                Vector3 scale = initialTransform.scale * 1.5f; // Make 50% bigger
                float rhythmPulse = 1f + Mathf.Sin(t * 3f) * 0.12f * intensity;
                scale *= rhythmPulse;
                if (analysisData.IsBeatAt(t, 0.08f))
                {
                    scale *= isFocused ? 1.25f : 1.12f;
                }
                
                AddRotationKeyframes(clip, "", rotation, t);
                AddPositionKeyframes(clip, "", pos, t);
                AddScaleKeyframes(clip, "", scale, t);
            }
            
            FinalizeAllCurves(clip);
        }
        
        #endregion
        
        private static void GenerateSimplePrimitiveAnimation(
            AnimationClip clip,
            CastMember castMember,
            int castIndex,
            RoleBehaviorConfig config,
            TransformData initialTransform,
            List<StoryBeat> storyBeats,
            List<MusicSectionData> musicSections,
            MusicAnalysisData analysisData,
            MusicVideoSettings settings,
            System.Random random)
        {
            // EMOTIONALLY-DRIVEN CHOREOGRAPHY - Background cast tells the story through movement
            float timeStep = settings.keyframeInterval;
            
            // Character assignment - each object has a role in the story
            float baseOrbitRadius = Mathf.Max(settings.stageBounds.x, settings.stageBounds.z);
            float characterDepth; // How far from center (0=center, 1=edge)
            float emotionalReactivity; // How strongly they react to story
            float scaleBase;
            
            switch (castMember.role)
            {
                case CastRole.BackgroundSupport:
                    characterDepth = 0.85f; // Distant observers
                    emotionalReactivity = 0.6f; // Subtle reactions
                    scaleBase = 0.5f;
                    break;
                case CastRole.Spotlight:
                    characterDepth = 0.95f; // Frame the story
                    emotionalReactivity = 0.4f; // Steady presence
                    scaleBase = 0.7f;
                    break;
                case CastRole.FeaturedSupport:
                    characterDepth = 0.7f; // Part of the chorus
                    emotionalReactivity = 1.2f; // Highly responsive
                    scaleBase = 0.8f;
                    break;
                default: // Ambient
                    characterDepth = 0.9f; // Atmosphere
                    emotionalReactivity = 0.8f;
                    scaleBase = 0.4f;
                    break;
            }
            
            // Assign each object to a formation group (creates synchronized patterns)
            int groupSize = 8;
            int groupId = castIndex / groupSize;
            int positionInGroup = castIndex % groupSize;
            float groupPhase = groupId * 1.3f; // Each group slightly offset
            
            StoryBeat previousBeat = storyBeats[0];
            
            for (float t = 0; t <= analysisData.maxTime; t += timeStep)
            {
                var section = GetSectionAtTime(musicSections, t);
                var beat = GetBeatAtTime(storyBeats, t);
                
                // Track beat transitions for dramatic moments
                bool momentChanged = beat.moment != previousBeat.moment;
                float timeSinceBeatStart = t - beat.time;
                float beatProgress = Mathf.Clamp01(timeSinceBeatStart / 4f);
                
                float intensity = section.intensity > 0 ? section.intensity : 0.5f;
                float energy = section.energy > 0 ? section.energy : 0.5f;
                
                // EMOTIONAL STATE - drives the entire performance
                Vector3 emotionalTarget = Vector3.zero;
                float emotionalDistance = 1f;
                float emotionalTension = 0f;
                float emotionalScale = 1f;
                float emotionalRotation = 1f;
                Color emotionalColor = Color.white; // Could be used for emission
                
                switch (beat.moment)
                {
                    case StoryMoment.Introduction:
                        // "We are gathering... something is about to begin"
                        // Objects emerge from darkness, slowly revealing themselves
                        float introProgress = Mathf.Min(1f, timeSinceBeatStart / 8f);
                        emotionalDistance = Mathf.Lerp(2f, 1f, Mathf.SmoothStep(0, 1, introProgress));
                        emotionalScale = Mathf.Lerp(0.1f, 1f, Mathf.SmoothStep(0, 1, introProgress));
                        emotionalRotation = 0.3f; // Gentle, curious rotation
                        emotionalTension = 0.1f;
                        
                        // Formation: Scattered, gradually forming rings
                        float scatterAmount = 1f - introProgress;
                        emotionalTarget = new Vector3(
                            Mathf.Sin(groupPhase + positionInGroup * 0.785f) * scatterAmount * 0.5f,
                            0,
                            Mathf.Cos(groupPhase + positionInGroup * 0.785f) * scatterAmount * 0.5f
                        );
                        break;
                        
                    case StoryMoment.Rising:
                        // "Tension builds... we draw closer together"
                        // Objects contract inward, circling, building anticipation
                        emotionalDistance = 0.9f - energy * 0.3f; // Pull closer as energy builds
                        emotionalTension = energy * 0.6f;
                        emotionalRotation = 1f + energy * 2f; // Speed increases
                        emotionalScale = 1f + Mathf.Sin(t * 2f) * energy * 0.2f; // Nervous pulsing
                        
                        // Formation: Tight spiral inward
                        float spiralTightness = energy;
                        float spiralAngle = t * (1f + energy) + positionInGroup * 0.5f;
                        emotionalTarget = new Vector3(
                            Mathf.Sin(spiralAngle) * (1f - spiralTightness * 0.4f),
                            Mathf.Sin(t * 1.5f) * spiralTightness * 0.3f,
                            Mathf.Cos(spiralAngle) * (1f - spiralTightness * 0.4f)
                        );
                        break;
                        
                    case StoryMoment.Conflict:
                        // "CHAOS! Everything breaks apart!"
                        // Violent, erratic movement - objects fight against each other
                        float chaosPhase = t * 3f + castIndex * 0.3f;
                        emotionalDistance = 1.2f + Mathf.Sin(chaosPhase) * 0.5f; // Pulsing in/out violently
                        emotionalTension = 1f; // Maximum stress
                        emotionalRotation = 3f + Mathf.Sin(t * 2f) * 2f; // Chaotic spinning
                        emotionalScale = 1f + Mathf.Sin(chaosPhase * 2f) * 0.4f; // Erratic scaling
                        
                        // Formation: BROKEN - each object moves independently
                        emotionalTarget = new Vector3(
                            Mathf.Sin(chaosPhase * 1.3f + positionInGroup) * 0.6f,
                            Mathf.Sin(chaosPhase * 2.1f) * 0.8f,
                            Mathf.Cos(chaosPhase * 1.7f - positionInGroup) * 0.6f
                        );
                        break;
                        
                    case StoryMoment.Climax:
                        // "THE PEAK! EVERYTHING EXPLODES OUTWARD!"
                        // Massive expansion, objects thrown to the edges
                        float climaxForce = intensity * 2f;
                        emotionalDistance = 1.5f + climaxForce * 0.8f; // BLAST OUTWARD
                        emotionalTension = 2f; // Beyond maximum
                        emotionalRotation = 5f + intensity * 3f; // SPINNING WILDLY
                        emotionalScale = 1.8f + intensity * 0.7f; // HUGE
                        
                        // Formation: Explosive radial burst
                        float burstAngle = positionInGroup * 0.785f + groupPhase;
                        float burstPower = 1f + intensity;
                        emotionalTarget = new Vector3(
                            Mathf.Sin(burstAngle) * burstPower,
                            Mathf.Sin(t * 3f + positionInGroup * 0.5f) * climaxForce * 0.4f,
                            Mathf.Cos(burstAngle) * burstPower
                        );
                        
                        // Pulse on beats during climax
                        if (analysisData.IsBeatAt(t, 0.1f))
                        {
                            emotionalScale *= 1.5f;
                            emotionalDistance *= 1.3f;
                        }
                        break;
                        
                    case StoryMoment.Resolution:
                        // "Exhale... we slowly return to peace"
                        // Calming, flowing back into order
                        float resolutionCalm = Mathf.Min(1f, timeSinceBeatStart / 6f);
                        emotionalDistance = Mathf.Lerp(1.5f, 1f, Mathf.SmoothStep(0, 1, resolutionCalm));
                        emotionalTension = Mathf.Lerp(1f, 0.2f, resolutionCalm);
                        emotionalRotation = Mathf.Lerp(2f, 0.5f, resolutionCalm);
                        emotionalScale = Mathf.Lerp(1.5f, 1f, Mathf.SmoothStep(0, 1, resolutionCalm));
                        
                        // Formation: Gently swaying in rings
                        float swayAmount = 0.2f * (1f - resolutionCalm);
                        emotionalTarget = new Vector3(
                            Mathf.Sin(t * 0.3f + positionInGroup * 0.785f) * swayAmount,
                            Mathf.Sin(t * 0.4f) * 0.3f,
                            Mathf.Cos(t * 0.3f + positionInGroup * 0.785f) * swayAmount
                        );
                        break;
                        
                    case StoryMoment.Finale:
                        // "Goodbye... we fade into the distance"
                        // Grand sweeping exit, objects drift away gracefully
                        float exitProgress = timeSinceBeatStart / 6f;
                        emotionalDistance = 1f + exitProgress * 1.5f; // Drift away
                        emotionalScale = Mathf.Lerp(1f, 0.2f, Mathf.SmoothStep(0, 1, exitProgress)); // Shrink
                        emotionalRotation = 0.8f - exitProgress * 0.5f; // Slow down
                        emotionalTension = 0f;
                        
                        // Formation: Spiral outward and fade
                        float exitAngle = t * 0.4f + positionInGroup * 0.785f;
                        float exitRadius = 1f + exitProgress * 2f;
                        emotionalTarget = new Vector3(
                            Mathf.Sin(exitAngle) * exitRadius,
                            Mathf.Lerp(0f, -5f, exitProgress), // Descend
                            Mathf.Cos(exitAngle) * exitRadius
                        );
                        break;
                }
                
                // Apply emotional reactivity based on character role
                emotionalDistance = Mathf.Lerp(1f, emotionalDistance, emotionalReactivity);
                emotionalScale = Mathf.Lerp(1f, emotionalScale, emotionalReactivity);
                emotionalTension = emotionalTension * emotionalReactivity;
                
                // POSITION - Combine formation target with emotional distance
                float currentRadius = baseOrbitRadius * characterDepth * emotionalDistance;
                float orbitAngle = t * emotionalRotation * 0.2f + castIndex * 0.6f + groupPhase;
                
                Vector3 pos = settings.stageCenter;
                pos += emotionalTarget * baseOrbitRadius * 0.3f; // Formation offset
                pos.x += Mathf.Cos(orbitAngle) * currentRadius;
                pos.z += Mathf.Sin(orbitAngle) * currentRadius;
                
                // Musical beat reactions - quick pulses
                if (analysisData.IsBeatAt(t, 0.1f))
                {
                    float beatReaction = intensity * emotionalReactivity * 0.15f;
                    pos += new Vector3(
                        Mathf.Sin(orbitAngle) * baseOrbitRadius * beatReaction,
                        baseOrbitRadius * beatReaction * 0.5f,
                        Mathf.Cos(orbitAngle) * baseOrbitRadius * beatReaction
                    );
                }
                
                if (settings.constrainMovement)
                {
                    pos = ClampToBounds(pos, settings.stageCenter, settings.movementBounds);
                }
                
                // ROTATION - Emotional tension affects spin
                float rotSpeed = emotionalRotation * (30f + emotionalTension * 50f);
                Quaternion rotation = initialTransform.rotation * Quaternion.Euler(
                    t * rotSpeed * 0.7f + Mathf.Sin(t * emotionalTension * 2f) * 30f * emotionalTension,
                    t * rotSpeed,
                    t * rotSpeed * 0.4f + Mathf.Cos(t * emotionalTension * 1.5f) * 20f * emotionalTension
                );
                
                // SCALE - Size reflects emotional intensity
                Vector3 scale = initialTransform.scale * scaleBase * emotionalScale;
                
                // Breathing - constant subtle pulse
                scale *= 1f + Mathf.Sin(t * 1.5f + castIndex * 0.3f) * 0.08f;
                
                AddRotationKeyframes(clip, "", rotation, t);
                AddPositionKeyframes(clip, "", pos, t);
                AddScaleKeyframes(clip, "", scale, t);
                
                previousBeat = beat;
            }
            
            FinalizeAllCurves(clip);
        }
        
        #region Helper Methods
        
        private static string GetRelativePath(Transform root, Transform child)
        {
            if (root == child) return "";
            
            var path = child.name;
            var parent = child.parent;
            
            while (parent != null && parent != root)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }
            
            return path;
        }
        
        private static Dictionary<string, AnimationCurve> curveCache = new Dictionary<string, AnimationCurve>();
        
        private static void AddPositionKeyframes(AnimationClip clip, string path, Vector3 position, float time)
        {
            GetOrCreateCurve(clip, path, "localPosition.x").AddKey(time, position.x);
            GetOrCreateCurve(clip, path, "localPosition.y").AddKey(time, position.y);
            GetOrCreateCurve(clip, path, "localPosition.z").AddKey(time, position.z);
        }
        
        private static void AddRotationKeyframes(AnimationClip clip, string path, Quaternion rotation, float time)
        {
            GetOrCreateCurve(clip, path, "localRotation.x").AddKey(time, rotation.x);
            GetOrCreateCurve(clip, path, "localRotation.y").AddKey(time, rotation.y);
            GetOrCreateCurve(clip, path, "localRotation.z").AddKey(time, rotation.z);
            GetOrCreateCurve(clip, path, "localRotation.w").AddKey(time, rotation.w);
        }
        
        private static void AddScaleKeyframes(AnimationClip clip, string path, Vector3 scale, float time)
        {
            GetOrCreateCurve(clip, path, "localScale.x").AddKey(time, scale.x);
            GetOrCreateCurve(clip, path, "localScale.y").AddKey(time, scale.y);
            GetOrCreateCurve(clip, path, "localScale.z").AddKey(time, scale.z);
        }
        
        private static AnimationCurve GetOrCreateCurve(AnimationClip clip, string path, string property)
        {
            string key = $"{clip.name}|||{path}|||{property}";
            
            if (!curveCache.ContainsKey(key))
            {
                curveCache[key] = new AnimationCurve();
            }
            
            return curveCache[key];
        }
        
        private static void FinalizeAllCurves(AnimationClip clip)
        {
            foreach (var kvp in curveCache)
            {
                string[] parts = kvp.Key.Split(new[] { "|||" }, System.StringSplitOptions.None);
                if (parts.Length == 3 && parts[0] == clip.name)
                {
                    string path = parts[1];
                    string property = parts[2];
                    clip.SetCurve(path, typeof(Transform), property, kvp.Value);
                }
            }
            
            curveCache.Clear();
        }
        
        private static MusicSectionData GetSectionAtTime(List<MusicSectionData> sections, float time)
        {
            return sections.FirstOrDefault(s => time >= s.startTime && time < s.endTime);
        }
        
        private static StoryBeat GetBeatAtTime(List<StoryBeat> beats, float time)
        {
            return beats.OrderBy(b => Mathf.Abs(b.time - time)).FirstOrDefault();
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
        
        #endregion
    }
}
