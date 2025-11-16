using UnityEngine;
using UnityEditor;

namespace MusicVideoGenerator
{
    public class CastGeneratorWindow : EditorWindow
    {
        private MusicVideoCast targetCast;
        
        [Header("Generation Settings")]
        private string selectedTheme = "Cosmic";
        private string[] availableThemes = { "Cosmic", "Neon City", "Underwater", "Crystal Garden", "Abstract Geometry" };
        
        [Header("Cast Counts")]
        private int leadStarCount = 6;
        private int coStarCount = 10;
        private int featuredCount = 15;
        private int backgroundCount = 25;
        private int ambientCount = 40;
        private int spotlightCount = 10;
        
        private Vector2 scrollPosition;
        
        [MenuItem("Tools/Music Video Generator/Cast Generator")]
        public static void ShowWindow()
        {
            var window = GetWindow<CastGeneratorWindow>("Cast Generator");
            window.minSize = new Vector2(400, 600);
        }
        
        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            EditorGUILayout.LabelField("🎬 Procedural Cast Generator", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            // Target Cast
            EditorGUILayout.LabelField("Target", EditorStyles.boldLabel);
            targetCast = (MusicVideoCast)EditorGUILayout.ObjectField("Music Video Cast", targetCast, typeof(MusicVideoCast), true);
            
            if (targetCast == null)
            {
                EditorGUILayout.HelpBox("Select a MusicVideoCast component to generate cast members for.", MessageType.Info);
                EditorGUILayout.EndScrollView();
                return;
            }
            
            EditorGUILayout.Space(10);
            
            // Theme Selection
            EditorGUILayout.LabelField("Theme", EditorStyles.boldLabel);
            int currentThemeIndex = System.Array.IndexOf(availableThemes, selectedTheme);
            if (currentThemeIndex < 0) currentThemeIndex = 0;
            
            int newThemeIndex = EditorGUILayout.Popup("Visual Theme", currentThemeIndex, availableThemes);
            selectedTheme = availableThemes[newThemeIndex];
            
            EditorGUILayout.Space(10);
            
            // Cast Counts
            EditorGUILayout.LabelField("Cast Member Counts", EditorStyles.boldLabel);
            
            leadStarCount = EditorGUILayout.IntSlider("⭐ Lead Stars", leadStarCount, 1, 10);
            coStarCount = EditorGUILayout.IntSlider("⭐ Co-Stars", coStarCount, 2, 15);
            featuredCount = EditorGUILayout.IntSlider("🎭 Featured Support", featuredCount, 5, 20);
            backgroundCount = EditorGUILayout.IntSlider("🏞️ Background", backgroundCount, 10, 50);
            ambientCount = EditorGUILayout.IntSlider("✨ Ambient", ambientCount, 20, 100);
            spotlightCount = EditorGUILayout.IntSlider("💡 Spotlight", spotlightCount, 5, 20);
            
            int totalCount = leadStarCount + coStarCount + featuredCount + backgroundCount + ambientCount + spotlightCount;
            EditorGUILayout.LabelField($"Total Objects: {totalCount}", EditorStyles.helpBox);
            
            EditorGUILayout.Space(10);
            
            // Performance Warning
            if (totalCount > 150)
            {
                EditorGUILayout.HelpBox($"⚠️ Generating {totalCount} objects may impact performance. Consider reducing counts.", MessageType.Warning);
            }
            else if (totalCount > 100)
            {
                EditorGUILayout.HelpBox($"Generating {totalCount} objects. This may take a moment.", MessageType.Info);
            }
            
            EditorGUILayout.Space(10);
            
            // Action Buttons
            EditorGUILayout.BeginHorizontal();
            
            GUI.backgroundColor = new Color(0.3f, 0.8f, 0.3f);
            if (GUILayout.Button("🎨 Generate Cast Members", GUILayout.Height(40)))
            {
                GenerateCast();
            }
            GUI.backgroundColor = Color.white;
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
            
            EditorGUILayout.BeginHorizontal();
            
            GUI.backgroundColor = new Color(0.8f, 0.3f, 0.3f);
            if (GUILayout.Button("🗑️ Clear All Cast Members", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog("Clear Cast Members", 
                    "This will delete all generated cast member GameObjects and clear the cast lists. This cannot be undone!", 
                    "Clear", "Cancel"))
                {
                    ClearCast();
                }
            }
            GUI.backgroundColor = Color.white;
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(10);
            
            // Current Cast Summary
            if (targetCast != null)
            {
                EditorGUILayout.LabelField("Current Cast", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Main: {targetCast.mainCast.Count}");
                EditorGUILayout.LabelField($"Supporting: {targetCast.supportingCast.Count}");
                EditorGUILayout.LabelField($"Spotlight: {targetCast.spotlightCast.Count}");
                EditorGUILayout.LabelField($"Total: {targetCast.GetAllCastMembers().Count}");
            }
            
            EditorGUILayout.EndScrollView();
        }
        
        private void GenerateCast()
        {
            if (targetCast == null)
            {
                EditorUtility.DisplayDialog("Error", "No MusicVideoCast selected!", "OK");
                return;
            }
            
            Undo.RecordObject(targetCast, "Generate Cast Members");
            
            // Clear existing
            ClearCastInternal();
            
            System.Random rng = new System.Random();
            Transform parent = targetCast.transform;
            
            Debug.Log($"[CastGenerator] 🎨 Generating '{selectedTheme}' themed cast with {leadStarCount + coStarCount + featuredCount + backgroundCount + ambientCount + spotlightCount} objects...");
            
            // Generate Lead Stars
            for (int i = 0; i < leadStarCount; i++)
            {
                GameObject lead = CreateThematicObject(selectedTheme, "LeadStar", i, parent, rng, true);
                targetCast.mainCast.Add(new CastMember
                {
                    gameObject = lead,
                    role = CastRole.LeadStar,
                    useCustomColors = true,
                    customPrimaryColor = GetThemeColor(selectedTheme, i, rng),
                    customSecondaryColor = GetThemeColor(selectedTheme, i + 100, rng)
                });
            }
            
            // Generate Co-Stars
            for (int i = 0; i < coStarCount; i++)
            {
                GameObject coStar = CreateThematicObject(selectedTheme, "CoStar", i, parent, rng, true);
                targetCast.mainCast.Add(new CastMember
                {
                    gameObject = coStar,
                    role = CastRole.CoStar,
                    useCustomColors = true,
                    customPrimaryColor = GetThemeColor(selectedTheme, i + 10, rng),
                    customSecondaryColor = GetThemeColor(selectedTheme, i + 110, rng)
                });
            }
            
            // Generate Featured Support
            for (int i = 0; i < featuredCount; i++)
            {
                GameObject featured = CreateThematicObject(selectedTheme, "Featured", i, parent, rng, false);
                targetCast.mainCast.Add(new CastMember
                {
                    gameObject = featured,
                    role = CastRole.FeaturedSupport,
                    useCustomColors = true,
                    customPrimaryColor = GetThemeColor(selectedTheme, i + 20, rng),
                    customSecondaryColor = GetThemeColor(selectedTheme, i + 120, rng)
                });
            }
            
            // Generate Background Support
            for (int i = 0; i < backgroundCount; i++)
            {
                GameObject background = CreateBackgroundObject(selectedTheme, i, parent, rng);
                targetCast.supportingCast.Add(new CastMember
                {
                    gameObject = background,
                    role = CastRole.BackgroundSupport,
                    useCustomColors = true,
                    customPrimaryColor = GetThemeColor(selectedTheme, i + 30, rng),
                    customSecondaryColor = GetThemeColor(selectedTheme, i + 130, rng)
                });
            }
            
            // Generate Ambient
            for (int i = 0; i < ambientCount; i++)
            {
                GameObject ambient = CreateAmbientObject(selectedTheme, i, parent, rng);
                targetCast.supportingCast.Add(new CastMember
                {
                    gameObject = ambient,
                    role = CastRole.Ambient,
                    useCustomColors = true,
                    customPrimaryColor = GetThemeColor(selectedTheme, i + 40, rng),
                    customSecondaryColor = GetThemeColor(selectedTheme, i + 140, rng),
                    animationDelay = (float)rng.NextDouble() * 2f
                });
            }
            
            // Generate Spotlight
            for (int i = 0; i < spotlightCount; i++)
            {
                GameObject spotlight = CreateSpotlightObject(selectedTheme, i, parent, rng);
                targetCast.spotlightCast.Add(new CastMember
                {
                    gameObject = spotlight,
                    role = CastRole.Spotlight,
                    useCustomColors = true,
                    customPrimaryColor = Color.white,
                    customSecondaryColor = GetThemeColor(selectedTheme, i + 50, rng),
                    animationDelay = (float)rng.NextDouble() * 3f
                });
            }
            
            EditorUtility.SetDirty(targetCast);
            Selection.activeGameObject = targetCast.gameObject;
            
            int totalGenerated = leadStarCount + coStarCount + featuredCount + backgroundCount + ambientCount + spotlightCount;
            Debug.Log($"[CastGenerator] ✨ Successfully generated {totalGenerated} cast members!");
            EditorUtility.DisplayDialog("Success", $"Generated {totalGenerated} cast members with {selectedTheme} theme!", "OK");
        }
        
        private void ClearCast()
        {
            if (targetCast == null) return;
            
            Undo.RecordObject(targetCast, "Clear Cast Members");
            ClearCastInternal();
            EditorUtility.SetDirty(targetCast);
            
            Debug.Log("[CastGenerator] 🗑️ Cleared all cast members");
        }
        
        private void ClearCastInternal()
        {
            // Delete all GameObjects that don't have the LODGroup component
            foreach (var member in targetCast.mainCast)
            {
                if (member != null && member.gameObject != null && member.gameObject.GetComponent<LODGroup>() == null)
                    DestroyImmediate(member.gameObject);
            }
            foreach (var member in targetCast.supportingCast)
            {
                if (member != null && member.gameObject != null && member.gameObject.GetComponent<LODGroup>() == null)
                    DestroyImmediate(member.gameObject);
            }
            foreach (var member in targetCast.spotlightCast)
            {
                if (member != null && member.gameObject != null && member.gameObject.GetComponent<LODGroup>() == null)
                    DestroyImmediate(member.gameObject);
            }
            
            // Clear lists
            targetCast.mainCast.Clear();
            targetCast.supportingCast.Clear();
            targetCast.spotlightCast.Clear();
        }
        
        #region Shape Creation Methods (copied from MusicVideoCast for Editor access)
        
        private GameObject CreateThematicObject(string theme, string roleName, int index, Transform parent, System.Random rng, bool isLarge)
        {
            GameObject obj = new GameObject($"{theme}_{roleName}_{index}");
            obj.transform.SetParent(parent);
            
            float radius = isLarge ? 5f : 10f;
            float angle = (float)(rng.NextDouble() * Mathf.PI * 2f);
            float distance = (float)(rng.NextDouble() * radius);
            float height = isLarge ? (float)(rng.NextDouble() * 3f) : (float)(rng.NextDouble() * 6f - 3f);
            
            obj.transform.localPosition = new Vector3(
                Mathf.Cos(angle) * distance,
                height,
                Mathf.Sin(angle) * distance
            );
            
            obj.transform.localRotation = Quaternion.Euler(
                (float)(rng.NextDouble() * 360),
                (float)(rng.NextDouble() * 360),
                (float)(rng.NextDouble() * 360)
            );
            
            float scale = isLarge ? (float)(rng.NextDouble() * 0.4 + 0.4) : (float)(rng.NextDouble() * 0.3 + 0.2); // Larger scale
            obj.transform.localScale = Vector3.one * scale;
            
            // Add ShapeIdentity and create compound shape
            ShapeIdentity identity = obj.AddComponent<ShapeIdentity>();
            identity.originalScale = obj.transform.localScale;
            identity.primaryColor = GetThemeColor(theme, index, rng);
            identity.animationSpeed = (float)(rng.NextDouble() * 0.5 + 0.5);
            identity.themeName = theme;
            
            // Pick random shape type
            ShapeType shapeType = (ShapeType)rng.Next(20);
            identity.shapeType = shapeType;
            
            // Set rotation axis for shapes that need it
            if (shapeType == ShapeType.OrbitalSystem || shapeType == ShapeType.Tower || 
                shapeType == ShapeType.Helix || shapeType == ShapeType.StarBurst ||
                shapeType == ShapeType.SpiralGalaxy || shapeType == ShapeType.FractalTree ||
                shapeType == ShapeType.GeometricFlower || shapeType == ShapeType.Torus ||
                shapeType == ShapeType.PyramidArray || shapeType == ShapeType.FibonacciSpiral)
            {
                identity.rotationAxis = Vector3.up;
            }
            else if (shapeType == ShapeType.Mandala)
            {
                identity.rotationAxis = Vector3.forward;
            }
            
            // Create simple compound shape
            CreateSimpleCompoundShape(obj.transform, scale, identity.primaryColor, rng, shapeType, identity);
            
            // Add subtle point light for glow effect (only for main cast)
            if (isLarge)
            {
                Light glowLight = obj.AddComponent<Light>();
                glowLight.type = LightType.Point;
                
                // Random range between 0.5 and 2
                float randomRange = (float)(rng.NextDouble() * 1.5 + 0.5); // 0.5 to 2.0
                glowLight.range = scale * randomRange * 8f;
                
                // Intensity inversely proportional to range (higher range = lower intensity)
                float normalizedRange = (randomRange - 0.5f) / 1.5f; // 0 to 1
                glowLight.intensity = Mathf.Lerp(2.5f, 0.8f, normalizedRange); // 2.5 for small range, 0.8 for large range
                
                glowLight.color = identity.primaryColor;
                glowLight.renderMode = LightRenderMode.ForceVertex;
                glowLight.shadows = LightShadows.None;
            }
            
            return obj;
        }
        
        private void CreateSimpleCompoundShape(Transform parent, float scale, Color color, System.Random rng, ShapeType shapeType, ShapeIdentity identity)
        {
            int elementCount = rng.Next(5, 12);
            identity.elementCount = elementCount;
            identity.shapeRadius = scale * 2f;
            
            for (int i = 0; i < elementCount; i++)
            {
                float angle = (i / (float)elementCount) * Mathf.PI * 2f;
                float radius = scale * (float)(rng.NextDouble() * 1.5 + 1);
                Vector3 pos = new Vector3(Mathf.Cos(angle) * radius, (float)(rng.NextDouble() * scale - scale / 2), Mathf.Sin(angle) * radius);
                
                GameObject primitive = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                primitive.transform.SetParent(parent);
                primitive.transform.localPosition = pos;
                primitive.transform.localScale = Vector3.one * scale * 0.3f;
                
                // Create glowing translucent material
                Material mat = new Material(Shader.Find("Standard"));
                Color glowColor = color * (float)(rng.NextDouble() * 0.3 + 0.7);
                mat.color = glowColor;
                mat.SetFloat("_Mode", 3); // Transparent rendering mode
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = 3000;
                
                // Make very translucent - almost see-through
                Color translucentColor = glowColor;
                translucentColor.a = 0.1f; // Very transparent
                mat.color = translucentColor;
                
                // Add strong emission for glow
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", glowColor * 3f); // Much brighter glow
                mat.SetFloat("_Metallic", 0f);
                mat.SetFloat("_Glossiness", 1f);
                
                primitive.GetComponent<Renderer>().material = mat;
            }
        }
        
        private GameObject CreateBackgroundObject(string theme, int index, Transform parent, System.Random rng)
        {
            PrimitiveType[] shapes = { PrimitiveType.Cube, PrimitiveType.Sphere, PrimitiveType.Cylinder };
            GameObject obj = GameObject.CreatePrimitive(shapes[rng.Next(shapes.Length)]);
            obj.name = $"{theme}_Background_{index}";
            obj.transform.SetParent(parent);
            
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
            mat.color = GetThemeColor(theme, index + 200, rng);
            mat.SetFloat("_Metallic", 0.2f);
            mat.SetFloat("_Glossiness", 0.3f);
            obj.GetComponent<Renderer>().material = mat;
            
            return obj;
        }
        
        private GameObject CreateAmbientObject(string theme, int index, Transform parent, System.Random rng)
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            obj.name = $"{theme}_Ambient_{index}";
            obj.transform.SetParent(parent);
            
            obj.transform.localPosition = new Vector3(
                (float)(rng.NextDouble() * 30 - 15),
                (float)(rng.NextDouble() * 15 - 7.5),
                (float)(rng.NextDouble() * 30 - 15)
            );
            
            float scale = (float)(rng.NextDouble() * 0.08 + 0.02);
            obj.transform.localScale = Vector3.one * scale;
            
            Material mat = new Material(Shader.Find("Standard"));
            Color ambientColor = GetThemeColor(theme, index + 300, rng);
            mat.color = ambientColor;
            mat.SetFloat("_Metallic", 0f);
            mat.SetFloat("_Glossiness", 0.8f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", ambientColor * 0.2f);
            obj.GetComponent<Renderer>().material = mat;
            
            return obj;
        }
        
        private GameObject CreateSpotlightObject(string theme, int index, Transform parent, System.Random rng)
        {
            PrimitiveType[] shapes = { PrimitiveType.Sphere, PrimitiveType.Capsule, PrimitiveType.Cylinder };
            GameObject obj = GameObject.CreatePrimitive(shapes[rng.Next(shapes.Length)]);
            DestroyImmediate(obj.GetComponent<Collider>());
            obj.name = $"{theme}_Spotlight_{index}";
            obj.transform.SetParent(parent);
            
            float radius = 20f;
            float angle = (float)(rng.NextDouble() * Mathf.PI * 2f);
            
            obj.transform.localPosition = new Vector3(
                Mathf.Cos(angle) * radius,
                (float)(rng.NextDouble() * 10f + 5f),
                Mathf.Sin(angle) * radius
            );
            
            obj.transform.localRotation = Quaternion.Euler(
                (float)(rng.NextDouble() * 360),
                (float)(rng.NextDouble() * 360),
                (float)(rng.NextDouble() * 360)
            );
            
            float scale = (float)(rng.NextDouble() * 0.4 + 0.4);
            obj.transform.localScale = Vector3.one * scale;
            
            Material mat = new Material(Shader.Find("Standard"));
            Color spotColor = GetThemeColor(theme, index + 400, rng);
            
            // Store original color for emission BEFORE modifying alpha
            Color emissionColor = spotColor * (float)(rng.NextDouble() * 1.5f + 1f); // Brighter glow with full opacity color
            
            mat.SetFloat("_Mode", 3); // Transparent rendering mode
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3000;
            
            // Set color with reasonable alpha
            spotColor.a = 0.5f; // More visible (50% opacity)
            mat.color = spotColor;
            mat.SetFloat("_Metallic", 0.5f);
            mat.SetFloat("_Glossiness", 0.8f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", emissionColor);
            mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            obj.GetComponent<Renderer>().material = mat;
            
            return obj;
        }
        
        private Color GetThemeColor(string theme, int variation, System.Random rng)
        {
            float randomFactor = (float)(rng.NextDouble() * 0.3 + 0.7);
            
            switch (theme)
            {
                case "Cosmic":
                    Color[] cosmicColors = {
                        new Color(0.1f, 0.1f, 0.5f),
                        new Color(0.5f, 0.1f, 0.8f),
                        new Color(1f, 0.8f, 0.2f),
                        new Color(0.2f, 0.8f, 0.9f),
                        new Color(1f, 0.4f, 0.7f)
                    };
                    return cosmicColors[variation % cosmicColors.Length] * randomFactor;
                
                case "Neon City":
                    Color[] neonColors = {
                        new Color(1f, 0f, 0.5f),
                        new Color(0f, 1f, 1f),
                        new Color(1f, 1f, 0f),
                        new Color(0.5f, 0f, 1f),
                        new Color(0f, 1f, 0.5f)
                    };
                    return neonColors[variation % neonColors.Length] * randomFactor;
                
                case "Underwater":
                    Color[] waterColors = {
                        new Color(0.1f, 0.3f, 0.6f),
                        new Color(0.2f, 0.6f, 0.8f),
                        new Color(0.1f, 0.8f, 0.7f),
                        new Color(0.5f, 0.9f, 1f),
                        new Color(0.2f, 0.5f, 0.3f)
                    };
                    return waterColors[variation % waterColors.Length] * randomFactor;
                
                case "Crystal Garden":
                    Color[] crystalColors = {
                        new Color(0.9f, 0.2f, 0.9f),
                        new Color(0.5f, 1f, 0.8f),
                        new Color(1f, 0.8f, 0.9f),
                        new Color(0.7f, 0.9f, 1f),
                        new Color(1f, 1f, 0.7f)
                    };
                    return crystalColors[variation % crystalColors.Length] * randomFactor;
                
                case "Abstract Geometry":
                    Color[] geometryColors = {
                        new Color(1f, 0.3f, 0.2f),
                        new Color(0.2f, 0.8f, 0.3f),
                        new Color(0.3f, 0.4f, 1f),
                        new Color(1f, 0.7f, 0.1f),
                        new Color(0.8f, 0.2f, 0.8f)
                    };
                    return geometryColors[variation % geometryColors.Length] * randomFactor;
                
                default:
                    return new Color((float)rng.NextDouble(), (float)rng.NextDouble(), (float)rng.NextDouble());
            }
        }
        
        #endregion
    }
}
