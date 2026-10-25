#if UNITY_EDITOR
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using UnityEditor;
using UnityEngine;
using UiContext.Core.Config;
using UiContext.Core.Formatters;
using UiContext.Core.Rules;
using UiContext.Core.Serialization;
using UiContext.Core.Validation;

namespace UiContext.Editor
{
    /// <summary>
    /// Localhost HTTP server running within the Unity Editor to respond to AI agent requests
    /// without requiring batchmode processes when the workspace is locked by the active editor.
    /// </summary>
    [InitializeOnLoad]
    public static class UiContextEditorBridge
    {
        private const int DefaultPort = 49152;
        private static HttpListener? _listener;
        private static Thread? _listenerThread;
        private static bool _isRunning;

        static UiContextEditorBridge()
        {
            // Auto-start bridge if configured or available
            EditorApplication.quitting += StopBridge;
        }

        [MenuItem("Tools/UI Context/Start Local Agent Bridge", false, 100)]
        public static void StartBridge()
        {
            if (_isRunning)
            {
                Debug.Log($"[UiContext] Bridge already active on http://127.0.0.1:{DefaultPort}/");
                return;
            }

            try
            {
                _listener = new HttpListener();
                _listener.Prefixes.Add($"http://127.0.0.1:{DefaultPort}/");
                _listener.Start();
                _isRunning = true;

                _listenerThread = new Thread(ListenLoop) { IsBackground = true };
                _listenerThread.Start();
                Debug.Log($"[UiContext] In-Editor Local Bridge started on http://127.0.0.1:{DefaultPort}/");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[UiContext] Could not start local bridge (port may be in use): {ex.Message}");
            }
        }

        [MenuItem("Tools/UI Context/Stop Local Agent Bridge", false, 101)]
        public static void StopBridge()
        {
            if (!_isRunning) return;
            _isRunning = false;
            try
            {
                _listener?.Stop();
                _listener?.Close();
            }
            catch { }
            Debug.Log("[UiContext] Local Agent Bridge stopped.");
        }

        private static void ListenLoop()
        {
            while (_isRunning && _listener != null && _listener.IsListening)
            {
                try
                {
                    var context = _listener.GetContext();
                    ThreadPool.QueueUserWorkItem(_ => HandleRequest(context));
                }
                catch
                {
                    break;
                }
            }
        }

        private static void HandleRequest(HttpListenerContext context)
        {
            var req = context.Request;
            var res = context.Response;

            res.Headers.Add("Access-Control-Allow-Origin", "*");
            res.Headers.Add("Content-Type", "application/json");

            try
            {
                string path = req.Url?.AbsolutePath ?? "/";
                if (string.Equals(path, "/health", StringComparison.OrdinalIgnoreCase))
                {
                    byte[] data = Encoding.UTF8.GetBytes("{\"status\":\"ready\",\"editor\":\"Unity\",\"version\":\"" + Application.unityVersion + "\"}");
                    res.StatusCode = 200;
                    res.OutputStream.Write(data, 0, data.Length);
                }
                else
                {
                    byte[] notFound = Encoding.UTF8.GetBytes("{\"error\":\"Endpoint not found. Use /health\"}");
                    res.StatusCode = 404;
                    res.OutputStream.Write(notFound, 0, notFound.Length);
                }
            }
            catch (Exception ex)
            {
                res.StatusCode = 500;
                byte[] err = Encoding.UTF8.GetBytes("{\"error\":\"" + ex.Message + "\"}");
                res.OutputStream.Write(err, 0, err.Length);
            }
            finally
            {
                res.Close();
            }
        }
    }
}
#endif
