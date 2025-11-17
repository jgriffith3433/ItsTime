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
        /// Remove all colliders from cast member GameObjects to improve performance.
        /// </summary>
        [ContextMenu("Remove All Colliders from Cast")]
        private void RemoveAllColliders()
        {
            int colliderCount = 0;
            
            // Get all colliders in children
            Collider[] colliders = GetComponentsInChildren<Collider>(true);
            
            foreach (Collider collider in colliders)
            {
                // Don't destroy colliders on the cast root itself
                if (collider.gameObject != gameObject)
                {
                    #if UNITY_EDITOR
                    UnityEditor.Undo.DestroyObjectImmediate(collider);
                    #else
                    Destroy(collider);
                    #endif
                    colliderCount++;
                }
            }
            
            Debug.Log($"[MusicVideoCast] Removed {colliderCount} colliders from cast members");
        }

        /// <summary>
        /// Find all children with the Name "Sphere" and get their material and increase the alpha on the Albedo + .1
        /// </summary>
        [ContextMenu("Increase Alpha on 'Sphere' Materials")]
        private void IncreaseAlphaOnSphereMaterials()
        {
            int modifiedCount = 0;
            
            // Get all renderers in children
            Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
            
            foreach (Renderer renderer in renderers)
            {
                if (renderer.gameObject.name.Contains("Sphere"))
                {
                    Material mat = renderer.sharedMaterial;
                    if (mat != null && mat.HasProperty("_Color"))
                    {
                        Color color = mat.color;
                        color.a = Mathf.Clamp01(color.a + 0.1f);
                        mat.color = color;
                        modifiedCount++;
                    }
                }
            }
            
            Debug.Log($"[MusicVideoCast] Increased alpha on {modifiedCount} 'Sphere' materials");
        }

        /// <summary>
        /// Find all children with the Name "Sphere" and get their material and decrease the alpha on the Albedo - .1
        /// </summary>
        [ContextMenu("Decrease Alpha on 'Sphere' Materials")]
        private void DecreaseAlphaOnSphereMaterials()
        {
            int modifiedCount = 0;
            
            // Get all renderers in children
            Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
            
            foreach (Renderer renderer in renderers)
            {
                if (renderer.gameObject.name.Contains("Sphere"))
                {
                    Material mat = renderer.sharedMaterial;
                    if (mat != null && mat.HasProperty("_Color"))
                    {
                        Color color = mat.color;
                        color.a = Mathf.Clamp01(color.a - 0.1f);
                        mat.color = color;
                        modifiedCount++;
                    }
                }
            }
            
            Debug.Log($"[MusicVideoCast] Decreased alpha on {modifiedCount} 'Sphere' materials");
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
        
        [ContextMenu("Add LODGroup children")]
        private void AddLODGroups()
        {
            // Clear all cast members
            // mainCast.Clear();
            // supportingCast.Clear();
            // spotlightCast.Clear();
            
            // Find all LODGroup child components
            LODGroup[] lodGroups = GetComponentsInChildren<LODGroup>(); 
            
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
                // check if already assigned
                if (mainCast.Any(m => m.gameObject == lodGroups[index].gameObject))
                {
                    continue; // Skip if already in main cast
                }
                mainCast.Add(new CastMember
                {
                    gameObject = lodGroups[index].gameObject,
                    role = CastRole.Ambient
                });
            }
            
            // Assign to supporting cast
            for (int i = 0; i < supportingCastCount && index < totalGroups; i++, index++)
            {
                // check if already assigned
                if (supportingCast.Any(m => m.gameObject == lodGroups[index].gameObject))
                {
                    continue; // Skip if already in supporting cast
                }
                supportingCast.Add(new CastMember
                {
                    gameObject = lodGroups[index].gameObject,
                    role = CastRole.Ambient
                });
            }
            
            // Assign remaining to spotlight cast
            while (index < totalGroups)
            {
                // check if already assigned
                if (spotlightCast.Any(m => m.gameObject == lodGroups[index].gameObject))
                {
                    index++;
                    continue; // Skip if already in spotlight cast
                }
                spotlightCast.Add(new CastMember
                {
                    gameObject = lodGroups[index].gameObject,
                    role = CastRole.Ambient
                });
                index++;
            }
            
            Debug.Log($"[MusicVideoCast] Auto-populated from {totalGroups} LODGroups: {mainCast.Count} main, {supportingCast.Count} supporting, {spotlightCast.Count} spotlight - all with Ambient role");
        }
        
        [ContextMenu("🎨 Generate Complete Scene from Primitives")]
        private void GenerateCompleteSceneFromPrimitives()
        {
#if UNITY_EDITOR
            // Clear existing cast
            ClearAllCast();
            
            System.Random rng = new System.Random();
            
            // Theme selection - randomly choose a visual theme
            string[] themes = { "Cosmic", "Neon City", "Underwater", "Crystal Garden", "Abstract Geometry" };
            string selectedTheme = themes[rng.Next(themes.Length)];
            Debug.Log($"[MusicVideoCast] 🎨 Generating '{selectedTheme}' themed music video scene...");
            
            // Generate Lead Stars (3-5 prominent objects)
            int leadCount = rng.Next(3, 6);
            for (int i = 0; i < leadCount; i++)
            {
                GameObject lead = CreateThematicObject(selectedTheme, "LeadStar", i, transform, rng, true);
                
                // Mark all children as ambient so they don't get animated independently
                MarkChildrenAsAmbient(lead.transform, selectedTheme, rng);
                
                mainCast.Add(new CastMember
                {
                    gameObject = lead,
                    role = CastRole.LeadStar,
                    useCustomColors = true,
                    customPrimaryColor = GetThemeColor(selectedTheme, 0, rng),
                    customSecondaryColor = GetThemeColor(selectedTheme, 1, rng)
                });
            }
            
            // Generate Co-Stars (5-8 secondary focal points)
            int coStarCount = rng.Next(5, 9);
            for (int i = 0; i < coStarCount; i++)
            {
                GameObject coStar = CreateThematicObject(selectedTheme, "CoStar", i, transform, rng, true);
                
                // Mark all children as ambient so they don't get animated independently
                MarkChildrenAsAmbient(coStar.transform, selectedTheme, rng);
                
                mainCast.Add(new CastMember
                {
                    gameObject = coStar,
                    role = CastRole.CoStar,
                    useCustomColors = true,
                    customPrimaryColor = GetThemeColor(selectedTheme, 2, rng),
                    customSecondaryColor = GetThemeColor(selectedTheme, 3, rng)
                });
            }
            
            // Generate Featured Support (8-12 objects)
            int featuredCount = rng.Next(8, 13);
            for (int i = 0; i < featuredCount; i++)
            {
                GameObject featured = CreateThematicObject(selectedTheme, "Featured", i, transform, rng, false);
                
                // Mark all children as ambient so they don't get animated independently
                MarkChildrenAsAmbient(featured.transform, selectedTheme, rng);
                
                mainCast.Add(new CastMember
                {
                    gameObject = featured,
                    role = CastRole.FeaturedSupport,
                    useCustomColors = true,
                    customPrimaryColor = GetThemeColor(selectedTheme, 4, rng),
                    customSecondaryColor = GetThemeColor(selectedTheme, 5, rng)
                });
            }
            
            // Generate Background Support (20-30 objects forming environment)
            int backgroundCount = rng.Next(20, 31);
            for (int i = 0; i < backgroundCount; i++)
            {
                GameObject background = CreateBackgroundObject(selectedTheme, i, transform, rng);
                
                // Mark all children as ambient so they don't get animated independently
                MarkChildrenAsAmbient(background.transform, selectedTheme, rng);
                
                supportingCast.Add(new CastMember
                {
                    gameObject = background,
                    role = CastRole.BackgroundSupport,
                    useCustomColors = true,
                    customPrimaryColor = GetThemeColor(selectedTheme, 6, rng),
                    customSecondaryColor = GetThemeColor(selectedTheme, 7, rng)
                });
            }
            
            // Generate Ambient atmosphere elements (30-50 subtle elements)
            int ambientCount = rng.Next(30, 51);
            for (int i = 0; i < ambientCount; i++)
            {
                GameObject ambient = CreateAmbientObject(selectedTheme, i, transform, rng);
                
                // Mark all children as ambient so they don't get animated independently
                MarkChildrenAsAmbient(ambient.transform, selectedTheme, rng);
                
                supportingCast.Add(new CastMember
                {
                    gameObject = ambient,
                    role = CastRole.Ambient,
                    useCustomColors = true,
                    customPrimaryColor = GetThemeColor(selectedTheme, 8, rng),
                    customSecondaryColor = GetThemeColor(selectedTheme, 9, rng),
                    animationDelay = (float)rng.NextDouble() * 2f
                });
            }
            
            // Generate Spotlight special effects (8-12 dramatic elements)
            int spotlightCount = rng.Next(8, 13);
            for (int i = 0; i < spotlightCount; i++)
            {
                GameObject spotlight = CreateSpotlightObject(selectedTheme, i, transform, rng);
                
                // Mark all children as ambient so they don't get animated independently
                MarkChildrenAsAmbient(spotlight.transform, selectedTheme, rng);
                
                spotlightCast.Add(new CastMember
                {
                    gameObject = spotlight,
                    role = CastRole.Spotlight,
                    useCustomColors = true,
                    customPrimaryColor = Color.white,
                    customSecondaryColor = GetThemeColor(selectedTheme, 10, rng),
                    animationDelay = (float)rng.NextDouble() * 3f
                });
            }
            
            int totalObjects = mainCast.Count + supportingCast.Count + spotlightCast.Count;
            Debug.Log($"[MusicVideoCast] ✨ Generated complete '{selectedTheme}' scene with {totalObjects} objects!");
            Debug.Log($"  - Main Cast: {mainCast.Count} ({leadCount} leads, {coStarCount} co-stars, {featuredCount} featured)");
            Debug.Log($"  - Supporting Cast: {supportingCast.Count} ({backgroundCount} background, {ambientCount} ambient)");
            Debug.Log($"  - Spotlight Cast: {spotlightCast.Count}");
            
            UnityEditor.Selection.activeGameObject = gameObject;
#endif
        }
        
#if UNITY_EDITOR
        private GameObject CreateThematicObject(string theme, string roleName, int index, Transform parent, System.Random rng, bool isLarge)
        {
            // Create parent container for compound shape
            GameObject obj = new GameObject($"{theme}_{roleName}_{index}");
            obj.transform.SetParent(parent);
            
            // Position based on role - leads are centered, others distributed
            float radius = isLarge ? 5f : 10f;
            float angle = (float)(rng.NextDouble() * Mathf.PI * 2f);
            float distance = (float)(rng.NextDouble() * radius);
            float height = isLarge ? (float)(rng.NextDouble() * 3f) : (float)(rng.NextDouble() * 6f - 3f);
            
            obj.transform.localPosition = new Vector3(
                Mathf.Cos(angle) * distance,
                height,
                Mathf.Sin(angle) * distance
            );
            
            // Random rotation
            obj.transform.localRotation = Quaternion.Euler(
                (float)(rng.NextDouble() * 360),
                (float)(rng.NextDouble() * 360),
                (float)(rng.NextDouble() * 360)
            );
            
            // Scale based on importance
            float scale = isLarge ? (float)(rng.NextDouble() * 0.3 + 0.3) : (float)(rng.NextDouble() * 0.2 + 0.15);
            obj.transform.localScale = Vector3.one * scale;
            
            // Create compound shape - choose a structure type
            int shapeType = rng.Next(20);
            Color baseColor = GetThemeColor(theme, index, rng);
            
            // Add ShapeIdentity component to track shape type
            ShapeIdentity identity = obj.AddComponent<ShapeIdentity>();
            identity.originalScale = obj.transform.localScale;
            identity.primaryColor = baseColor;
            identity.animationSpeed = (float)(rng.NextDouble() * 0.5 + 0.5);
            
            switch (shapeType)
            {
                case 0: // Orbital system - core with orbiting satellites
                    identity.shapeType = ShapeType.OrbitalSystem;
                    identity.rotationAxis = Vector3.up;
                    CreateOrbitalSystem(obj.transform, scale, baseColor, rng, identity);
                    break;
                case 1: // Crystal cluster
                    identity.shapeType = ShapeType.CrystalCluster;
                    CreateCrystalCluster(obj.transform, scale, baseColor, rng, identity);
                    break;
                case 2: // Tower/Totem
                    identity.shapeType = ShapeType.Tower;
                    identity.rotationAxis = Vector3.up;
                    CreateTowerStructure(obj.transform, scale, baseColor, rng, identity);
                    break;
                case 3: // Ring structure
                    identity.shapeType = ShapeType.Ring;
                    CreateRingStructure(obj.transform, scale, baseColor, rng, identity);
                    break;
                case 4: // Flower/Star burst
                    identity.shapeType = ShapeType.StarBurst;
                    identity.rotationAxis = Vector3.up;
                    CreateStarBurst(obj.transform, scale, baseColor, rng, identity);
                    break;
                case 5: // DNA helix
                    identity.shapeType = ShapeType.Helix;
                    identity.rotationAxis = Vector3.up;
                    CreateHelixStructure(obj.transform, scale, baseColor, rng, identity);
                    break;
                case 6: // Geometric lattice
                    identity.shapeType = ShapeType.Lattice;
                    CreateLatticeStructure(obj.transform, scale, baseColor, rng, identity);
                    break;
                case 7: // Abstract sculpture
                    identity.shapeType = ShapeType.AbstractSculpture;
                    CreateAbstractSculpture(obj.transform, scale, baseColor, rng, identity);
                    break;
                case 8: // Spiraling galaxy
                    identity.shapeType = ShapeType.SpiralGalaxy;
                    identity.rotationAxis = Vector3.up;
                    CreateSpiralGalaxy(obj.transform, scale, baseColor, rng, identity);
                    break;
                case 9: // Molecular structure
                    identity.shapeType = ShapeType.Molecular;
                    CreateMolecularStructure(obj.transform, scale, baseColor, rng, identity);
                    break;
                case 10: // Fractal tree
                    identity.shapeType = ShapeType.FractalTree;
                    identity.rotationAxis = Vector3.up;
                    CreateFractalTree(obj.transform, scale, baseColor, rng, 0, 3, identity);
                    break;
                case 11: // Gyroscope/Armillary
                    identity.shapeType = ShapeType.Gyroscope;
                    CreateGyroscope(obj.transform, scale, baseColor, rng, identity);
                    break;
                case 12: // Mandala pattern
                    identity.shapeType = ShapeType.Mandala;
                    identity.rotationAxis = Vector3.forward;
                    CreateMandala(obj.transform, scale, baseColor, rng, identity);
                    break;
                case 13: // Atom model
                    identity.shapeType = ShapeType.Atom;
                    CreateAtomModel(obj.transform, scale, baseColor, rng, identity);
                    break;
                case 14: // Geometric flower
                    identity.shapeType = ShapeType.GeometricFlower;
                    identity.rotationAxis = Vector3.up;
                    CreateGeometricFlower(obj.transform, scale, baseColor, rng, identity);
                    break;
                case 15: // Wireframe polyhedron
                    identity.shapeType = ShapeType.WireframePolyhedron;
                    CreateWireframePolyhedron(obj.transform, scale, baseColor, rng, identity);
                    break;
                case 16: // Spinning top/Torus
                    identity.shapeType = ShapeType.Torus;
                    identity.rotationAxis = Vector3.up;
                    CreateTorusStructure(obj.transform, scale, baseColor, rng, identity);
                    break;
                case 17: // Chain links
                    identity.shapeType = ShapeType.ChainLinks;
                    CreateChainLinks(obj.transform, scale, baseColor, rng, identity);
                    break;
                case 18: // Pyramid array
                    identity.shapeType = ShapeType.PyramidArray;
                    identity.rotationAxis = Vector3.up;
                    CreatePyramidArray(obj.transform, scale, baseColor, rng, identity);
                    break;
                case 19: // Fibonacci spiral
                    identity.shapeType = ShapeType.FibonacciSpiral;
                    identity.rotationAxis = Vector3.up;
                    CreateFibonacciSpiral(obj.transform, scale, baseColor, rng, identity);
                    break;
            }
            
            return obj;
        }
        
        private void CreateOrbitalSystem(Transform parent, float scale, Color color, System.Random rng, ShapeIdentity identity)
        {
            // Central core
            AddPrimitive(PrimitiveType.Sphere, parent, Vector3.zero, scale, color, rng);
            
            // Orbiting satellites
            int satellites = rng.Next(3, 6);
            identity.elementCount = satellites;
            identity.shapeRadius = scale * 2.5f;
            
            for (int i = 0; i < satellites; i++)
            {
                float angle = (i / (float)satellites) * Mathf.PI * 2f;
                float orbitRadius = scale * 2.5f;
                Vector3 pos = new Vector3(Mathf.Cos(angle) * orbitRadius, 0, Mathf.Sin(angle) * orbitRadius);
                AddPrimitive(PrimitiveType.Sphere, parent, pos, scale * 0.4f, color * 0.8f, rng);
            }
        }
        
        private void CreateCrystalCluster(Transform parent, float scale, Color color, System.Random rng, ShapeIdentity identity)
        {
            // Multiple shards pointing outward
            int crystals = rng.Next(5, 9);
            identity.elementCount = crystals;
            identity.shapeRadius = scale * 0.5f;
            identity.shapeHeight = scale * 1.5f;
            
            for (int i = 0; i < crystals; i++)
            {
                float angle = (float)(rng.NextDouble() * Mathf.PI * 2f);
                float tilt = (float)(rng.NextDouble() * 60f - 30f);
                Vector3 pos = new Vector3(
                    Mathf.Cos(angle) * scale * 0.5f,
                    (float)(rng.NextDouble() * scale * 0.3f),
                    Mathf.Sin(angle) * scale * 0.5f
                );
                
                GameObject crystal = AddPrimitive(PrimitiveType.Cylinder, parent, pos, scale * 0.6f, color, rng);
                crystal.transform.localScale = new Vector3(scale * 0.3f, scale * 1.5f, scale * 0.3f);
                crystal.transform.localRotation = Quaternion.Euler(tilt, angle * Mathf.Rad2Deg, 0);
            }
        }
        
        private void CreateTowerStructure(Transform parent, float scale, Color color, System.Random rng, ShapeIdentity identity)
        {
            // Stacked segments
            int segments = rng.Next(4, 7);
            identity.elementCount = segments;
            identity.shapeHeight = scale * 3f;
            
            PrimitiveType[] types = { PrimitiveType.Cube, PrimitiveType.Cylinder, PrimitiveType.Capsule };
            
            for (int i = 0; i < segments; i++)
            {
                float heightOffset = i * scale * 0.8f;
                float segmentScale = scale * (1f - i * 0.1f);
                Vector3 pos = new Vector3(0, heightOffset, 0);
                AddPrimitive(types[rng.Next(types.Length)], parent, pos, segmentScale, color * (1f - i * 0.1f), rng);
            }
        }
        
        private void CreateRingStructure(Transform parent, float scale, Color color, System.Random rng, ShapeIdentity identity)
        {
            // Ring of objects
            int count = rng.Next(8, 13);
            identity.elementCount = count;
            identity.shapeRadius = scale * 2f;
            for (int i = 0; i < count; i++)
            {
                float angle = (i / (float)count) * Mathf.PI * 2f;
                float radius = scale * 2f;
                Vector3 pos = new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
                AddPrimitive(PrimitiveType.Cube, parent, pos, scale * 0.3f, color, rng);
            }
        }
        
        private void CreateStarBurst(Transform parent, float scale, Color color, System.Random rng, ShapeIdentity identity)
        {
            // Central sphere with radiating spikes
            AddPrimitive(PrimitiveType.Sphere, parent, Vector3.zero, scale, color, rng);
            
            int spikes = rng.Next(6, 10);
            identity.elementCount = spikes;
            identity.shapeRadius = scale * 1.5f;
            for (int i = 0; i < spikes; i++)
            {
                float angle = (float)(rng.NextDouble() * Mathf.PI * 2f);
                float elevation = (float)(rng.NextDouble() * Mathf.PI - Mathf.PI / 2f);
                Vector3 direction = new Vector3(
                    Mathf.Cos(angle) * Mathf.Cos(elevation),
                    Mathf.Sin(elevation),
                    Mathf.Sin(angle) * Mathf.Cos(elevation)
                );
                
                GameObject spike = AddPrimitive(PrimitiveType.Cylinder, parent, direction * scale * 1.5f, scale * 0.5f, color * 0.9f, rng);
                spike.transform.localScale = new Vector3(scale * 0.15f, scale * 0.8f, scale * 0.15f);
                spike.transform.localRotation = Quaternion.LookRotation(direction);
            }
        }
        
        private void CreateHelixStructure(Transform parent, float scale, Color color, System.Random rng, ShapeIdentity identity)
        {
            // Double helix
            int segments = 15;
            identity.elementCount = segments * 2;
            identity.shapeHeight = scale * 3f;
            identity.shapeRadius = scale * 0.8f;
            
            Color color2 = GetThemeColor("Secondary", rng.Next(100), rng);
            
            for (int i = 0; i < segments; i++)
            {
                float t = i / (float)segments;
                float angle = t * Mathf.PI * 4f;
                float height = (t - 0.5f) * scale * 3f;
                float radius = scale * 0.8f;
                
                // First strand
                Vector3 pos1 = new Vector3(Mathf.Cos(angle) * radius, height, Mathf.Sin(angle) * radius);
                AddPrimitive(PrimitiveType.Sphere, parent, pos1, scale * 0.25f, color, rng);
                
                // Second strand (offset)
                Vector3 pos2 = new Vector3(Mathf.Cos(angle + Mathf.PI) * radius, height, Mathf.Sin(angle + Mathf.PI) * radius);
                AddPrimitive(PrimitiveType.Sphere, parent, pos2, scale * 0.25f, color2, rng);
            }
        }
        
        private void CreateLatticeStructure(Transform parent, float scale, Color color, System.Random rng, ShapeIdentity identity)
        {
            // 3D grid of cubes
            int gridSize = 3;
            identity.elementCount = gridSize * gridSize * gridSize;
            identity.shapeRadius = scale * 1.5f;
            
            for (int x = 0; x < gridSize; x++)
            {
                for (int y = 0; y < gridSize; y++)
                {
                    for (int z = 0; z < gridSize; z++)
                    {
                        if (rng.NextDouble() > 0.3) // Randomly skip some
                        {
                            Vector3 pos = new Vector3(
                                (x - gridSize / 2f) * scale * 0.8f,
                                (y - gridSize / 2f) * scale * 0.8f,
                                (z - gridSize / 2f) * scale * 0.8f
                            );
                            AddPrimitive(PrimitiveType.Cube, parent, pos, scale * 0.3f, color, rng);
                        }
                    }
                }
            }
        }
        
        private void CreateAbstractSculpture(Transform parent, float scale, Color color, System.Random rng, ShapeIdentity identity)
        {
            // Random artistic arrangement
            int pieces = rng.Next(5, 9);
            identity.elementCount = pieces;
            identity.shapeRadius = scale * 2f;
            PrimitiveType[] types = { PrimitiveType.Sphere, PrimitiveType.Cube, PrimitiveType.Cylinder, PrimitiveType.Capsule };
            
            for (int i = 0; i < pieces; i++)
            {
                Vector3 pos = new Vector3(
                    (float)(rng.NextDouble() * scale * 2 - scale),
                    (float)(rng.NextDouble() * scale * 2 - scale),
                    (float)(rng.NextDouble() * scale * 2 - scale)
                );
                
                float pieceScale = (float)(rng.NextDouble() * scale * 0.5 + scale * 0.3);
                AddPrimitive(types[rng.Next(types.Length)], parent, pos, pieceScale, color * (float)(rng.NextDouble() * 0.4 + 0.6), rng);
            }
        }
        
        private void CreateSpiralGalaxy(Transform parent, float scale, Color color, System.Random rng, ShapeIdentity identity)
        {
            // Spiral arms with particles
            int arms = rng.Next(2, 4);
            int particlesPerArm = 12;
            identity.elementCount = arms * particlesPerArm;
            identity.shapeRadius = scale * 2.5f;
            identity.shapeHeight = scale * 0.3f;
            
            for (int arm = 0; arm < arms; arm++)
            {
                float armOffset = (arm / (float)arms) * Mathf.PI * 2f;
                for (int i = 0; i < particlesPerArm; i++)
                {
                    float t = i / (float)particlesPerArm;
                    float angle = armOffset + t * Mathf.PI * 3f;
                    float radius = t * scale * 2.5f;
                    float height = (float)(rng.NextDouble() * scale * 0.3 - scale * 0.15);
                    
                    Vector3 pos = new Vector3(
                        Mathf.Cos(angle) * radius,
                        height,
                        Mathf.Sin(angle) * radius
                    );
                    
                    float particleSize = scale * 0.15f * (1f - t * 0.5f);
                    AddPrimitive(PrimitiveType.Sphere, parent, pos, particleSize, color * (1f - t * 0.3f), rng);
                }
            }
        }
        
        private void CreateMolecularStructure(Transform parent, float scale, Color color, System.Random rng, ShapeIdentity identity)
        {
            // Center atom
            AddPrimitive(PrimitiveType.Sphere, parent, Vector3.zero, scale * 0.6f, color, rng);
            
            // Connected atoms with bonds
            int atoms = rng.Next(4, 7);
            identity.elementCount = atoms + 1;
            identity.shapeRadius = scale * 2f;
            for (int i = 0; i < atoms; i++)
            {
                float angle = (i / (float)atoms) * Mathf.PI * 2f;
                float elevation = (float)(rng.NextDouble() * Mathf.PI - Mathf.PI / 2f) * 0.5f;
                float bondLength = scale * 2f;
                
                Vector3 direction = new Vector3(
                    Mathf.Cos(angle) * Mathf.Cos(elevation),
                    Mathf.Sin(elevation),
                    Mathf.Sin(angle) * Mathf.Cos(elevation)
                );
                
                // Bond (cylinder)
                GameObject bond = AddPrimitive(PrimitiveType.Cylinder, parent, direction * bondLength * 0.5f, scale * 0.5f, color * 0.7f, rng);
                bond.transform.localScale = new Vector3(scale * 0.1f, bondLength * 0.5f, scale * 0.1f);
                bond.transform.localRotation = Quaternion.FromToRotation(Vector3.up, direction);
                
                // Atom at end
                Color atomColor = GetThemeColor("Atom", i, rng);
                AddPrimitive(PrimitiveType.Sphere, parent, direction * bondLength, scale * 0.5f, atomColor, rng);
            }
        }
        
        private void CreateFractalTree(Transform parent, float scale, Color color, System.Random rng, int depth, int maxDepth, ShapeIdentity identity)
        {
            if (depth >= maxDepth) return;
            
            // Trunk/branch
            GameObject branch = AddPrimitive(PrimitiveType.Cylinder, parent, Vector3.up * scale * 0.5f, scale, color * (1f - depth * 0.15f), rng);
            branch.transform.localScale = new Vector3(scale * 0.15f, scale * 0.5f, scale * 0.15f);
            
            // Create sub-branches
            int branches = depth == 0 ? rng.Next(3, 5) : rng.Next(2, 4);
            if (depth == 0)
            {
                identity.elementCount = branches;
                identity.shapeHeight = scale * maxDepth;
            }
            for (int i = 0; i < branches; i++)
            {
                GameObject branchPoint = new GameObject($"Branch_{depth}_{i}");
                branchPoint.transform.SetParent(parent);
                branchPoint.transform.localPosition = Vector3.up * scale;
                
                float angle = (i / (float)branches) * Mathf.PI * 2f;
                branchPoint.transform.localRotation = Quaternion.Euler(30, angle * Mathf.Rad2Deg, 0);
                
                CreateFractalTree(branchPoint.transform, scale * 0.7f, color, rng, depth + 1, maxDepth, identity);
            }
        }
        
        private void CreateGyroscope(Transform parent, float scale, Color color, System.Random rng, ShapeIdentity identity)
        {
            // Multiple rotating rings
            int rings = 3;
            int segments = 20;
            identity.elementCount = rings * segments;
            identity.shapeRadius = scale * 2f;
            for (int i = 0; i < rings; i++)
            {
                float radius = scale * (1.5f + i * 0.5f);
                float ringAngle = i * 60f;
                
                for (int s = 0; s < segments; s++)
                {
                    float angle = (s / (float)segments) * Mathf.PI * 2f;
                    Vector3 pos = new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
                    
                    // Rotate ring plane
                    pos = Quaternion.Euler(ringAngle, 0, 0) * pos;
                    
                    AddPrimitive(PrimitiveType.Cube, parent, pos, scale * 0.15f, color * (1f - i * 0.2f), rng);
                }
            }
        }
        
        private void CreateMandala(Transform parent, float scale, Color color, System.Random rng, ShapeIdentity identity)
        {
            // Center
            AddPrimitive(PrimitiveType.Sphere, parent, Vector3.zero, scale * 0.4f, color, rng);
            
            // Concentric layers
            int layers = 4;
            int totalPetals = 0;
            for (int layer = 1; layer <= layers; layer++)
            {
                totalPetals += 6 * layer;
            }
            identity.elementCount = totalPetals + 1;
            identity.shapeRadius = scale * layers * 0.7f;
            for (int layer = 1; layer <= layers; layer++)
            {
                int petals = 6 * layer;
                float radius = scale * layer * 0.7f;
                
                for (int i = 0; i < petals; i++)
                {
                    float angle = (i / (float)petals) * Mathf.PI * 2f;
                    Vector3 pos = new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
                    
                    PrimitiveType type = layer % 2 == 0 ? PrimitiveType.Cube : PrimitiveType.Sphere;
                    float petalScale = scale * 0.2f * (1f - layer * 0.1f);
                    AddPrimitive(type, parent, pos, petalScale, color * (1f - layer * 0.15f), rng);
                }
            }
        }
        
        private void CreateAtomModel(Transform parent, float scale, Color color, System.Random rng, ShapeIdentity identity)
        {
            // Nucleus
            AddPrimitive(PrimitiveType.Sphere, parent, Vector3.zero, scale * 0.5f, color, rng);
            
            // Electron shells
            int shells = rng.Next(2, 4);
            int totalElectrons = 0;
            for (int shell = 0; shell < shells; shell++)
            {
                float orbitRadius = scale * (1.5f + shell);
                int electrons = rng.Next(2, 5);
                totalElectrons += electrons;
                float shellTilt = shell * 60f;
                
                if (shell == shells - 1)
                {
                    identity.elementCount = totalElectrons + 1;
                    identity.shapeRadius = orbitRadius;
                }
                
                for (int e = 0; e < electrons; e++)
                {
                    float angle = (e / (float)electrons) * Mathf.PI * 2f + (float)(rng.NextDouble() * 0.5);
                    Vector3 pos = new Vector3(Mathf.Cos(angle) * orbitRadius, 0, Mathf.Sin(angle) * orbitRadius);
                    pos = Quaternion.Euler(shellTilt, 0, 0) * pos;
                    
                    Color electronColor = GetThemeColor("Electron", shell, rng);
                    AddPrimitive(PrimitiveType.Sphere, parent, pos, scale * 0.25f, electronColor, rng);
                }
            }
        }
        
        private void CreateGeometricFlower(Transform parent, float scale, Color color, System.Random rng, ShapeIdentity identity)
        {
            // Center
            AddPrimitive(PrimitiveType.Sphere, parent, Vector3.zero, scale * 0.5f, color, rng);
            
            // Petals
            int petals = rng.Next(6, 10);
            identity.elementCount = petals + 1 + 5;
            identity.shapeRadius = scale * 1.5f;
            identity.shapeHeight = scale * 0.6f;
            for (int i = 0; i < petals; i++)
            {
                float angle = (i / (float)petals) * Mathf.PI * 2f;
                float petalDist = scale * 1.5f;
                Vector3 pos = new Vector3(Mathf.Cos(angle) * petalDist, 0, Mathf.Sin(angle) * petalDist);
                
                GameObject petal = AddPrimitive(PrimitiveType.Sphere, parent, pos, scale * 0.6f, color * 0.9f, rng);
                petal.transform.localScale = new Vector3(scale * 0.4f, scale * 0.2f, scale * 0.6f);
                petal.transform.localRotation = Quaternion.Euler(0, angle * Mathf.Rad2Deg, 0);
            }
            
            // Stamen
            for (int i = 0; i < 5; i++)
            {
                Vector3 pos = new Vector3(
                    (float)(rng.NextDouble() * scale * 0.4 - scale * 0.2),
                    scale * 0.6f,
                    (float)(rng.NextDouble() * scale * 0.4 - scale * 0.2)
                );
                AddPrimitive(PrimitiveType.Sphere, parent, pos, scale * 0.15f, Color.yellow, rng);
            }
        }
        
        private void CreateWireframePolyhedron(Transform parent, float scale, Color color, System.Random rng, ShapeIdentity identity)
        {
            // Icosahedron-like structure
            float phi = (1f + Mathf.Sqrt(5f)) / 2f;
            Vector3[] vertices = new Vector3[]
            {
                new Vector3(-1, phi, 0).normalized,
                new Vector3(1, phi, 0).normalized,
                new Vector3(-1, -phi, 0).normalized,
                new Vector3(1, -phi, 0).normalized,
                new Vector3(0, -1, phi).normalized,
                new Vector3(0, 1, phi).normalized,
                new Vector3(0, -1, -phi).normalized,
                new Vector3(0, 1, -phi).normalized,
                new Vector3(phi, 0, -1).normalized,
                new Vector3(phi, 0, 1).normalized,
                new Vector3(-phi, 0, -1).normalized,
                new Vector3(-phi, 0, 1).normalized
            };
            
            identity.elementCount = vertices.Length;
            identity.shapeRadius = scale * 1.5f;
            
            // Place spheres at vertices
            foreach (var vertex in vertices)
            {
                AddPrimitive(PrimitiveType.Sphere, parent, vertex * scale * 1.5f, scale * 0.2f, color, rng);
            }
            
            // Add connecting edges (sample)
            int[][] edges = new int[][] {
                new int[] {0, 1}, new int[] {0, 5}, new int[] {0, 7}, new int[] {0, 10}, new int[] {0, 11},
                new int[] {1, 5}, new int[] {1, 7}, new int[] {1, 8}, new int[] {1, 9}
            };
            
            foreach (var edge in edges)
            {
                Vector3 start = vertices[edge[0]] * scale * 1.5f;
                Vector3 end = vertices[edge[1]] * scale * 1.5f;
                Vector3 mid = (start + end) / 2f;
                Vector3 dir = end - start;
                
                GameObject conn = AddPrimitive(PrimitiveType.Cylinder, parent, mid, scale * 0.3f, color * 0.6f, rng);
                conn.transform.localScale = new Vector3(scale * 0.05f, dir.magnitude / 2f, scale * 0.05f);
                conn.transform.localRotation = Quaternion.FromToRotation(Vector3.up, dir);
            }
        }
        
        private void CreateTorusStructure(Transform parent, float scale, Color color, System.Random rng, ShapeIdentity identity)
        {
            // Torus made from spheres
            int majorSegments = 16;
            int minorSegments = 8;
            float majorRadius = scale * 1.5f;
            float minorRadius = scale * 0.5f;
            identity.elementCount = majorSegments * minorSegments;
            identity.shapeRadius = majorRadius + minorRadius;
            
            for (int i = 0; i < majorSegments; i++)
            {
                float u = (i / (float)majorSegments) * Mathf.PI * 2f;
                for (int j = 0; j < minorSegments; j++)
                {
                    float v = (j / (float)minorSegments) * Mathf.PI * 2f;
                    
                    float x = (majorRadius + minorRadius * Mathf.Cos(v)) * Mathf.Cos(u);
                    float y = minorRadius * Mathf.Sin(v);
                    float z = (majorRadius + minorRadius * Mathf.Cos(v)) * Mathf.Sin(u);
                    
                    AddPrimitive(PrimitiveType.Sphere, parent, new Vector3(x, y, z), scale * 0.15f, color, rng);
                }
            }
        }
        
        private void CreateChainLinks(Transform parent, float scale, Color color, System.Random rng, ShapeIdentity identity)
        {
            // Interlocking rings
            int links = rng.Next(4, 6);
            int segments = 12;
            identity.elementCount = links * segments;
            identity.shapeRadius = scale * 0.7f;
            identity.shapeHeight = links * scale * 1.2f;
            for (int i = 0; i < links; i++)
            {
                float radius = scale * 0.7f;
                float linkOffset = i * scale * 1.2f;
                float linkAngle = i % 2 == 0 ? 0 : 90;
                
                for (int s = 0; s < segments; s++)
                {
                    float angle = (s / (float)segments) * Mathf.PI * 2f;
                    Vector3 pos = new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
                    pos = Quaternion.Euler(0, linkAngle, 0) * pos;
                    pos += Vector3.right * linkOffset;
                    
                    AddPrimitive(PrimitiveType.Capsule, parent, pos, scale * 0.2f, color * (1f - i * 0.1f), rng);
                }
            }
        }
        
        private void CreatePyramidArray(Transform parent, float scale, Color color, System.Random rng, ShapeIdentity identity)
        {
            // Stepped pyramid
            int levels = 4;
            int totalBlocks = 0;
            for (int level = 0; level < levels; level++)
            {
                int size = levels - level;
                totalBlocks += size * size;
            }
            identity.elementCount = totalBlocks;
            identity.shapeHeight = levels * scale * 0.6f;
            identity.shapeRadius = levels * scale * 0.5f;
            for (int level = 0; level < levels; level++)
            {
                int size = levels - level;
                float levelHeight = level * scale * 0.6f;
                
                for (int x = 0; x < size; x++)
                {
                    for (int z = 0; z < size; z++)
                    {
                        Vector3 pos = new Vector3(
                            (x - size / 2f) * scale * 0.5f,
                            levelHeight,
                            (z - size / 2f) * scale * 0.5f
                        );
                        
                        AddPrimitive(PrimitiveType.Cube, parent, pos, scale * 0.4f, color * (1f - level * 0.15f), rng);
                    }
                }
            }
        }
        
        private void CreateFibonacciSpiral(Transform parent, float scale, Color color, System.Random rng, ShapeIdentity identity)
        {
            // Golden spiral
            int points = 25;
            float goldenAngle = Mathf.PI * (3f - Mathf.Sqrt(5f));
            identity.elementCount = points;
            identity.shapeRadius = scale * Mathf.Sqrt(points) * 0.3f;
            identity.shapeHeight = points * scale * 0.15f;
            
            for (int i = 0; i < points; i++)
            {
                float t = i / (float)points;
                float angle = i * goldenAngle;
                float radius = scale * Mathf.Sqrt(i) * 0.3f;
                float height = i * scale * 0.15f;
                
                Vector3 pos = new Vector3(
                    Mathf.Cos(angle) * radius,
                    height,
                    Mathf.Sin(angle) * radius
                );
                
                float sphereSize = scale * 0.15f * (1f + t * 0.5f);
                AddPrimitive(PrimitiveType.Sphere, parent, pos, sphereSize, color * (0.5f + t * 0.5f), rng);
            }
        }
        
        private GameObject AddPrimitive(PrimitiveType type, Transform parent, Vector3 position, float scale, Color color, System.Random rng)
        {
            GameObject obj = GameObject.CreatePrimitive(type);
            obj.transform.SetParent(parent);
            obj.transform.localPosition = position;
            obj.transform.localScale = Vector3.one * scale;
            obj.transform.localRotation = Quaternion.Euler(
                (float)(rng.NextDouble() * 360),
                (float)(rng.NextDouble() * 360),
                (float)(rng.NextDouble() * 360)
            );
            
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = color;
            mat.SetFloat("_Metallic", (float)rng.NextDouble() * 0.8f);
            mat.SetFloat("_Glossiness", (float)rng.NextDouble() * 0.5f + 0.5f);
            
            if (rng.NextDouble() > 0.5)
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", color * (float)(rng.NextDouble() * 0.5));
            }
            
            obj.GetComponent<Renderer>().material = mat;
            return obj;
        }
        
        private void MarkChildrenAsAmbient(Transform parent, string theme, System.Random rng)
        {
            // Recursively mark all children with Ambient role
            // DO NOT add them to cast - they should not be animated independently
            foreach (Transform child in parent)
            {
                // Just mark children, don't add to cast lists
                // This prevents them from being animated separately
                
                // Recurse for nested children
                if (child.childCount > 0)
                {
                    MarkChildrenAsAmbient(child, theme, rng);
                }
            }
        }
        
        private GameObject CreateBackgroundObject(string theme, int index, Transform parent, System.Random rng)
        {
            PrimitiveType[] shapes = { PrimitiveType.Cube, PrimitiveType.Sphere, PrimitiveType.Cylinder };
            GameObject obj = GameObject.CreatePrimitive(shapes[rng.Next(shapes.Length)]);
            obj.name = $"{theme}_Background_{index}";
            obj.transform.SetParent(parent);
            
            // Create a ring/circle formation
            float radius = 15f;
            float angle = (index / 12f) * Mathf.PI * 2f + (float)(rng.NextDouble() * 0.5);
            
            obj.transform.localPosition = new Vector3(
                Mathf.Cos(angle) * radius,
                (float)(rng.NextDouble() * 8f - 4f),
                Mathf.Sin(angle) * radius
            );
            
            obj.transform.localRotation = Quaternion.Euler(
                (float)(rng.NextDouble() * 360),
                (float)(rng.NextDouble() * 360),
                0
            );
            
            float scale = (float)(rng.NextDouble() * 0.15 + 0.1);
            obj.transform.localScale = Vector3.one * scale;
            
            Material mat = new Material(Shader.Find("Standard"));
            mat.name = $"{obj.name}_Material";
            mat.color = GetThemeColor(theme, index + 20, rng);
            mat.SetFloat("_Metallic", 0.2f);
            mat.SetFloat("_Glossiness", 0.3f);
            obj.GetComponent<Renderer>().material = mat;
            
            return obj;
        }
        
        private GameObject CreateAmbientObject(string theme, int index, Transform parent, System.Random rng)
        {
            // Ambient objects are small, subtle particles
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            obj.name = $"{theme}_Ambient_{index}";
            obj.transform.SetParent(parent);
            
            // Random position in a larger volume
            obj.transform.localPosition = new Vector3(
                (float)(rng.NextDouble() * 30 - 15),
                (float)(rng.NextDouble() * 15 - 7.5),
                (float)(rng.NextDouble() * 30 - 15)
            );
            
            obj.transform.localRotation = Quaternion.identity;
            
            float scale = (float)(rng.NextDouble() * 0.08 + 0.02);
            obj.transform.localScale = Vector3.one * scale;
            
            Material mat = new Material(Shader.Find("Standard"));
            mat.name = $"{obj.name}_Material";
            Color ambientColor = GetThemeColor(theme, index + 40, rng);
            ambientColor.a = 0.6f;
            mat.color = ambientColor;
            mat.SetFloat("_Metallic", 0f);
            mat.SetFloat("_Glossiness", 0.8f);
            
            // Add subtle emission
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", ambientColor * 0.2f);
            
            obj.GetComponent<Renderer>().material = mat;
            
            return obj;
        }
        
        private GameObject CreateSpotlightObject(string theme, int index, Transform parent, System.Random rng)
        {
            // Spotlight objects are dramatic and eye-catching
            PrimitiveType[] dramaticShapes = { PrimitiveType.Sphere, PrimitiveType.Capsule, PrimitiveType.Cylinder };
            GameObject obj = GameObject.CreatePrimitive(dramaticShapes[rng.Next(dramaticShapes.Length)]);
            obj.name = $"{theme}_Spotlight_{index}";
            obj.transform.SetParent(parent);
            
            // Start at extreme positions
            float extremeRadius = 20f;
            float angle = (float)(rng.NextDouble() * Mathf.PI * 2f);
            
            obj.transform.localPosition = new Vector3(
                Mathf.Cos(angle) * extremeRadius,
                (float)(rng.NextDouble() * 10f + 5f),
                Mathf.Sin(angle) * extremeRadius
            );
            
            obj.transform.localRotation = Quaternion.Euler(
                (float)(rng.NextDouble() * 360),
                (float)(rng.NextDouble() * 360),
                (float)(rng.NextDouble() * 360)
            );
            
            float scale = (float)(rng.NextDouble() * 0.4 + 0.4);
            obj.transform.localScale = Vector3.one * scale;
            
            Material mat = new Material(Shader.Find("Standard"));
            mat.name = $"{obj.name}_Material";
            Color spotColor = GetThemeColor(theme, index + 60, rng);
            mat.color = spotColor;
            mat.SetFloat("_Metallic", 0.9f);
            mat.SetFloat("_Glossiness", 1f);
            
            // Strong emission for spotlight effect
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", spotColor * (float)(rng.NextDouble() * 2 + 1));
            
            obj.GetComponent<Renderer>().material = mat;
            
            return obj;
        }
        
        private Color GetThemeColor(string theme, int variation, System.Random rng)
        {
            float randomFactor = (float)(rng.NextDouble() * 0.3 + 0.7); // 0.7 to 1.0
            
            switch (theme)
            {
                case "Cosmic":
                    Color[] cosmicColors = {
                        new Color(0.1f, 0.1f, 0.5f),   // Deep blue
                        new Color(0.5f, 0.1f, 0.8f),   // Purple
                        new Color(1f, 0.8f, 0.2f),     // Gold
                        new Color(0.2f, 0.8f, 0.9f),   // Cyan
                        new Color(1f, 0.4f, 0.7f)      // Pink
                    };
                    return cosmicColors[variation % cosmicColors.Length] * randomFactor;
                
                case "Neon City":
                    Color[] neonColors = {
                        new Color(1f, 0f, 0.5f),       // Hot pink
                        new Color(0f, 1f, 1f),         // Cyan
                        new Color(1f, 1f, 0f),         // Yellow
                        new Color(0.5f, 0f, 1f),       // Purple
                        new Color(0f, 1f, 0.5f)        // Green
                    };
                    return neonColors[variation % neonColors.Length] * randomFactor;
                
                case "Underwater":
                    Color[] waterColors = {
                        new Color(0.1f, 0.3f, 0.6f),   // Deep ocean
                        new Color(0.2f, 0.6f, 0.8f),   // Light blue
                        new Color(0.1f, 0.8f, 0.7f),   // Turquoise
                        new Color(0.5f, 0.9f, 1f),     // Light cyan
                        new Color(0.2f, 0.5f, 0.3f)    // Seaweed green
                    };
                    return waterColors[variation % waterColors.Length] * randomFactor;
                
                case "Crystal Garden":
                    Color[] crystalColors = {
                        new Color(0.9f, 0.2f, 0.9f),   // Magenta
                        new Color(0.5f, 1f, 0.8f),     // Mint
                        new Color(1f, 0.8f, 0.9f),     // Rose
                        new Color(0.7f, 0.9f, 1f),     // Ice blue
                        new Color(1f, 1f, 0.7f)        // Pale yellow
                    };
                    return crystalColors[variation % crystalColors.Length] * randomFactor;
                
                case "Abstract Geometry":
                    Color[] geometryColors = {
                        new Color(1f, 0.3f, 0.2f),     // Red
                        new Color(0.2f, 0.8f, 0.3f),   // Green
                        new Color(0.3f, 0.4f, 1f),     // Blue
                        new Color(1f, 0.7f, 0.1f),     // Orange
                        new Color(0.8f, 0.2f, 0.8f)    // Violet
                    };
                    return geometryColors[variation % geometryColors.Length] * randomFactor;
                
                default:
                    return new Color((float)rng.NextDouble(), (float)rng.NextDouble(), (float)rng.NextDouble());
            }
        }
#endif
        #endregion
    }
}