using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// ランキングシステムを管理するクラス
/// </summary>
public class RankingManager : MonoBehaviour
{
    private static RankingManager instance;
    public static RankingManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("RankingManager");
                instance = go.AddComponent<RankingManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    private const int MAX_RANKING_ENTRIES = 100;  // 各モデルの最大ランキング数
    private const string RANKING_KEY_PREFIX = "Ranking_Model_";

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// ランキングエントリを追加します
    /// </summary>
    public void AddRankingEntry(RankingEntry entry)
    {
        // 既存のランキングデータを読み込む
        RankingData data = LoadRankingData(entry.modelId);

        // 新しいエントリを追加
        data.entries.Add(entry);

        // 整定時間でソート（昇順）
        data.entries = data.entries
            .OrderBy(e => e.settlingTime)
            .Take(MAX_RANKING_ENTRIES)
            .ToList();

        // 保存
        SaveRankingData(entry.modelId, data);

        Debug.Log($"ランキングに追加: {entry.playerName} - {entry.settlingTime:F2}秒");
    }

    /// <summary>
    /// 指定モデルのランキングを取得します
    /// </summary>
    public List<RankingEntry> GetRanking(int modelId, int maxCount = 10)
    {
        RankingData data = LoadRankingData(modelId);
        return data.entries.Take(maxCount).ToList();
    }

    /// <summary>
    /// すべてのランキングを取得します
    /// </summary>
    public List<RankingEntry> GetAllRankings(int modelId)
    {
        RankingData data = LoadRankingData(modelId);
        return data.entries;
    }

    /// <summary>
    /// プレイヤーの順位を取得します
    /// </summary>
    public int GetPlayerRank(int modelId, string playerName, float settlingTime)
    {
        RankingData data = LoadRankingData(modelId);
        
        for (int i = 0; i < data.entries.Count; i++)
        {
            if (data.entries[i].playerName == playerName && 
                Mathf.Approximately(data.entries[i].settlingTime, settlingTime))
            {
                return i + 1;  // 1位から始まる
            }
        }
        
        return -1;  // 見つからない
    }

    /// <summary>
    /// ランキングをクリアします
    /// </summary>
    public void ClearRanking(int modelId)
    {
        string key = RANKING_KEY_PREFIX + modelId;
        PlayerPrefs.DeleteKey(key);
        PlayerPrefs.Save();
        Debug.Log($"モデル{modelId}のランキングをクリアしました");
    }

    /// <summary>
    /// すべてのランキングをクリアします
    /// </summary>
    public void ClearAllRankings()
    {
        for (int i = 0; i < 3; i++)  // 3つのモデル
        {
            ClearRanking(i);
        }
        Debug.Log("すべてのランキングをクリアしました");
    }

    /// <summary>
    /// ランキングデータを読み込みます
    /// </summary>
    private RankingData LoadRankingData(int modelId)
    {
        string key = RANKING_KEY_PREFIX + modelId;
        string json = PlayerPrefs.GetString(key, "");

        if (string.IsNullOrEmpty(json))
        {
            return new RankingData();
        }

        try
        {
            return JsonUtility.FromJson<RankingData>(json);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"ランキングデータの読み込みに失敗: {e.Message}");
            return new RankingData();
        }
    }

    /// <summary>
    /// ランキングデータを保存します
    /// </summary>
    private void SaveRankingData(int modelId, RankingData data)
    {
        string key = RANKING_KEY_PREFIX + modelId;
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(key, json);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// ランキングが存在するかチェックします
    /// </summary>
    public bool HasRanking(int modelId)
    {
        RankingData data = LoadRankingData(modelId);
        return data.entries.Count > 0;
    }

    /// <summary>
    /// 最高記録を取得します
    /// </summary>
    public RankingEntry? GetBestRecord(int modelId)
    {
        RankingData data = LoadRankingData(modelId);
        if (data.entries.Count > 0)
        {
            return data.entries[0];
        }
        return null;
    }
}

/// <summary>
/// ランキングエントリ
/// </summary>
[System.Serializable]
public struct RankingEntry
{
    public string playerName;     // プレイヤー名
    public float settlingTime;    // 整定時間
    public float kp;              // 使用したKp
    public float ki;              // 使用したKi
    public float kd;              // 使用したKd
    public float targetValue;     // 目標値
    public string timestamp;      // 達成日時
    public int modelId;           // モデルID (0: Pendulum, 1: Helicopter, 2: Crane)
    public float totalScore;      // 総合スコア

    public RankingEntry(string name, float time, float kpValue, float kiValue, float kdValue, 
                       float target, int model, float score)
    {
        playerName = name;
        settlingTime = time;
        kp = kpValue;
        ki = kiValue;
        kd = kdValue;
        targetValue = target;
        modelId = model;
        totalScore = score;
        timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm");
    }

    public override string ToString()
    {
        return $"{playerName}: {settlingTime:F2}秒 (Kp={kp}, Ki={ki}, Kd={kd})";
    }
}

/// <summary>
/// ランキングデータ
/// </summary>
[System.Serializable]
public class RankingData
{
    public List<RankingEntry> entries = new List<RankingEntry>();
}
