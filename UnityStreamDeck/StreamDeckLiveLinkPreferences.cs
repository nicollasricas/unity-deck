using UnityEditor;
using UnityEngine;

namespace UnityStreamDeck
{
    public class StreamDeckLiveLinkPreferences
    {
        private readonly string ConfirmMultipleActionsPreference = "StreamDeckConfirmMultipleActions";

        private readonly string VerbosePreference = "StreamDeckVerbose";

        public bool ConfirmActions { get; set; }

        public bool Verbose { get; set; }

        private static StreamDeckLiveLinkPreferences instance;

        public static StreamDeckLiveLinkPreferences Instance
        {
            get
            {
                if (instance is null)
                {
                    instance = new StreamDeckLiveLinkPreferences();
                }

                return instance;
            }
        }

        public StreamDeckLiveLinkPreferences() => Load();

        private void Load()
        {
            Log("Loading preferences");

            ConfirmActions = EditorPrefs.GetBool(ConfirmMultipleActionsPreference, true);
            Verbose = EditorPrefs.GetBool(VerbosePreference, false);
        }

        public void Save()
        {
            Log("Saving preferences");

            EditorPrefs.SetBool(ConfirmMultipleActionsPreference, ConfirmActions);
            EditorPrefs.SetBool(VerbosePreference, Verbose);
        }

        private void Log(string message)
        {
            if (Verbose)
            {
                Debug.Log($"Stream Deck Live Link - {message}");
            }
        }
    }
}
