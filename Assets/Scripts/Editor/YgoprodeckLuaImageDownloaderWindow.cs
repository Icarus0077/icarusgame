using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace IcarusGame.EditorTools
{
    public sealed class YgoprodeckLuaImageDownloaderWindow : EditorWindow
    {
        private const string CardInfoUrl = "https://db.ygoprodeck.com/api/v7/cardinfo.php";
        private const string FullImageBaseUrl = "https://images.ygoprodeck.com/images/cards/";
        private const string SmallImageBaseUrl = "https://images.ygoprodeck.com/images/cards_small/";
        private const string CardInfoCacheFileName = "ygoprodeck_cardinfo_cache.json";
        private const string PrefPrefix = "IcarusGame.YgoprodeckLuaImageDownloader.";
        private const int YgoprodeckHardRateLimitPerSecond = 20;
        private const int DefaultRequestsPerSecond = 8;

        private static readonly Regex CardScriptFileNameRegex =
            new Regex(@"^c(?<id>\d{4,10})\.lua(?:\.txt)?$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex ExplicitCardIdRegex =
            new Regex(@"\b(?:cardId|card_id|cardCode|card_code|passcode|code)\s*=\s*(?<id>\d{4,10})\b",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex StringIdRegex =
            new Regex(@"aux\.Stringid\s*\(\s*(?<id>\d{4,10})\s*,", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly string[] ForbiddenProjectFolders =
        {
            "Library",
            "Temp",
            "Obj",
            "Build",
            "Builds",
            "Logs"
        };

        private string luaFolder = "Assets";
        private string outputFolder = "Assets/YGOProDeck/CardImages";
        private string cacheFolder = "Assets/YGOProDeck";
        private bool useCardDatabaseCache = true;
        private bool refreshCardDatabaseBeforeDownload;
        private bool readExplicitIdsFromContents;
        private bool downloadSmallImages;
        private int requestsPerSecond = DefaultRequestsPerSecond;
        private Vector2 scrollPosition;
        private ScanResult lastScan;
        private string status = "Ready.";
        private string lastRunSummary = string.Empty;
        private CancellationTokenSource cancellation;
        private bool isRunning;

        [MenuItem("Tools/Icarus/YGOPRODeck/Lua Image Downloader")]
        public static void Open()
        {
            GetWindow<YgoprodeckLuaImageDownloaderWindow>("YGOPRODeck Images");
        }

        private void OnEnable()
        {
            luaFolder = EditorPrefs.GetString(PrefPrefix + nameof(luaFolder), luaFolder);
            outputFolder = EditorPrefs.GetString(PrefPrefix + nameof(outputFolder), outputFolder);
            cacheFolder = EditorPrefs.GetString(PrefPrefix + nameof(cacheFolder), cacheFolder);
            useCardDatabaseCache = EditorPrefs.GetBool(PrefPrefix + nameof(useCardDatabaseCache), useCardDatabaseCache);
            refreshCardDatabaseBeforeDownload = EditorPrefs.GetBool(PrefPrefix + nameof(refreshCardDatabaseBeforeDownload), refreshCardDatabaseBeforeDownload);
            readExplicitIdsFromContents = EditorPrefs.GetBool(PrefPrefix + nameof(readExplicitIdsFromContents), readExplicitIdsFromContents);
            downloadSmallImages = EditorPrefs.GetBool(PrefPrefix + nameof(downloadSmallImages), downloadSmallImages);
            requestsPerSecond = EditorPrefs.GetInt(PrefPrefix + nameof(requestsPerSecond), requestsPerSecond);
            requestsPerSecond = Mathf.Clamp(requestsPerSecond, 1, YgoprodeckHardRateLimitPerSecond);
        }

        private void OnDisable()
        {
            SavePreferences();
            if (isRunning && cancellation != null)
            {
                cancellation.Cancel();
            }
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            EditorGUILayout.LabelField("Lua scan", EditorStyles.boldLabel);
            DrawFolderField("Lua folder", ref luaFolder, false);
            readExplicitIdsFromContents = EditorGUILayout.Toggle("Read explicit IDs in files", readExplicitIdsFromContents);

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Downloads", EditorStyles.boldLabel);
            DrawFolderField("Image output folder", ref outputFolder, true);
            downloadSmallImages = EditorGUILayout.Toggle("Download small images", downloadSmallImages);
            requestsPerSecond = EditorGUILayout.IntSlider("Requests per second", requestsPerSecond, 1, YgoprodeckHardRateLimitPerSecond);

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("YGOPRODeck card data cache", EditorStyles.boldLabel);
            useCardDatabaseCache = EditorGUILayout.Toggle("Use card database cache", useCardDatabaseCache);
            using (new EditorGUI.DisabledScope(!useCardDatabaseCache))
            {
                DrawFolderField("Cache folder", ref cacheFolder, true);
                refreshCardDatabaseBeforeDownload = EditorGUILayout.Toggle("Refresh before download", refreshCardDatabaseBeforeDownload);
            }

            EditorGUILayout.Space(12f);
            using (new EditorGUI.DisabledScope(isRunning))
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Scan Lua Files", GUILayout.Height(28f)))
                {
                    StartScan();
                }

                if (GUILayout.Button("Download Missing Images", GUILayout.Height(28f)))
                {
                    StartDownload();
                }

                EditorGUILayout.EndHorizontal();
            }

            using (new EditorGUI.DisabledScope(!isRunning))
            {
                if (GUILayout.Button("Stop Download", GUILayout.Height(24f)))
                {
                    cancellation?.Cancel();
                    status = "Stopping after current request...";
                }
            }

            EditorGUILayout.Space(12f);
            EditorGUILayout.HelpBox(status, MessageType.Info);
            if (!string.IsNullOrEmpty(lastRunSummary))
            {
                EditorGUILayout.HelpBox(lastRunSummary, MessageType.None);
            }

            DrawScanSummary();
            EditorGUILayout.EndScrollView();
        }

        private void DrawFolderField(string label, ref string path, bool requireAssetsPath)
        {
            EditorGUILayout.BeginHorizontal();
            path = EditorGUILayout.TextField(label, path);
            if (GUILayout.Button("...", GUILayout.Width(32f)))
            {
                string startFolder = ToAbsoluteProjectPath(path);
                if (!Directory.Exists(startFolder))
                {
                    startFolder = Application.dataPath;
                }

                string selected = EditorUtility.OpenFolderPanel(label, startFolder, string.Empty);
                if (!string.IsNullOrEmpty(selected))
                {
                    if (TryGetProjectRelativePath(selected, out string relativePath) &&
                        (!requireAssetsPath || IsAssetsProjectPath(relativePath)) &&
                        !ContainsForbiddenProjectFolder(relativePath))
                    {
                        path = relativePath;
                        SavePreferences();
                    }
                    else
                    {
                        status = requireAssetsPath
                            ? "Choose a folder under Assets and outside Library, Temp, Obj, Build, Builds, and Logs."
                            : "Choose a folder inside this project and outside Library, Temp, Obj, Build, Builds, and Logs.";
                    }
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawScanSummary()
        {
            if (lastScan == null)
            {
                return;
            }

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Last scan", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Lua files scanned", lastScan.LuaFileCount.ToString());
            EditorGUILayout.LabelField("Files with IDs", lastScan.FilesWithIds.ToString());
            EditorGUILayout.LabelField("Unique card IDs", lastScan.CardIds.Count.ToString());

            if (TryValidateFolders(false, out _) && lastScan.CardIds.Count > 0)
            {
                int existing = CountExistingImages(lastScan.CardIds);
                EditorGUILayout.LabelField("Images already present", existing.ToString());
                EditorGUILayout.LabelField("Images missing", Mathf.Max(0, lastScan.CardIds.Count - existing).ToString());
            }

            int sampleCount = Mathf.Min(8, lastScan.CardIds.Count);
            if (sampleCount > 0)
            {
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < sampleCount; i++)
                {
                    if (i > 0)
                    {
                        builder.Append(", ");
                    }

                    builder.Append(lastScan.CardIds[i]);
                }

                EditorGUILayout.LabelField("Sample IDs", builder.ToString());
            }
        }

        private void StartScan()
        {
            SavePreferences();
            lastRunSummary = string.Empty;

            try
            {
                lastScan = ScanLuaFiles();
                status = "Scan complete. Found " + lastScan.CardIds.Count + " unique card IDs from " + lastScan.LuaFileCount + " Lua files.";
            }
            catch (Exception exception)
            {
                status = "Scan failed: " + exception.Message;
                Debug.LogException(exception);
            }
        }

        private void StartDownload()
        {
            if (isRunning)
            {
                return;
            }

            SavePreferences();
            lastRunSummary = string.Empty;
            cancellation = new CancellationTokenSource();
            _ = RunDownloadAsync(cancellation.Token);
        }

        private async Task RunDownloadAsync(CancellationToken token)
        {
            int skipped = 0;
            int downloaded = 0;
            int failed = 0;

            isRunning = true;

            try
            {
                lastScan = ScanLuaFiles();
                if (lastScan.CardIds.Count == 0)
                {
                    status = "No card IDs found. Card scripts are usually named like c12345678.lua.";
                    return;
                }

                string outputAbsolutePath = ToAbsoluteProjectPath(outputFolder);
                Directory.CreateDirectory(outputAbsolutePath);

                Dictionary<long, CardImageInfo> imageLookup = null;
                if (useCardDatabaseCache)
                {
                    status = "Loading YGOPRODeck card database cache...";
                    Repaint();
                    string cardInfoJson = await LoadOrFetchCardInfoJsonAsync(token);
                    imageLookup = BuildImageLookup(cardInfoJson);
                }

                int delayMs = Mathf.CeilToInt(1000f / Mathf.Clamp(requestsPerSecond, 1, YgoprodeckHardRateLimitPerSecond));
                IReadOnlyList<long> cardIds = lastScan.CardIds;

                for (int index = 0; index < cardIds.Count; index++)
                {
                    token.ThrowIfCancellationRequested();

                    long cardId = cardIds[index];
                    string outputFile = Path.Combine(outputAbsolutePath, cardId + ".jpg");
                    if (File.Exists(outputFile) && new FileInfo(outputFile).Length > 0L)
                    {
                        skipped++;
                        continue;
                    }

                    float progress = cardIds.Count == 0 ? 1f : (float)index / cardIds.Count;
                    if (EditorUtility.DisplayCancelableProgressBar(
                            "YGOPRODeck image download",
                            "Downloading " + cardId + " (" + (index + 1) + "/" + cardIds.Count + ")",
                            progress))
                    {
                        cancellation?.Cancel();
                        token.ThrowIfCancellationRequested();
                    }

                    if (index > 0)
                    {
                        await Task.Delay(delayMs, token);
                    }

                    string imageUrl = ResolveImageUrl(cardId, imageLookup);
                    bool success = await DownloadImageAsync(imageUrl, outputFile, token);
                    if (success)
                    {
                        downloaded++;
                    }
                    else
                    {
                        failed++;
                    }

                    status = "Downloaded " + downloaded + ", skipped " + skipped + ", failed " + failed + ".";
                    Repaint();
                }

                AssetDatabase.Refresh();
                status = "Download complete.";
                lastRunSummary = "Downloaded: " + downloaded + "\nSkipped existing: " + skipped + "\nFailed: " + failed + "\nOutput: " + outputFolder;
            }
            catch (OperationCanceledException)
            {
                status = "Download stopped.";
                lastRunSummary = "Downloaded: " + downloaded + "\nSkipped existing: " + skipped + "\nFailed: " + failed;
            }
            catch (Exception exception)
            {
                status = "Download failed: " + exception.Message;
                Debug.LogException(exception);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                isRunning = false;
                cancellation?.Dispose();
                cancellation = null;
                Repaint();
            }
        }

        private ScanResult ScanLuaFiles()
        {
            if (!TryValidateFolders(true, out string validationError))
            {
                throw new InvalidOperationException(validationError);
            }

            string luaAbsolutePath = ToAbsoluteProjectPath(luaFolder);
            SortedDictionary<long, List<string>> sourcesById = new SortedDictionary<long, List<string>>();
            int luaFileCount = 0;
            int filesWithIds = 0;

            foreach (string file in EnumerateLuaFiles(luaAbsolutePath))
            {
                if (!TryGetProjectRelativePath(file, out string projectRelativePath) ||
                    ContainsForbiddenProjectFolder(projectRelativePath))
                {
                    continue;
                }

                luaFileCount++;
                HashSet<long> idsInFile = new HashSet<long>();
                Match fileNameMatch = CardScriptFileNameRegex.Match(Path.GetFileName(file));
                if (fileNameMatch.Success && TryParseCardId(fileNameMatch.Groups["id"].Value, out long fileNameId))
                {
                    idsInFile.Add(fileNameId);
                }

                if (readExplicitIdsFromContents)
                {
                    AddExplicitIdsFromFile(file, idsInFile);
                }

                if (idsInFile.Count == 0)
                {
                    continue;
                }

                filesWithIds++;
                foreach (long cardId in idsInFile)
                {
                    if (!sourcesById.TryGetValue(cardId, out List<string> sources))
                    {
                        sources = new List<string>();
                        sourcesById.Add(cardId, sources);
                    }

                    sources.Add(projectRelativePath);
                }
            }

            return new ScanResult(luaFileCount, filesWithIds, sourcesById);
        }

        private void AddExplicitIdsFromFile(string file, HashSet<long> idsInFile)
        {
            string text = File.ReadAllText(file, Encoding.UTF8);
            foreach (Match match in ExplicitCardIdRegex.Matches(text))
            {
                if (TryParseCardId(match.Groups["id"].Value, out long cardId))
                {
                    idsInFile.Add(cardId);
                }
            }

            foreach (Match match in StringIdRegex.Matches(text))
            {
                if (TryParseCardId(match.Groups["id"].Value, out long cardId))
                {
                    idsInFile.Add(cardId);
                }
            }
        }

        private IEnumerable<string> EnumerateLuaFiles(string rootFolder)
        {
            Stack<string> folders = new Stack<string>();
            folders.Push(rootFolder);

            while (folders.Count > 0)
            {
                string currentFolder = folders.Pop();
                if (!TryGetProjectRelativePath(currentFolder, out string folderProjectPath) ||
                    ContainsForbiddenProjectFolder(folderProjectPath))
                {
                    continue;
                }

                string[] files;
                try
                {
                    files = Directory.GetFiles(currentFolder, "*.lua*");
                }
                catch (Exception exception)
                {
                    Debug.LogWarning("Skipping Lua folder " + currentFolder + ": " + exception.Message);
                    continue;
                }

                foreach (string file in files)
                {
                    yield return file;
                }

                string[] childFolders;
                try
                {
                    childFolders = Directory.GetDirectories(currentFolder);
                }
                catch (Exception exception)
                {
                    Debug.LogWarning("Skipping child folders for " + currentFolder + ": " + exception.Message);
                    continue;
                }

                foreach (string childFolder in childFolders)
                {
                    if (TryGetProjectRelativePath(childFolder, out string childProjectPath) &&
                        !ContainsForbiddenProjectFolder(childProjectPath))
                    {
                        folders.Push(childFolder);
                    }
                }
            }
        }

        private async Task<string> LoadOrFetchCardInfoJsonAsync(CancellationToken token)
        {
            string cacheAbsoluteFolder = ToAbsoluteProjectPath(cacheFolder);
            string cacheFile = Path.Combine(cacheAbsoluteFolder, CardInfoCacheFileName);

            if (!refreshCardDatabaseBeforeDownload && File.Exists(cacheFile) && new FileInfo(cacheFile).Length > 0L)
            {
                return File.ReadAllText(cacheFile, Encoding.UTF8);
            }

            Directory.CreateDirectory(cacheAbsoluteFolder);
            string json = await DownloadTextAsync(CardInfoUrl, token);
            File.WriteAllText(cacheFile, json, Encoding.UTF8);

            if (TryGetProjectRelativePath(cacheFile, out string cacheProjectPath) && IsAssetsProjectPath(cacheProjectPath))
            {
                AssetDatabase.ImportAsset(cacheProjectPath, ImportAssetOptions.ForceUpdate);
            }

            return json;
        }

        private Dictionary<long, CardImageInfo> BuildImageLookup(string cardInfoJson)
        {
            Dictionary<long, CardImageInfo> lookup = new Dictionary<long, CardImageInfo>();
            if (string.IsNullOrEmpty(cardInfoJson))
            {
                return lookup;
            }

            CardInfoResponse response = JsonUtility.FromJson<CardInfoResponse>(cardInfoJson);
            if (response == null || response.data == null)
            {
                return lookup;
            }

            foreach (CardInfoCard card in response.data)
            {
                if (card == null || card.card_images == null)
                {
                    continue;
                }

                foreach (CardInfoImage image in card.card_images)
                {
                    if (image == null || image.id <= 0L || lookup.ContainsKey(image.id))
                    {
                        continue;
                    }

                    lookup.Add(image.id, new CardImageInfo
                    {
                        CardName = card.name,
                        FullUrl = image.image_url,
                        SmallUrl = image.image_url_small
                    });
                }
            }

            return lookup;
        }

        private string ResolveImageUrl(long cardId, Dictionary<long, CardImageInfo> imageLookup)
        {
            if (imageLookup != null && imageLookup.TryGetValue(cardId, out CardImageInfo info))
            {
                string url = downloadSmallImages ? info.SmallUrl : info.FullUrl;
                if (!string.IsNullOrEmpty(url))
                {
                    return url;
                }
            }

            string baseUrl = downloadSmallImages ? SmallImageBaseUrl : FullImageBaseUrl;
            return baseUrl + cardId + ".jpg";
        }

        private async Task<string> DownloadTextAsync(string url, CancellationToken token)
        {
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.timeout = 60;
                request.SetRequestHeader("User-Agent", "IcarusGameUnityEditor/1.0");
                await SendWebRequestAsync(request, token);

                if (!IsSuccessfulRequest(request))
                {
                    throw new InvalidOperationException("Request failed: " + url + " (" + request.responseCode + ") " + request.error);
                }

                return request.downloadHandler.text;
            }
        }

        private async Task<bool> DownloadImageAsync(string url, string outputFile, CancellationToken token)
        {
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.timeout = 30;
                request.SetRequestHeader("User-Agent", "IcarusGameUnityEditor/1.0");
                await SendWebRequestAsync(request, token);

                if (!IsSuccessfulRequest(request) || request.downloadHandler == null || request.downloadHandler.data == null || request.downloadHandler.data.Length == 0)
                {
                    Debug.LogWarning("Failed to download card image from " + url + ": " + request.responseCode + " " + request.error);
                    return false;
                }

                File.WriteAllBytes(outputFile, request.downloadHandler.data);
                return true;
            }
        }

        private static async Task SendWebRequestAsync(UnityWebRequest request, CancellationToken token)
        {
            UnityWebRequestAsyncOperation operation = request.SendWebRequest();
            while (!operation.isDone)
            {
                token.ThrowIfCancellationRequested();
                await Task.Delay(50, token);
            }

            token.ThrowIfCancellationRequested();
        }

        private static bool IsSuccessfulRequest(UnityWebRequest request)
        {
            return request.result == UnityWebRequest.Result.Success &&
                   request.responseCode >= 200L &&
                   request.responseCode < 300L;
        }

        private int CountExistingImages(IReadOnlyList<long> cardIds)
        {
            string outputAbsolutePath = ToAbsoluteProjectPath(outputFolder);
            if (!Directory.Exists(outputAbsolutePath))
            {
                return 0;
            }

            int count = 0;
            foreach (long cardId in cardIds)
            {
                string outputFile = Path.Combine(outputAbsolutePath, cardId + ".jpg");
                if (File.Exists(outputFile) && new FileInfo(outputFile).Length > 0L)
                {
                    count++;
                }
            }

            return count;
        }

        private bool TryValidateFolders(bool requireExistingLuaFolder, out string error)
        {
            error = string.Empty;

            if (!TryGetProjectRelativePath(ToAbsoluteProjectPath(luaFolder), out string luaRelativePath))
            {
                error = "Lua folder must be inside this project.";
                return false;
            }

            if (ContainsForbiddenProjectFolder(luaRelativePath))
            {
                error = "Lua folder cannot be inside Library, Temp, Obj, Build, Builds, or Logs.";
                return false;
            }

            if (requireExistingLuaFolder && !Directory.Exists(ToAbsoluteProjectPath(luaFolder)))
            {
                error = "Lua folder does not exist: " + luaFolder;
                return false;
            }

            if (!TryGetProjectRelativePath(ToAbsoluteProjectPath(outputFolder), out string outputRelativePath) ||
                !IsAssetsProjectPath(outputRelativePath) ||
                ContainsForbiddenProjectFolder(outputRelativePath))
            {
                error = "Image output folder must be under Assets and outside Library, Temp, Obj, Build, Builds, and Logs.";
                return false;
            }

            if (useCardDatabaseCache &&
                (!TryGetProjectRelativePath(ToAbsoluteProjectPath(cacheFolder), out string cacheRelativePath) ||
                 !IsAssetsProjectPath(cacheRelativePath) ||
                 ContainsForbiddenProjectFolder(cacheRelativePath)))
            {
                error = "Cache folder must be under Assets and outside Library, Temp, Obj, Build, Builds, and Logs.";
                return false;
            }

            return true;
        }

        private static bool TryParseCardId(string value, out long cardId)
        {
            if (long.TryParse(value, out cardId) && cardId > 0L)
            {
                return true;
            }

            cardId = 0L;
            return false;
        }

        private void SavePreferences()
        {
            EditorPrefs.SetString(PrefPrefix + nameof(luaFolder), luaFolder);
            EditorPrefs.SetString(PrefPrefix + nameof(outputFolder), outputFolder);
            EditorPrefs.SetString(PrefPrefix + nameof(cacheFolder), cacheFolder);
            EditorPrefs.SetBool(PrefPrefix + nameof(useCardDatabaseCache), useCardDatabaseCache);
            EditorPrefs.SetBool(PrefPrefix + nameof(refreshCardDatabaseBeforeDownload), refreshCardDatabaseBeforeDownload);
            EditorPrefs.SetBool(PrefPrefix + nameof(readExplicitIdsFromContents), readExplicitIdsFromContents);
            EditorPrefs.SetBool(PrefPrefix + nameof(downloadSmallImages), downloadSmallImages);
            EditorPrefs.SetInt(PrefPrefix + nameof(requestsPerSecond), requestsPerSecond);
        }

        private static string ToAbsoluteProjectPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return ProjectRoot;
            }

            if (Path.IsPathRooted(path))
            {
                return Path.GetFullPath(path);
            }

            return Path.GetFullPath(Path.Combine(ProjectRoot, path.Replace('/', Path.DirectorySeparatorChar)));
        }

        private static bool TryGetProjectRelativePath(string absolutePath, out string projectRelativePath)
        {
            string fullPath = Path.GetFullPath(absolutePath);
            string root = ProjectRoot.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            string rootWithSeparator = root + Path.DirectorySeparatorChar;

            if (string.Equals(fullPath, root, StringComparison.OrdinalIgnoreCase))
            {
                projectRelativePath = string.Empty;
                return true;
            }

            if (!fullPath.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase))
            {
                projectRelativePath = string.Empty;
                return false;
            }

            projectRelativePath = fullPath.Substring(rootWithSeparator.Length).Replace('\\', '/');
            return true;
        }

        private static bool IsAssetsProjectPath(string projectRelativePath)
        {
            return string.Equals(projectRelativePath, "Assets", StringComparison.OrdinalIgnoreCase) ||
                   projectRelativePath.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase);
        }

        private static bool ContainsForbiddenProjectFolder(string projectRelativePath)
        {
            if (string.IsNullOrEmpty(projectRelativePath))
            {
                return false;
            }

            string[] parts = projectRelativePath.Split('/');
            foreach (string part in parts)
            {
                foreach (string forbiddenFolder in ForbiddenProjectFolders)
                {
                    if (string.Equals(part, forbiddenFolder, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static string ProjectRoot
        {
            get
            {
                return Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            }
        }

        private sealed class ScanResult
        {
            public ScanResult(int luaFileCount, int filesWithIds, SortedDictionary<long, List<string>> sourcesById)
            {
                LuaFileCount = luaFileCount;
                FilesWithIds = filesWithIds;
                SourcesById = sourcesById;
                CardIds = new List<long>(sourcesById.Keys);
            }

            public int LuaFileCount { get; }
            public int FilesWithIds { get; }
            public SortedDictionary<long, List<string>> SourcesById { get; }
            public List<long> CardIds { get; }
        }

        private sealed class CardImageInfo
        {
            public string CardName;
            public string FullUrl;
            public string SmallUrl;
        }

        [Serializable]
        private sealed class CardInfoResponse
        {
            public CardInfoCard[] data;
        }

        [Serializable]
        private sealed class CardInfoCard
        {
            public string name;
            public CardInfoImage[] card_images;
        }

        [Serializable]
        private sealed class CardInfoImage
        {
            public int id;
            public string image_url;
            public string image_url_small;
        }
    }
}
