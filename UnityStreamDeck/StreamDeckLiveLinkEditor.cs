using UnityEditor;
using UnityEngine;

namespace UnityStreamDeck
{
    public class StreamDeckLiveLinkEditor : EditorWindow
    {
        //[MenuItem("Tools/Stream Deck")]
        //public static void Init()
        //{
        //    var window = EditorWindow.GetWindow<StreamDeckLiveLinkEditor>();
        //    window.titleContent = CreateWindowHeader();
        //    window.maxSize = new Vector2(300, 300);
        //    window.minSize = window.maxSize;
        //    window.Show();
        //}

        private static GUIContent CreateWindowHeader()
        {
            return new GUIContent()
            {
                text = "Stream Deck",
                tooltip = "Stream deck"
            };
        }

        private void OnGUI()
        {
            GUILayout.BeginVertical();

#if DEBUG
            StreamDeckLiveLinkPreferences.Instance.Verbose = GUILayout.Toggle(StreamDeckLiveLinkPreferences.Instance.Verbose, new GUIContent("Verbose"));
#endif

            if (GUILayout.Button("Save"))
            {
                StreamDeckLiveLinkPreferences.Instance.Save();
            }

            GUILayout.EndVertical();
        }
    }
}
