using System;
using System.IO;
using IcarusGame.CleanDuel;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace IcarusGame.EditorTools
{
    public static class CleanDuelVisualCapture
    {
        private const string ScenePath = "Assets/Scenes/CleanDuelScene.unity";
        private const string DefaultOutputFileName = "TempMasterDuelVisualCapture.png";
        private const string SurrenderOutputFileName = "TempSurrenderPromptCapture.png";
        private const int CaptureWidth = 1600;
        private const int CaptureHeight = 900;
        private static int playFrames;
        private static bool captureRequested;
        private static bool selectedPreviewCard;
        private static bool showSurrenderPrompt;
        private static string outputFileName = DefaultOutputFileName;
        private static int exitCode;

        [MenuItem("Icarus/Capture Clean Duel Visual")]
        public static void Capture()
        {
            BeginCapture(false, DefaultOutputFileName);
        }

        [MenuItem("Icarus/Capture Surrender Prompt")]
        public static void CaptureSurrenderPrompt()
        {
            BeginCapture(true, SurrenderOutputFileName);
        }

        private static void BeginCapture(bool surrenderPrompt, string requestedOutputFileName)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                throw new InvalidOperationException("Stop Play Mode before starting a Clean Duel visual capture.");
            }

            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            playFrames = 0;
            selectedPreviewCard = false;
            showSurrenderPrompt = surrenderPrompt;
            outputFileName = requestedOutputFileName;
            exitCode = 0;
            captureRequested = true;
            EditorApplication.update -= UpdateCapture;
            EditorApplication.update += UpdateCapture;
            EditorApplication.playModeStateChanged -= HandlePlayModeChanged;
            EditorApplication.playModeStateChanged += HandlePlayModeChanged;
            EditorApplication.isPlaying = true;
        }

        private static void HandlePlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                Screen.SetResolution(CaptureWidth, CaptureHeight, false);
                playFrames = 0;
                return;
            }

            if (state == PlayModeStateChange.EnteredEditMode && captureRequested)
            {
                CompleteCaptureSession();
            }
        }

        private static void UpdateCapture()
        {
            if (!captureRequested || !EditorApplication.isPlaying)
            {
                return;
            }

            playFrames++;
            CleanDuelBootstrap bootstrap = UnityEngine.Object.FindAnyObjectByType<CleanDuelBootstrap>();
            if (!selectedPreviewCard && playFrames >= 8 && bootstrap != null)
            {
                if (showSurrenderPrompt)
                {
                    bootstrap.RequestSurrender();
                }
                else
                {
                    bootstrap.SelectHand(0);
                }

                selectedPreviewCard = true;
            }

            if (playFrames < 18)
            {
                return;
            }

            Camera camera = Camera.main;
            Canvas canvas = UnityEngine.Object.FindAnyObjectByType<Canvas>();
            if (camera == null || canvas == null)
            {
                if (playFrames < 240)
                {
                    return;
                }

                FailCapture("The duel camera or HUD canvas was not created in time.");
                return;
            }

            try
            {
                CaptureFrame(camera, canvas);
                Debug.Log("Clean Duel visual capture saved to " + CapturePath());
                StopPlayMode(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                StopPlayMode(1);
            }
        }

        private static void CaptureFrame(Camera camera, Canvas canvas)
        {
            RenderTexture renderTexture = new RenderTexture(CaptureWidth, CaptureHeight, 24, RenderTextureFormat.ARGB32);
            renderTexture.name = "Clean Duel Visual Capture";
            Texture2D screenshot = new Texture2D(CaptureWidth, CaptureHeight, TextureFormat.RGBA32, false);
            RenderTexture previousActive = RenderTexture.active;
            RenderTexture previousTarget = camera.targetTexture;

            try
            {
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = camera;
                canvas.planeDistance = 1f;
                Canvas.ForceUpdateCanvases();

                camera.aspect = (float)CaptureWidth / CaptureHeight;
                camera.targetTexture = renderTexture;
                camera.Render();
                RenderTexture.active = renderTexture;
                screenshot.ReadPixels(new Rect(0f, 0f, CaptureWidth, CaptureHeight), 0, 0, false);
                screenshot.Apply(false, false);
                File.WriteAllBytes(CapturePath(), screenshot.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture = previousTarget;
                RenderTexture.active = previousActive;
                UnityEngine.Object.DestroyImmediate(screenshot);
                renderTexture.Release();
                UnityEngine.Object.DestroyImmediate(renderTexture);
            }
        }

        private static void FailCapture(string message)
        {
            Debug.LogError(message);
            StopPlayMode(1);
        }

        private static void StopPlayMode(int requestedExitCode)
        {
            exitCode = requestedExitCode;
            EditorApplication.update -= UpdateCapture;
            if (EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = false;
            }
            else
            {
                CompleteCaptureSession();
            }
        }

        private static void CompleteCaptureSession()
        {
            captureRequested = false;
            EditorApplication.update -= UpdateCapture;
            EditorApplication.playModeStateChanged -= HandlePlayModeChanged;
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(exitCode);
            }
        }

        private static string CapturePath()
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, "..", outputFileName));
        }
    }
}
