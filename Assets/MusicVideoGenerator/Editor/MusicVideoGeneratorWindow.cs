using UnityEngine;
using UnityEditor;
using UnityEngine.Timeline;
using UnityEditor.SceneManagement;
using System.Linq;

namespace MusicVideoGenerator
{
    /// <summary>
    /// Main editor window for generating music videos.
    /// Super simple: assign cast, assign settings, press button!
    /// </summary>
    public class MusicVideoGeneratorWindow : EditorWindow
    {
        private MusicVideoCast cast;
        private MusicVideoSettings settings;
        private Vector2 scrollPosition;
        
        [MenuItem("Tools/Music Video Generator")]
        public static void ShowWindow()
        {
            var window = GetWindow<MusicVideoGeneratorWindow>("Music Video Generator");
            window.minSize = new Vector2(400, 300);
        }
        
        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            DrawHeader();
            DrawCastSection();
            DrawSettingsSection();
            DrawGenerateSection();
            DrawHelpSection();
            
            EditorGUILayout.EndScrollView();
        }
        
        private void DrawHeader()
        {
            GUILayout.Space(10);
            
            var headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 18,
                alignment = TextAnchor.MiddleCenter
            };
            
            GUILayout.Label("🎬 Music Video Generator", headerStyle);
            GUILayout.Space(5);
            
            EditorGUILayout.HelpBox(
                "Drag & drop GameObjects, assign roles, press generate!\nYour objects will come to life synchronized to music!",
                MessageType.Info
            );
            
            GUILayout.Space(10);
        }
        
        private void DrawCastSection()
        {
            EditorGUILayout.LabelField("Step 1: Assign Cast", EditorStyles.boldLabel);
            
            MusicVideoCast newCast = (MusicVideoCast)EditorGUILayout.ObjectField(
                "Music Video Cast",
                cast,
                typeof(MusicVideoCast),
                true
            );
            
            if (newCast != cast)
            {
                cast = newCast;
            }
            
            if (cast != null)
            {
                EditorGUI.indentLevel++;
                
                var validation = cast.Validate();
                
                if (validation.IsValid)
                {
                    EditorGUILayout.HelpBox("✅ " + validation.GetSummary(), MessageType.Info);
                }
                else if (validation.HasWarnings)
                {
                    EditorGUILayout.HelpBox("⚠️ " + validation.GetSummary(), MessageType.Warning);
                }
                else
                {
                    EditorGUILayout.HelpBox("❌ " + validation.GetSummary(), MessageType.Error);
                }
                
                EditorGUI.indentLevel--;
                
                if (GUILayout.Button("Select Cast in Scene"))
                {
                    Selection.activeGameObject = cast.gameObject;
                    EditorGUIUtility.PingObject(cast);
                }
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "Create a MusicVideoCast component in your scene:\n" +
                    "1. Create an empty GameObject\n" +
                    "2. Add MusicVideoCast component\n" +
                    "3. Drag GameObjects into Main Cast / Supporting Cast\n" +
                    "4. Assign roles (LeadStar, CoStar, etc.)",
                    MessageType.Warning
                );
            }
            
            GUILayout.Space(10);
        }
        
        private void DrawSettingsSection()
        {
            EditorGUILayout.LabelField("Step 2: Assign Settings", EditorStyles.boldLabel);
            
            MusicVideoSettings newSettings = (MusicVideoSettings)EditorGUILayout.ObjectField(
                "Settings Asset",
                settings,
                typeof(MusicVideoSettings),
                false
            );
            
            if (newSettings != settings)
            {
                settings = newSettings;
            }
            
            if (settings == null)
            {
                EditorGUILayout.HelpBox(
                    "Create a settings asset:\nRight-click in Project → Create → Music Video → Settings",
                    MessageType.Warning
                );
                
                if (GUILayout.Button("Create New Settings Asset"))
                {
                    CreateSettingsAsset();
                }
            }
            else
            {
                EditorGUI.indentLevel++;
                
                if (settings.HasAnyMusicData())
                {
                    EditorGUILayout.HelpBox("✅ Music analysis data assigned", MessageType.Info);
                }
                else
                {
                    EditorGUILayout.HelpBox("⚠️ No music analysis data - assign Aubio files for music sync", MessageType.Warning);
                }
                
                EditorGUI.indentLevel--;
                
                if (GUILayout.Button("Edit Settings"))
                {
                    Selection.activeObject = settings;
                    EditorGUIUtility.PingObject(settings);
                }
            }
            
            GUILayout.Space(10);
        }
        
        private void DrawGenerateSection()
        {
            EditorGUILayout.LabelField("Step 3: Generate!", EditorStyles.boldLabel);
            
            if (settings != null)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Random Seed:", GUILayout.Width(100));
                settings.randomSeed = EditorGUILayout.IntField(settings.randomSeed);
                if (GUILayout.Button("🎲 New", GUILayout.Width(60)))
                {
                    settings.randomSeed = Random.Range(0, 100000);
                }
                EditorGUILayout.EndHorizontal();
            }
            
            GUILayout.Space(5);
            
            bool canGenerate = CanGenerate();
            GUI.enabled = canGenerate;
            
            var buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                fixedHeight = 50
            };
            
            if (GUILayout.Button("🎥 GENERATE MUSIC VIDEO 🎥", buttonStyle))
            {
                GenerateMusicVideo();
            }
            
            GUI.enabled = true;
            
            if (!canGenerate)
            {
                EditorGUILayout.HelpBox(GetGenerationBlockerMessage(), MessageType.Error);
            }
            
            GUILayout.Space(5);
            
            GUI.enabled = cast != null && cast.playableDirector != null;
            if (GUILayout.Button("🗑️ Clear Timeline", GUILayout.Height(35)))
            {
                if (EditorUtility.DisplayDialog(
                    "Clear Timeline",
                    "Remove all tracks and delete generated animation clips?",
                    "Clear",
                    "Cancel"))
                {
                    TimelineUtilities.ClearTimeline(cast.playableDirector);
                }
            }
            GUI.enabled = true;
            
            GUILayout.Space(10);
        }
        
        private void DrawHelpSection()
        {
            EditorGUILayout.LabelField("Tips", EditorStyles.boldLabel);
            
            EditorGUILayout.HelpBox(
                "🌟 Role Guide:\n\n" +
                "• LeadStar - Main character, most dramatic animations\n" +
                "• CoStar - Secondary main, important but not always center\n" +
                "• FeaturedSupport - Gets spotlight moments\n" +
                "• BackgroundSupport - Simple, supportive animations\n" +
                "• Ambient - Subtle atmosphere effects\n" +
                "• Spotlight - Dramatic entrances at key moments",
                MessageType.None
            );
        }
        
        private bool CanGenerate()
        {
            if (cast == null) return false;
            if (settings == null) return false;
            
            var validation = cast.Validate();
            return validation.IsValid;
        }
        
        private string GetGenerationBlockerMessage()
        {
            if (cast == null)
                return "No MusicVideoCast assigned";
            if (settings == null)
                return "No Settings assigned";
            
            var validation = cast.Validate();
            if (!validation.IsValid)
                return validation.GetSummary();
            
            return "Unknown error";
        }
        
        private void GenerateMusicVideo()
        {
            var generator = new MusicVideoGenerator();
            
            try
            {
                generator.Generate(cast, settings);
                
                EditorUtility.DisplayDialog(
                    "Success! 🎉",
                    $"Music video generated!\n\n" +
                    $"Seed: {settings.randomSeed}\n" +
                    $"Cast: {cast.GetAllCastMembers().Count} members\n\n" +
                    "Press Play to watch!",
                    "Awesome!"
                );
            }
            catch (System.Exception ex)
            {
                EditorUtility.DisplayDialog(
                    "Generation Failed",
                    $"Error: {ex.Message}\n\nCheck console for details.",
                    "OK"
                );
                Debug.LogError($"Music video generation failed: {ex}");
            }
        }
        
        private void CreateSettingsAsset()
        {
            var newSettings = CreateInstance<MusicVideoSettings>();
            
            string path = EditorUtility.SaveFilePanelInProject(
                "Save Music Video Settings",
                "MusicVideoSettings",
                "asset",
                "Create settings asset"
            );
            
            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.CreateAsset(newSettings, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                settings = newSettings;
                EditorGUIUtility.PingObject(newSettings);
            }
        }
    }
}