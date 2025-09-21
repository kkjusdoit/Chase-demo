using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System;

[System.Serializable]
public class ScoreSubmissionData
{
    public string device_id;
    public string player_name;
    public int score;
}

[System.Serializable]
public class ApiResponse
{
    public bool success;
    public string message;
    public bool new_record;
    public int score;
    public int previous_best;
    public int current_best;
    public int submitted_score;
    public string error;
}

[System.Serializable]
public class LeaderboardEntry
{
    public string player_name;
    public int best_score;
    public string created_at;
}

[System.Serializable]
public class LeaderboardResponse
{
    public bool success;
    public int count;
    public LeaderboardEntry[] leaderboard;
    public string error;
}

[System.Serializable]
public class PlayerData
{
    public string player_name;
    public int best_score;
    public string created_at;
    public string updated_at;
}

[System.Serializable]
public class PlayerResponse
{
    public bool success;
    public PlayerData player;
    public string message;
    public string error;
}

public class CloudScoreManager : MonoBehaviour
{
    [Header("API Configuration")]
    [SerializeField] private string apiBaseUrl = "https://chase-game-api.kkjusdoit.workers.dev";
    
    [Header("Player Settings")]
    [SerializeField] private string playerName = "Anonymous";
    private string deviceId;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;
    
    // Events
    public static event System.Action<bool, string> OnScoreSubmitted;
    public static event System.Action<LeaderboardEntry[]> OnLeaderboardLoaded;
    public static event System.Action<PlayerData> OnPlayerDataLoaded;
    public static event System.Action<string> OnError;
    
    // Single instance
    public static CloudScoreManager Instance { get; private set; }
    
    void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Generate unique device ID
        GenerateDeviceId();
        
        // Load saved player name
        LoadPlayerName();
    }
    
    void Start()
    {
        // Test API connection
        StartCoroutine(TestApiConnection());
    }
    
    private void GenerateDeviceId()
    {
        // Use Unity's device unique identifier
        deviceId = SystemInfo.deviceUniqueIdentifier;
        
        // If that fails, generate a custom one and save it
        if (string.IsNullOrEmpty(deviceId) || deviceId == "n/a")
        {
            deviceId = PlayerPrefs.GetString("CustomDeviceId", "");
            if (string.IsNullOrEmpty(deviceId))
            {
                deviceId = "device_" + System.Guid.NewGuid().ToString();
                PlayerPrefs.SetString("CustomDeviceId", deviceId);
                PlayerPrefs.Save();
            }
        }
        
        LogDebug($"Device ID: {deviceId}");
    }
    
    private void LoadPlayerName()
    {
        playerName = PlayerPrefs.GetString("PlayerName", "Anonymous");
        LogDebug($"Player Name: {playerName}");
    }
    
    public void SetPlayerName(string newName)
    {
        if (!string.IsNullOrEmpty(newName) && newName.Trim().Length > 0)
        {
            playerName = newName.Trim();
            PlayerPrefs.SetString("PlayerName", playerName);
            PlayerPrefs.Save();
            LogDebug($"Player name updated to: {playerName}");
        }
    }
    
    public string GetPlayerName()
    {
        return playerName;
    }
    
    public string GetDeviceId()
    {
        return deviceId;
    }
    
    private IEnumerator TestApiConnection()
    {
        LogDebug("Testing API connection...");
        
        using (UnityWebRequest request = UnityWebRequest.Get(apiBaseUrl + "/"))
        {
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                LogDebug("API connection successful!");
                LogDebug($"Response: {request.downloadHandler.text}");
            }
            else
            {
                LogDebug($"API connection failed: {request.error}");
                OnError?.Invoke($"API连接失败: {request.error}");
            }
        }
    }
    
    public void SubmitScore(int score)
    {
        StartCoroutine(SubmitScoreCoroutine(score));
    }
    
    private IEnumerator SubmitScoreCoroutine(int score)
    {
        LogDebug($"Submitting score: {score} for player: {playerName}");
        
        // Create submission data
        ScoreSubmissionData submissionData = new ScoreSubmissionData
        {
            device_id = deviceId,
            player_name = playerName,
            score = score
        };
        
        string jsonData = JsonUtility.ToJson(submissionData);
        LogDebug($"Submission JSON: {jsonData}");
        
        // Create request
        using (UnityWebRequest request = new UnityWebRequest(apiBaseUrl + "/submit-score", "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                LogDebug($"Score submission response: {responseText}");
                
                try
                {
                    ApiResponse response = JsonUtility.FromJson<ApiResponse>(responseText);
                    OnScoreSubmitted?.Invoke(response.success, response.message);
                    
                    if (response.new_record)
                    {
                        LogDebug("New high score recorded!");
                    }
                }
                catch (Exception e)
                {
                    LogDebug($"Failed to parse response: {e.Message}");
                    OnError?.Invoke($"响应解析失败: {e.Message}");
                }
            }
            else
            {
                LogDebug($"Score submission failed: {request.error}");
                OnError?.Invoke($"分数提交失败: {request.error}");
            }
        }
    }
    
    public void LoadLeaderboard(int limit = 10)
    {
        StartCoroutine(LoadLeaderboardCoroutine(limit));
    }
    
    private IEnumerator LoadLeaderboardCoroutine(int limit)
    {
        LogDebug($"Loading leaderboard (top {limit})...");
        
        string url = $"{apiBaseUrl}/leaderboard?limit={limit}";
        
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                LogDebug($"Leaderboard response: {responseText}");
                
                try
                {
                    LeaderboardResponse response = JsonUtility.FromJson<LeaderboardResponse>(responseText);
                    if (response.success)
                    {
                        OnLeaderboardLoaded?.Invoke(response.leaderboard);
                        LogDebug($"Loaded {response.count} leaderboard entries");
                    }
                    else
                    {
                        OnError?.Invoke($"排行榜加载失败: {response.error}");
                    }
                }
                catch (Exception e)
                {
                    LogDebug($"Failed to parse leaderboard response: {e.Message}");
                    OnError?.Invoke($"排行榜数据解析失败: {e.Message}");
                }
            }
            else
            {
                LogDebug($"Leaderboard request failed: {request.error}");
                OnError?.Invoke($"排行榜请求失败: {request.error}");
            }
        }
    }
    
    public void LoadPlayerData()
    {
        StartCoroutine(LoadPlayerDataCoroutine());
    }
    
    private IEnumerator LoadPlayerDataCoroutine()
    {
        LogDebug($"Loading player data for device: {deviceId}");
        
        string url = $"{apiBaseUrl}/player-score/{deviceId}";
        
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                LogDebug($"Player data response: {responseText}");
                
                try
                {
                    PlayerResponse response = JsonUtility.FromJson<PlayerResponse>(responseText);
                    if (response.success)
                    {
                        OnPlayerDataLoaded?.Invoke(response.player);
                        if (response.player != null)
                        {
                            LogDebug($"Player best score: {response.player.best_score}");
                        }
                        else
                        {
                            LogDebug("No player data found (new player)");
                        }
                    }
                    else
                    {
                        OnError?.Invoke($"玩家数据加载失败: {response.error}");
                    }
                }
                catch (Exception e)
                {
                    LogDebug($"Failed to parse player data response: {e.Message}");
                    OnError?.Invoke($"玩家数据解析失败: {e.Message}");
                }
            }
            else
            {
                LogDebug($"Player data request failed: {request.error}");
                OnError?.Invoke($"玩家数据请求失败: {request.error}");
            }
        }
    }
    
    // Public method to get cloud high score
    public void GetCloudHighScore(System.Action<int> callback)
    {
        StartCoroutine(GetCloudHighScoreCoroutine(callback));
    }
    
    private IEnumerator GetCloudHighScoreCoroutine(System.Action<int> callback)
    {
        string url = $"{apiBaseUrl}/player-score/{deviceId}";
        
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();
            
            int cloudScore = 0;
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    PlayerResponse response = JsonUtility.FromJson<PlayerResponse>(request.downloadHandler.text);
                    if (response.success && response.player != null)
                    {
                        cloudScore = response.player.best_score;
                    }
                }
                catch (Exception e)
                {
                    LogDebug($"Failed to get cloud high score: {e.Message}");
                }
            }
            
            callback?.Invoke(cloudScore);
        }
    }
    
    private void LogDebug(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[CloudScoreManager] {message}");
        }
    }
    
    // Event subscriptions for testing
    void OnEnable()
    {
        OnScoreSubmitted += HandleScoreSubmitted;
        OnLeaderboardLoaded += HandleLeaderboardLoaded;
        OnPlayerDataLoaded += HandlePlayerDataLoaded;
        OnError += HandleError;
    }
    
    void OnDisable()
    {
        OnScoreSubmitted -= HandleScoreSubmitted;
        OnLeaderboardLoaded -= HandleLeaderboardLoaded;
        OnPlayerDataLoaded -= HandlePlayerDataLoaded;
        OnError -= HandleError;
    }
    
    private void HandleScoreSubmitted(bool success, string message)
    {
        LogDebug($"Score submitted: {success} - {message}");
    }
    
    private void HandleLeaderboardLoaded(LeaderboardEntry[] entries)
    {
        LogDebug($"Leaderboard loaded with {entries.Length} entries");
        for (int i = 0; i < entries.Length; i++)
        {
            LogDebug($"{i + 1}. {entries[i].player_name}: {entries[i].best_score}");
        }
    }
    
    private void HandlePlayerDataLoaded(PlayerData playerData)
    {
        if (playerData != null)
        {
            LogDebug($"Player data loaded: {playerData.player_name} - Best: {playerData.best_score}");
        }
        else
        {
            LogDebug("No player data found");
        }
    }
    
    private void HandleError(string errorMessage)
    {
        LogDebug($"Error: {errorMessage}");
    }
} 