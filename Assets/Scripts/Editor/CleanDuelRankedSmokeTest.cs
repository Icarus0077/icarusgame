using System;
using System.IO;
using System.Reflection;
using IcarusGame.CleanDuel;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace IcarusGame.EditorTools
{
    public static class CleanDuelRankedSmokeTest
    {
        [MenuItem("Tools/Icarus/Clean Duel/Test Ranked Display")]
        public static void Run()
        {
            PrepareRankImages();
            AssertRankLevels();
            AssertServerSnapshot();
            AssertMissingAndDisabledRanked();
            AssertDisplay();
            AssertBoardClicksBlocked();
            Debug.Log("Ranked display smoke test passed: all 39 Greek tier levels, authoritative server progression, four-win/three-loss counters, diamond wallet, inactivity dates, legacy/disabled accounts and board input isolation.");
        }

        [MenuItem("Tools/Icarus/Clean Duel/Capture Ranked Preview")]
        public static void CapturePreview()
        {
            Require(SystemInfo.graphicsDeviceType != GraphicsDeviceType.Null, "Run the ranked visual preview without -nographics.");
            PrepareRankImages();
            Scene preview = EditorSceneManager.NewPreviewScene();
            RenderTexture target = null;
            Texture2D image = null;
            RenderTexture previousTarget = RenderTexture.active;
            try
            {
                GameObject root = new GameObject("Isolated Ranked Preview");
                SceneManager.MoveGameObjectToScene(root, preview);
                CleanDuelRankedData data = Data(12);
                data.rating = 1324;
                data.games = 27;
                data.wins = 18;
                data.losses = 8;
                data.draws = 1;
                data.levelWins = 2;
                data.levelLosses = 1;
                data.diamonds = 1400;
                data.lastRankedAt = "2026-10-19T12:00:00.000Z";
                data.nextDecayAt = "2026-11-18T12:00:00.000Z";
                CleanDuelRankedView view = new CleanDuelRankedView();
                view.Build(root.transform, Session(true, data));
                view.SetOpen(true);

                Camera camera = new GameObject("Ranked Preview Camera", typeof(Camera)).GetComponent<Camera>();
                SceneManager.MoveGameObjectToScene(camera.gameObject, preview);
                camera.scene = preview;
                camera.enabled = false;
                camera.orthographic = true;
                camera.orthographicSize = 5.4f;
                camera.aspect = 1920f / 1080f;
                camera.transform.position = new Vector3(0f, 0f, -10f);
                camera.nearClipPlane = 0.1f;
                camera.farClipPlane = 30f;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(0.02f, 0.03f, 0.05f, 1f);
                camera.allowHDR = false;
                camera.allowMSAA = false;
                UniversalAdditionalCameraData cameraData = camera.GetUniversalAdditionalCameraData();
                cameraData.renderPostProcessing = false;
                cameraData.renderShadows = false;

                Canvas canvas = root.GetComponentInChildren<Canvas>();
                canvas.renderMode = RenderMode.WorldSpace;
                canvas.worldCamera = camera;
                RectTransform canvasRect = canvas.GetComponent<RectTransform>();
                canvasRect.pivot = new Vector2(0.5f, 0.5f);
                canvasRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 1920f);
                canvasRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 1080f);
                canvasRect.localScale = Vector3.one * 0.01f;
                canvasRect.localPosition = Vector3.zero;
                Canvas.ForceUpdateCanvases();

                target = new RenderTexture(1920, 1080, 24, RenderTextureFormat.ARGB32);
                target.Create();
                camera.targetTexture = target;
                if (GraphicsSettings.currentRenderPipeline != null)
                {
                    UniversalRenderPipeline.SingleCameraRequest request = new UniversalRenderPipeline.SingleCameraRequest { destination = target };
                    Require(RenderPipeline.SupportsRenderRequest(camera, request), "Active render pipeline does not support the ranked preview request.");
                    RenderPipeline.SubmitRenderRequest(camera, request);
                }
                else camera.Render();
                RenderTexture.active = target;
                image = new Texture2D(target.width, target.height, TextureFormat.RGB24, false);
                image.ReadPixels(new Rect(0f, 0f, target.width, target.height), 0, 0);
                image.Apply();
                string requestedPath = Environment.GetEnvironmentVariable("ICARUS_RANKED_PREVIEW_PATH");
                string path = Path.GetFullPath(string.IsNullOrWhiteSpace(requestedPath)
                    ? Path.Combine(Path.GetTempPath(), "icarus-ranked-preview.png")
                    : requestedPath);
                string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, "..")) + Path.DirectorySeparatorChar;
                Require(!path.StartsWith(projectRoot, StringComparison.OrdinalIgnoreCase), "Store ranked preview captures outside the Unity project.");
                File.WriteAllBytes(path, image.EncodeToPNG());
                Debug.Log("Ranked visual preview saved: " + path);
            }
            finally
            {
                RenderTexture.active = previousTarget;
                if (image != null) UnityEngine.Object.DestroyImmediate(image);
                if (target != null)
                {
                    target.Release();
                    UnityEngine.Object.DestroyImmediate(target);
                }
                EditorSceneManager.ClosePreviewScene(preview);
            }
        }

        [MenuItem("Tools/Icarus/Clean Duel/Import Greek Rank Badges")]
        public static void PrepareRankImages()
        {
            for (int index = 0; index < CleanDuelRankCatalog.Count; index++)
            {
                string path = "Assets/Resources/Ranked/" + CleanDuelRankCatalog.At(index).Id + ".png";
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                Require(importer != null, "Missing Greek rank texture importer: " + path);
                if (importer.textureType == TextureImporterType.Default && importer.maxTextureSize == 512 &&
                    importer.alphaIsTransparency && !importer.mipmapEnabled && !importer.isReadable &&
                    importer.alphaSource == TextureImporterAlphaSource.FromInput && importer.sRGBTexture &&
                    importer.wrapMode == TextureWrapMode.Clamp && importer.filterMode == FilterMode.Bilinear)
                {
                    continue;
                }
                importer.textureType = TextureImporterType.Default;
                importer.maxTextureSize = 512;
                importer.alphaSource = TextureImporterAlphaSource.FromInput;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.isReadable = false;
                importer.sRGBTexture = true;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.filterMode = FilterMode.Bilinear;
                importer.SaveAndReimport();
            }
        }

        private static void AssertRankLevels()
        {
            Require(CleanDuelRankCatalog.Count == 6, "Expected six original Greek ranks.");
            string[] ids = { "icarus", "hermes", "athena", "apollo", "poseidon", "zeus" };
            string[] tiers = { "Bronze", "Silver", "Gold", "Platinum", "Diamond", "Master" };
            string[] patrons = { "Icarus", "Hermes", "Athena", "Apollo", "Poseidon", "Zeus" };
            int[] levels = { 4, 5, 6, 7, 8, 9 };
            int firstStep = 0;
            for (int index = 0; index < ids.Length; index++)
            {
                CleanDuelRankDefinition rank = CleanDuelRankCatalog.At(index);
                Require(rank.Id == ids[index] && rank.FirstStep == firstStep && rank.Levels == levels[index], "Greek rank level catalog mismatch.");
                Require(rank.Name == tiers[index] && rank.PatronName == patrons[index], "Material tier must be primary and Greek patron secondary.");
                Require(rank.DisplayName == tiers[index] + " — " + patrons[index], "Tier and patron display label mismatch.");
                for (int level = levels[index]; level >= 1; level--)
                {
                    int step = firstStep + levels[index] - level;
                    Require(CleanDuelRankCatalog.ForStep(step).Id == ids[index], "Wrong tier for ladder step.");
                    Require(CleanDuelRankCatalog.LevelForStep(step) == level, "Levels must count down towards one.");
                    Require(CleanDuelRankCatalog.DisplayForStep(step) == tiers[index] + " " + level + " — " + patrons[index], "Level label mismatch.");
                }
                firstStep += levels[index];
                Require(Resources.Load<Texture2D>("Ranked/" + ids[index]) != null, "Missing original Greek rank image: " + ids[index]);
            }
            Require(firstStep == 39 && CleanDuelRankCatalog.StepCount == 39, "Expected exactly 39 ladder levels.");
            Require(CleanDuelRankCatalog.DisplayForStep(0) == "Bronze 4 — Icarus", "New accounts must start at Bronze 4.");
            Require(CleanDuelRankCatalog.DisplayForStep(38) == "Master 1 — Zeus", "Highest supported level must be Master 1.");
        }

        private static void AssertServerSnapshot()
        {
            string json = "{\"decision\":{\"allowed\":true},\"user\":{\"id\":\"ranked-test\",\"displayName\":\"Test\"},\"settings\":{\"rankedEnabled\":true},\"ranked\":{\"seasonId\":\"ranked-v1\",\"rating\":1300,\"rankId\":\"athena\",\"rankName\":\"Gold\",\"rankPatronName\":\"Athena\",\"games\":10,\"wins\":6,\"losses\":3,\"draws\":1,\"isSimulation\":false,\"progressionVersion\":1,\"ladderStep\":12,\"level\":3,\"tierLevels\":6,\"levelWins\":2,\"levelLosses\":1,\"winsRequired\":4,\"lossesRequired\":3,\"diamonds\":4500000000,\"highestStep\":12,\"lastRankedAt\":\"2026-10-01T12:00:00.000Z\",\"nextDecayAt\":\"2026-10-31T12:00:00.000Z\"}}";
            CleanDuelAccessResponse response = JsonUtility.FromJson<CleanDuelAccessResponse>(json);
            CleanDuelAccessSession session;
            string error;
            Require(CleanDuelAccessSession.TryCreate(response, out session, out error), "Ranked login response failed to parse.");
            Require(session.Ranked != null && session.Ranked.Rating == 1300 && session.Ranked.Rank.Id == "athena", "Ranked server snapshot was not retained.");
            Require(session.Ranked.DisplayName == "Gold 3 — Athena", "Ranked login must display the authoritative tier, level and patron.");
            Require(session.Ranked.Diamonds == 4500000000L, "Diamond wallets must not overflow 32-bit integers.");
            Require(session.Ranked.LevelWins == 2 && session.Ranked.LevelLosses == 1, "Accumulated match counters were not retained.");
            Require(session.Ranked.NextDecayAt.HasValue && session.Ranked.LastRankedAt.HasValue, "Inactivity dates were not retained.");
            Require(session.Ranked.Games == 10 && session.Ranked.Wins == 6 && session.Ranked.Losses == 3 && session.Ranked.Draws == 1, "Per-account ranked record mismatch.");
            response.ranked.rating = 9999;
            response.ranked.games = 999;
            response.ranked.ladderStep = 38;
            response.ranked.diamonds = 0;
            Require(session.Ranked.Rating == 1300 && session.Ranked.Games == 10 && session.Ranked.LadderStep == 12 && session.Ranked.Diamonds == 4500000000L, "Mutable JSON must not mutate the session's read-only ladder snapshot.");

            CleanDuelRankedData valid = Data();
            valid.isSimulation = true;
            Require(CleanDuelRankedSnapshot.FromServer(valid).IsSimulation, "Simulation state must be retained for explicit labeling.");
            valid.rating = 9000;
            Require(CleanDuelRankedSnapshot.FromServer(valid).DisplayName == "Bronze 4 — Icarus", "Hidden MMR must not determine visible progression.");
            valid = Data(38);
            valid.rating = 0;
            Require(CleanDuelRankedSnapshot.FromServer(valid).DisplayName == "Master 1 — Zeus", "Low hidden MMR must not overwrite a server's visible level.");
            Reject(data => data.progressionVersion = 0, "Legacy progression must not be invented from Elo.");
            Reject(data => data.progressionVersion = 2, "Unknown progression versions must not be misread.");
            Reject(data => data.ladderStep = 39, "Out-of-range ladder step accepted.");
            Reject(data => data.level = 1, "Inconsistent tier level accepted.");
            Reject(data => data.tierLevels = 6, "Inconsistent tier size accepted.");
            Reject(data => data.rankId = "zeus", "Inconsistent rank ID accepted.");
            Reject(data => data.rankName = "Gold", "Inconsistent material tier accepted.");
            Reject(data => data.rankPatronName = "Athena", "Inconsistent patron accepted.");
            Reject(data => data.levelWins = 4, "Unapplied promotion must not be shown.");
            Reject(data => data.levelLosses = 3, "Unapplied demotion must not be shown.");
            Reject(data => data.winsRequired = 5, "Unexpected promotion threshold accepted.");
            Reject(data => data.lossesRequired = 4, "Unexpected demotion threshold accepted.");
            Reject(data => data.levelWins = 1, "Counters cannot exceed recorded wins.");
            Reject(data => data.games = 1, "Inconsistent match totals accepted.");
            Reject(data => data.rating = -1, "Negative hidden MMR accepted.");
            Reject(data => data.diamonds = -1, "Negative wallet accepted.");
            Reject(data => data.diamonds = 9007199254740992L, "Unsafe JSON integer wallet accepted.");
            Reject(data => data.highestStep = -1, "Highest step cannot be below current step.");
            Reject(data => data.highestStep = 39, "Out-of-range highest step accepted.");
            Reject(data => data.lastRankedAt = "not-a-date", "Malformed inactivity date accepted.");
            Reject(data => data.lastRankedAt = "2026-10-01T12:00:00.000Z", "Inactivity dates must be paired.");
            Reject(data => { data.lastRankedAt = "2026-10-01T12:00:00.000Z"; data.nextDecayAt = "2026-09-01T12:00:00.000Z"; }, "Decay cannot precede last ranked play.");
            Reject(data => { data.wins = int.MaxValue; data.losses = int.MaxValue; data.draws = 2; }, "Overflowed match totals accepted.");
        }

        private static void AssertMissingAndDisabledRanked()
        {
            CleanDuelAccessSession legacy = Session(true, null);
            Require(legacy.Ranked == null, "An older server must not cause the client to invent progression.");
            CleanDuelRankedData oldData = Data();
            oldData.progressionVersion = 0;
            oldData.rating = 2500;
            Require(Session(true, oldData).Ranked == null, "Legacy Elo data must not create a visible rank.");
            CleanDuelAccessSession disabled = Session(false, Data());
            Require(!disabled.RankedEnabled && disabled.Ranked == null, "Disabled ranked access must not expose a ladder snapshot.");
            GameObject root = new GameObject("Disabled Ranked Smoke");
            try
            {
                CleanDuelRankedView view = new CleanDuelRankedView();
                view.Build(root.transform, disabled);
                Require(!Field<Button>(view, "badgeButton").interactable, "Ranked button must be disabled by account settings.");
                view.SetOpen(true);
                Require(!view.IsOpen, "Programmatic opening must also respect disabled ranked access.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
            GameObject legacyRoot = new GameObject("Legacy Ranked Smoke");
            try
            {
                CleanDuelRankedView view = new CleanDuelRankedView();
                view.Build(legacyRoot.transform, legacy);
                view.SetOpen(true);
                Require(FindText(legacyRoot, "Rank Badge Subtitle").text.Contains("niet beschikbaar"), "Unavailable progression must be labeled instead of inventing a level.");
                Require(FindText(legacyRoot, "Ranked Summary").text.Contains("geldige niveaugegevens"), "Legacy servers must explain missing progression data.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(legacyRoot);
            }
        }

        private static void AssertDisplay()
        {
            GameObject root = new GameObject("Ranked Display Smoke");
            try
            {
                CleanDuelRankedData data = Data(12);
                data.isSimulation = true;
                data.games = 6;
                data.wins = 3;
                data.losses = 2;
                data.draws = 1;
                data.levelWins = 2;
                data.levelLosses = 1;
                data.diamonds = 1400;
                data.lastRankedAt = "2026-10-19T12:00:00.000Z";
                data.nextDecayAt = "2026-11-18T12:00:00.000Z";
                CleanDuelRankedView view = new CleanDuelRankedView();
                view.Build(root.transform, Session(true, data));
                Require(!view.IsOpen, "Ranked details must not cover the board by default.");
                Field<Button>(view, "badgeButton").onClick.Invoke();
                Require(view.IsOpen, "Ranked badge must open the rank gallery.");
                Require(FindText(root, "Rank Badge Title").text == "GOLD 3", "Badge title must show the material tier and authoritative level.");
                Require(FindText(root, "Rank Badge Subtitle").text.Contains("Athena"), "Badge subtitle must retain the Greek patron.");
                Require(FindText(root, "Ranked Summary").text == "Gold 3 — Athena", "Rank summary must include tier, level and patron.");
                Require(FindText(root, "Ranked Season").text.Contains("SIMULATIESEIZOEN"), "Simulation ratings must never masquerade as real ladder ratings.");
                Require(FindText(root, "Ranked Practice Notice").text.Contains("dit duel verandert je rang of diamanten niet"), "Local practice must explain that progression and wallet are unchanged.");
                Require(FindText(root, "Ranked Practice Notice").text.Contains("opnieuw promoveren wordt beloond"), "Every promotion must be rewarded, not only new personal bests.");
                Require(FindText(root, "Ranked Level Progress").text.Contains("2 / 4 zeges") && FindText(root, "Ranked Level Progress").text.Contains("1 / 3 nederlagen"), "Promotion and demotion counters are incorrect.");
                Require(FindText(root, "Rank Progress Text").text.Contains("Nog 2 zeges tot Gold 2 — Athena"), "Next-level preview must be driven by the authoritative ladder step.");
                Require(FindText(root, "Ranked Wallet").text == "Simulatiewallet: 1.400", "Simulation wallet must be explicitly labeled and formatted.");
                Require(FindText(root, "Ranked Decay").text.Contains("18-11-2026 12:00 UTC"), "Server inactivity deadline must be visible.");
                for (int index = 0; index < CleanDuelRankCatalog.Count; index++)
                {
                    CleanDuelRankDefinition rank = CleanDuelRankCatalog.At(index);
                    Transform card = root.transform.Find("Icarus Ranked/Ranked Overlay/Ranked Panel/Rank Gallery " + rank.Id);
                    Require(card != null, "Missing rank gallery card: " + rank.Id);
                    Require(FindText(card.gameObject, "Rank Name").text == rank.Name.ToUpperInvariant(), "Gallery tier must remain primary.");
                    Require(FindText(card.gameObject, "Rank Patron").text == rank.PatronName, "Gallery Greek patron mismatch.");
                    Require(FindText(card.gameObject, "Rank Levels").text.Contains(rank.Levels + " niveaus"), "Gallery must show each tier's level count.");
                }
                Text[] labels = root.GetComponentsInChildren<Text>(true);
                for (int index = 0; index < labels.Length; index++) Require(!labels[index].text.Contains("Elo") && !labels[index].text.Contains("MMR"), "Hidden matchmaking rating must not be presented as progression.");
                Button[] buttons = root.GetComponentsInChildren<Button>(true);
                bool closed = false;
                for (int index = 0; index < buttons.Length; index++)
                {
                    if (buttons[index].name != "Close Ranked") continue;
                    buttons[index].onClick.Invoke();
                    closed = true;
                }
                Require(closed && !view.IsOpen, "Ranked gallery must close without changing duel state.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static void AssertBoardClicksBlocked()
        {
            GameObject root = new GameObject("Ranked Board Input Smoke");
            root.SetActive(false);
            CleanDuelBootstrap bootstrap = root.AddComponent<CleanDuelBootstrap>();
            FieldInfo viewField = typeof(CleanDuelBootstrap).GetField("rankedView", BindingFlags.NonPublic | BindingFlags.Instance);
            try
            {
                CleanDuelRankedView view = new CleanDuelRankedView();
                view.Build(root.transform, Session(true, Data()));
                view.SetOpen(true);
                viewField.SetValue(bootstrap, view);
                // An invalid board slot would throw if it reached the gameplay code.
                bootstrap.HandleZoneClicked(CleanPlayerId.Local, CleanZoneKind.Monster, int.MaxValue);
                Require(view.IsOpen, "An underlying board click must not close the rank gallery.");
            }
            finally
            {
                viewField.SetValue(bootstrap, null);
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static CleanDuelRankedData Data(int step = 0)
        {
            CleanDuelRankDefinition rank = CleanDuelRankCatalog.ForStep(step);
            return new CleanDuelRankedData
            {
                seasonId = "ranked-v1", rating = 1000, progressionVersion = 1,
                ladderStep = step, level = CleanDuelRankCatalog.LevelForStep(step), tierLevels = rank.Levels,
                rankId = rank.Id, rankName = rank.Name, rankPatronName = rank.PatronName,
                winsRequired = 4, lossesRequired = 3, highestStep = step
            };
        }

        private static void Reject(Action<CleanDuelRankedData> mutate, string message)
        {
            CleanDuelRankedData data = Data();
            mutate(data);
            Require(CleanDuelRankedSnapshot.FromServer(data) == null, message);
        }

        private static CleanDuelAccessSession Session(bool enabled, CleanDuelRankedData ranked)
        {
            CleanDuelAccessSession session;
            string error;
            Require(CleanDuelAccessSession.TryCreate(new CleanDuelAccessResponse
            {
                decision = new CleanDuelAccessDecision { allowed = true },
                user = new CleanDuelAccessUser { id = "ranked-test" },
                settings = new CleanDuelAccessSettings { rankedEnabled = enabled },
                ranked = ranked
            }, out session, out error), "Could not create ranked test session.");
            return session;
        }

        private static Text FindText(GameObject root, string name)
        {
            Text[] texts = root.GetComponentsInChildren<Text>(true);
            for (int index = 0; index < texts.Length; index++) if (texts[index].name == name) return texts[index];
            throw new InvalidOperationException("Missing ranked label: " + name);
        }

        private static T Field<T>(object instance, string name)
        {
            return (T)instance.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(instance);
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }
    }
}
