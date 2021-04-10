using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Net;

namespace UnityStreamDeck
{
    [InitializeOnLoad]
    [ExecuteInEditMode]
    public class StreamDeckLink
    {
        private WebSocket webSocket;
        private bool disconnectedFromMessageServer;
        private double lastReconnectTime;
        private readonly float reconnectInterval = 5f;
        private readonly StreamDeckLinkConfiguration configuration;
        private readonly Dictionary<string, MessageHandler> messagesHandlers = new Dictionary<string, MessageHandler>();
        private readonly Queue<string> messages = new Queue<string>();
        private readonly Dictionary<string, Type> components = new Dictionary<string, Type>();
        private readonly string sceneViewPath = "Window/Scene";
        private readonly string gameViewPath = "Window/Game";

        static StreamDeckLink()
        {
            var liveLink = new StreamDeckLink(StreamDeckLinkConfiguration.Instance);
            liveLink.Initialize();
        }

        public StreamDeckLink(StreamDeckLinkConfiguration configuration) => this.configuration = configuration;

        private void Initialize()
        {
            configuration.ConfigurationChanged += Configuration_ConfigurationChanged;

#if UNITY_2019_1_OR_NEWER
            sceneViewPath = "Window/General/Scene";
            gameViewPath = "Window/General/Game";
#endif

            if (configuration.Enabled)
            {
                RegisterFeaturesMessages();
                RegisterUnityEvents();
                ConnectToMessageServer();
            }
        }

        private void RegisterFeatureMessageHandler<T>(Action<T> handler) where T : Message
        {
            var type = typeof(T);

            messagesHandlers.Add(type.Name, new MessageHandler(type, (message) => handler?.DynamicInvoke(message)));
        }

        private void RegisterFeaturesMessages()
        {
            RegisterFeatureMessageHandler<ExecuteMenuMessage>(ExecuteMenu);
            RegisterFeatureMessageHandler<RotateObjectMessage>(RotateObject);
            RegisterFeatureMessageHandler<ResetObjectMessage>(ResetObject);
            RegisterFeatureMessageHandler<PasteComponentMessage>(PasteComponent);
            RegisterFeatureMessageHandler<TogglePauseMessage>(TogglePauseMode);
            RegisterFeatureMessageHandler<ToggleSceneGameMessage>(ToggleSceneGame);
            RegisterFeatureMessageHandler<ToggleObjectStateMessage>(ToggleObjectState);
            RegisterFeatureMessageHandler<SelectObjectMessage>(SelectObject);
            RegisterFeatureMessageHandler<AddComponentMessage>(AddComponent);
        }

        private void UnregisterFeaturesMessages() => messagesHandlers.Clear();

        private void RegisterUnityEvents()
        {
            EditorApplication.update += EditorApplication_update;

            AssemblyReloadEvents.afterAssemblyReload += AssemblyReloadEvents_afterAssemblyReload;
        }

        private void UnregisterUnityEvents()
        {
            EditorApplication.update -= EditorApplication_update;

            AssemblyReloadEvents.afterAssemblyReload -= AssemblyReloadEvents_afterAssemblyReload;
        }

        private void Configuration_ConfigurationChanged()
        {
            Log("Configuration changed", ignoreVerbose: true);

            DisconnectFromMessageServer();
            UnregisterUnityEvents();
            UnregisterFeaturesMessages();

            if (configuration.Enabled)
            {
                RegisterUnityEvents();
                RegisterFeaturesMessages();
            }
        }

        private void ConnectToMessageServer()
        {
            if (webSocket is null)
            {
                webSocket = new WebSocket($"ws://{configuration.ServerHost}:{configuration.ServerPort}");
                webSocket.SetCookie(new Cookie("X-Unity-PID", System.Diagnostics.Process.GetCurrentProcess().Id.ToString()));
                webSocket.OnMessage += (sender, e) => OnMessageReceivedFromMessageServer(e);
                webSocket.OnClose += (s, e) => OnDisconnectedFromMessageServer(e.Code, e.Reason);
                webSocket.OnError += (s, e) => Debug.LogError($"Stream Deck - Error: {e.Message}");
                webSocket.OnOpen += (s, e) => OnConnectedToMessageServer();
            }

            Log($"Connecting to {webSocket.Url.Host}:{webSocket.Url.Port}...");

            webSocket.ConnectAsync();
        }

        private void DisconnectFromMessageServer()
        {
            webSocket?.Close(CloseStatusCode.Normal);
            webSocket = null;

            disconnectedFromMessageServer = true;
        }

        private void OnConnectedToMessageServer()
        {
            Log("Connected", ignoreVerbose: true);

            disconnectedFromMessageServer = false;
        }

        private void OnDisconnectedFromMessageServer(ushort code, string reason)
        {
            Log($"Disconnected ({code}/{reason})");

            disconnectedFromMessageServer = true;
        }

        private void EditorApplication_update()
        {
            HandleStreamDeckConnection();

            HandleMessages();
        }

        private void HandleStreamDeckConnection()
        {
            if (configuration.Enabled
                && disconnectedFromMessageServer)
            {
                if (EditorApplication.timeSinceStartup - lastReconnectTime > reconnectInterval)
                {
                    disconnectedFromMessageServer = false;

                    ConnectToMessageServer();

                    lastReconnectTime = EditorApplication.timeSinceStartup;
                }
            }
        }

        private void AssemblyReloadEvents_afterAssemblyReload() => CacheComponents();

        private void CacheComponents()
        {
#if UNITY_2019_2_OR_NEWER
            Log("Caching components...");

            components.Clear();

            foreach (var type in TypeCache.GetTypesDerivedFrom<Component>())
            {
                components[type.Name.ToLower()] = type;
            }
#endif
        }

        private void HandleMessages()
        {
            if (messages.Count > 0)
            {
                var message = messages.Dequeue();

                var messageId = JsonUtility.FromJson<Message>(message).Id;

                if (configuration.Debug)
                {
                    Log($"Message [{messageId}]: {message}", ignoreVerbose: true);
                }

                if (messagesHandlers.TryGetValue(messageId, out var messageHandler))
                {
                    messageHandler.Handler?.DynamicInvoke(JsonUtility.FromJson(message, messageHandler.Type));
                }
            }
        }

        private void OnMessageReceivedFromMessageServer(MessageEventArgs e) => messages.Enqueue(e.Data);

        private void ResetObject(ResetObjectMessage message)
        {
            foreach (var gameObject in Selection.gameObjects)
            {
                Undo.RecordObject(gameObject.transform, "Reset Object");

                gameObject.transform.SetPositionAndRotation(Vector3.zero, Quaternion.Euler(Vector3.zero));
            }
        }

        private void RotateObject(RotateObjectMessage message)
        {
            var eulers = Vector3.zero;

            var desiredAngle = message.Angle > 0 ? message.Angle : 90;

            switch (message.Axis)
            {
                case "X":
                    eulers.x = desiredAngle;
                    break;
                case "Z":
                    eulers.z = desiredAngle;
                    break;
                default:
                    eulers.y = desiredAngle;
                    break;
            }

            foreach (var gameObject in Selection.gameObjects)
            {
                Undo.RecordObject(gameObject.transform, "Rotate Object");

                gameObject.transform.Rotate(eulers, Space.Self);
            }
        }

        private void PasteComponent(PasteComponentMessage message)
        {
            foreach (var gameObject in Selection.gameObjects)
            {
                Undo.RecordObject(gameObject, "Paste Component");

                ComponentUtility.PasteComponentAsNew(gameObject);
            }
        }

        private void ExecuteMenu(ExecuteMenuMessage message)
        {
            if (!string.IsNullOrEmpty(message.Path))
            {
                EditorApplication.ExecuteMenuItem(message.Path);
            }
        }

        public void AddComponent(AddComponentMessage message)
        {
            if (components.TryGetValue(message.Name.ToLower(), out var component))
            {
                foreach (var gameObject in Selection.gameObjects)
                {
                    Undo.RecordObject(gameObject, "Add Component");

                    gameObject.AddComponent(component);
                }
            }
        }

        private void TogglePauseMode(TogglePauseMessage message)
        {
            if (EditorApplication.isPlaying)
            {
                EditorApplication.isPaused = !EditorApplication.isPaused;
            }
        }

        private void ToggleSceneGame(ToggleSceneGameMessage message)
        {
            if (EditorWindow.focusedWindow != null)
            {
                if (EditorWindow.focusedWindow.GetType() == typeof(SceneView))
                {
                    EditorApplication.ExecuteMenuItem(gameViewPath);
                }
                else
                {
                    EditorApplication.ExecuteMenuItem(sceneViewPath);
                }
            }
        }

        public void ToggleObjectState(ToggleObjectStateMessage message)
        {
            foreach (var gameObject in Selection.gameObjects)
            {
                Undo.RecordObject(gameObject, "Toggle Object State");

                if (message.State == ObjectState.Active)
                {
                    gameObject.SetActive(!gameObject.activeSelf);
                }
                else if (message.State == ObjectState.Static)
                {
                    gameObject.isStatic = !gameObject.isStatic;
                }
            }
        }

        private void SelectObject(SelectObjectMessage message)
        {
            GameObject gameObject = null;

            if (!string.IsNullOrEmpty(message.Name))
            {
                gameObject = GameObject.Find(message.Name);
            }

            if (!string.IsNullOrEmpty(message.Tag))
            {
                gameObject = GameObject.FindWithTag(message.Tag);
            }

            if (gameObject != null)
            {
                Selection.activeGameObject = gameObject;
            }
        }

        private void Log(string message, bool ignoreVerbose = false)
        {
            if (ignoreVerbose || configuration.Verbose)
            {
                Debug.Log($"Stream Deck - {message}");
            }
        }
    }
}