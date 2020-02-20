using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityStreamDeck.Messages;
using WebSocketSharp;
using WebSocketSharp.Net;

namespace UnityStreamDeck
{
    [InitializeOnLoad]
    [ExecuteInEditMode]
    public class StreamDeckLiveLink
    {
        private static WebSocket connection;

        private static readonly Queue<string> messagesQueue = new Queue<string>();

        private static readonly string sceneViewPath = "Window/General/Scene";
        private static readonly string gameViewPath = "Window/General/Game";

        static StreamDeckLiveLink()
        {
            if (int.TryParse(Application.unityVersion.Substring(0, 4), out var unityMajorVersion) && unityMajorVersion < 2018)
            {
                sceneViewPath = "Window/Scene";
                gameViewPath = "Window/Game";
            }

            CreateConnection();

            ConnectToMessageServer();

            Log("initialized");
        }

        private static void Log(string message)
        {
            //if (StreamDeckLiveLinkPreferences.Instance.Verbose)
            //{
            Debug.Log($"Stream Deck Live Link - {message}.");
            //}
        }

        private static void LogError(string message)
        {
            //if (StreamDeckLiveLinkPreferences.Instance.Verbose)
            //{
            Debug.LogError($"Stream Deck Live Link - {message}");
            //}
        }

        private static void CreateConnection()
        {
            connection = new WebSocket("ws://127.0.0.1:18084");
            connection.SetCookie(new Cookie("X-Unity-PID", System.Diagnostics.Process.GetCurrentProcess().Id.ToString()));
            connection.OnMessage += (sender, e) => QueueMessage(e);
            connection.OnClose += (s, e) => OnDisconnectedFromMessageServer(e.Code, e.Reason);
            connection.OnError += (s, e) => LogError("disconnected, error: " + e.Message);
            connection.OnOpen += (s, e) => OnConnectedToMessageServer();
        }

        private static void ConnectToMessageServer()
        {
            Debug.Log("connecting");

            connection.Connect();
        }

        private static void OnConnectedToMessageServer()
        {
            EditorApplication.update += HandleMessages;

            Log("connected");
        }

        private static void OnDisconnectedFromMessageServer(ushort code, string reason)
        {
            Log($"disconnected ({code}/{reason})");

            ConnectToMessageServer();
        }

        private static void HandleMessages()
        {
            if (messagesQueue.Count > 0)
            {
                var rawMessage = messagesQueue.Dequeue();

                var message = JsonUtility.FromJson<Message>(rawMessage);

                switch (message.Id)
                {
                    case nameof(ExecuteMenuMessage):
                        var executeMessage = JsonUtility.FromJson<ExecuteMenuMessage>(rawMessage);
                        ExecuteMenu(executeMessage.Path);
                        break;
                    case nameof(RotateObjectMessage):
                        var rotateMessage = JsonUtility.FromJson<RotateObjectMessage>(rawMessage);
                        RotateObjects(rotateMessage.Axis, rotateMessage.Angle);
                        break;
                    case nameof(ResetObjectMessage):
                        ResetObjectsPositionAndRotation();
                        break;
                    case nameof(PasteComponentMessage):
                        PasteComponent();
                        break;
                    case nameof(PauseModeMessage):
                        TogglePauseMode();
                        break;
                    case nameof(ToggleSceneGameMessage):
                        ToggleSceneGame();
                        break;
                }
            }
        }

        private static void QueueMessage(MessageEventArgs e)
        {
            //Log("Message received, " + e.Data);

            messagesQueue.Enqueue(e.Data);
        }

        //private static bool ConfirmAction()
        //{
        //    return EditorUtility.DisplayDialog("Confirm execution", "????", "Ok", "Cancel");
        //}

        private static bool CanExecute()
        {
            return Selection.gameObjects?.Length > 0;

            //if (Selection.gameObjects.Length > 1)
            //{
            //    if (!StreamDeckLiveLinkPreferences.Instance.ConfirmActions || ConfirmAction())
            //    {
            //        return true;
            //    }
            //}
            //else
            //{
            //    return true;
            //}

            //return false;
        }

        private static void RecordForUndo(UnityEngine.Object toBeUndo, string description)
        {
            Undo.RecordObject(toBeUndo, description);
        }

        private static void ResetObjectsPositionAndRotation()
        {
            if (CanExecute())
            {
                foreach (var gameObject in Selection.gameObjects)
                {
                    RecordForUndo(gameObject.transform, nameof(ResetObjectsPositionAndRotation));

                    gameObject.transform.SetPositionAndRotation(Vector3.zero, Quaternion.Euler(Vector3.zero));
                }
            }
        }

        private static void RotateObjects(string axis, float angle)
        {
            if (CanExecute())
            {
                var eulers = Vector3.zero;

                var desiredAngle = angle > 0 ? angle : 90;

                switch (axis)
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
                    RecordForUndo(gameObject.transform, nameof(RotateObjects));

                    gameObject.transform.Rotate(eulers, Space.Self);
                }
            }
        }

        private static void PasteComponent()
        {
            if (CanExecute())
            {
                foreach (var gameObject in Selection.gameObjects)
                {
                    RecordForUndo(gameObject, nameof(PasteComponent));

                    ComponentUtility.PasteComponentAsNew(gameObject);
                }
            }
        }

        private static void ExecuteMenu(string data)
        {
            if (!string.IsNullOrEmpty(data))
            {
                EditorApplication.ExecuteMenuItem(data);
            }
        }

        private static void TogglePauseMode()
        {
            if (EditorApplication.isPlaying)
            {
                EditorApplication.isPaused = !EditorApplication.isPaused;
            }
        }

        private static void ToggleSceneGame()
        {
            if (EditorWindow.focusedWindow != null)
            {
                if (EditorWindow.focusedWindow.GetType() == typeof(SceneView))
                {
                    ExecuteMenu(gameViewPath);
                }
                else
                {
                    ExecuteMenu(sceneViewPath);
                }
            }
        }

        // smooth, low/terrain
        //private static void SelectTerrainPaintLayer()
        //{
        //    //TerrainLayerInspector (Reflection Internal)
        //}

        //private static void SelectTerrainBrush()
        //{
        //    //TerrainLayerInspector (Reflection Internal)
        //}
    }
}
