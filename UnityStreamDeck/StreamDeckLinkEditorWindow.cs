using UnityEditor;
using UnityEngine;

namespace UnityStreamDeck
{
    public class StreamDeckLinkEditorWindow : EditorWindow
    {
        [MenuItem("Tools/Stream Deck")]
        public static void Init()
        {
            var editorWindow = GetWindow<StreamDeckLinkEditorWindow>("Stream Deck");
            editorWindow.maxSize = new Vector2(280, 180);
            editorWindow.minSize = editorWindow.maxSize;
            editorWindow.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();

            var configuration = StreamDeckLinkConfiguration.Instance;
            configuration.Enabled = EditorGUILayout.BeginToggleGroup(new GUIContent("Enabled"), configuration.Enabled);
            configuration.ServerHost = EditorGUILayout.TextField(new GUIContent("Server Host"), configuration.ServerHost);
            configuration.ServerPort = EditorGUILayout.IntField(new GUIContent("Server Port"), configuration.ServerPort);
            configuration.Verbose = EditorGUILayout.ToggleLeft(new GUIContent("Verbose"), configuration.Verbose);

            EditorGUILayout.Space();

            EditorGUILayout.Space();

            EditorGUILayout.EndToggleGroup();

            if (GUILayout.Button("Save"))
            {
                configuration.Save();
            }

            if (GUILayout.Button("Report ISSUE"))
            {
                Application.OpenURL("https://github.com/nicollasricas/unity-streamdeck/issues/new");
            }

            EditorGUILayout.Space();

            configuration.Debug = EditorGUILayout.ToggleLeft(new GUIContent("Debug"), configuration.Debug);

            EditorGUILayout.EndVertical();
        }
    }
}
