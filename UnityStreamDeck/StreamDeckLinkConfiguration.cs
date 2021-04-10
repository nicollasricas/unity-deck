using System;
using UnityEditor;

namespace UnityStreamDeck
{
    public class StreamDeckLinkConfiguration
    {
        public string ServerHost { get; set; }

        public int ServerPort { get; set; }

        public bool Enabled { get; set; }

        public bool Verbose { get; set; }

        public bool Debug { get; set; }

        private static StreamDeckLinkConfiguration instance;

        public static StreamDeckLinkConfiguration Instance
        {
            get
            {
                if (instance is null)
                {
                    instance = new StreamDeckLinkConfiguration();
                }

                return instance;
            }
        }

        public event Action ConfigurationChanged;

        public StreamDeckLinkConfiguration() => Load();

        private void Load()
        {
            ServerHost = EditorPrefs.GetString(GetPrefsFullName(nameof(ServerHost)), "127.0.0.1");
            ServerPort = EditorPrefs.GetInt(GetPrefsFullName(nameof(ServerPort)), 18084);
            Enabled = EditorPrefs.GetBool(GetPrefsFullName(nameof(Enabled)), true);
            Verbose = EditorPrefs.GetBool(GetPrefsFullName(nameof(Verbose)), true);
            Debug = EditorPrefs.GetBool(GetPrefsFullName(nameof(Debug)), false);
        }

        public void Save()
        {
            EditorPrefs.SetString(GetPrefsFullName(nameof(ServerHost)), ServerHost);
            EditorPrefs.SetInt(GetPrefsFullName(nameof(ServerPort)), ServerPort);
            EditorPrefs.SetBool(GetPrefsFullName(nameof(Enabled)), Enabled);
            EditorPrefs.SetBool(GetPrefsFullName(nameof(Verbose)), Verbose);
            EditorPrefs.SetBool(GetPrefsFullName(nameof(Debug)), Debug);

            ConfigurationChanged?.Invoke();
        }

        private string GetPrefsFullName(string preference) => $"StreamDeck_{preference}";
    }
}
