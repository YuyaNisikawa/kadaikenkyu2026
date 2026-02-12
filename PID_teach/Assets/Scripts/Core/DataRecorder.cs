using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// シミュレーション中のデータを記録するクラス
/// </summary>
public class DataRecorder
{
    private List<DataPoint> dataPoints = new List<DataPoint>();

    // グラフ描画用の公開プロパティ
    public List<float> TimeData => dataPoints.Select(p => p.time).ToList();
    public List<float> CurrentValueData => dataPoints.Select(p => p.currentValue).ToList();
    public List<float> TargetValue => dataPoints.Select(p => p.targetValue).ToList();
    public List<float> PTermData => dataPoints.Select(p => p.pTerm).ToList();
    public List<float> ITermData => dataPoints.Select(p => p.iTerm).ToList();
    public List<float> DTermData => dataPoints.Select(p => p.dTerm).ToList();
    private float recordInterval = 0.05f;  // 記録間隔（秒）
    private float lastRecordTime = 0f;
    private int maxDataPoints = 2000;  // 最大データポイント数

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="interval">記録間隔（秒）</param>
    /// <param name="maxPoints">最大データポイント数</param>
    public DataRecorder(float interval = 0.05f, int maxPoints = 2000)
    {
        recordInterval = interval;
        maxDataPoints = maxPoints;
    }

    /// <summary>
    /// データポイントを記録します
    /// </summary>
    /// <param name="time">経過時間</param>
    /// <param name="currentValue">現在値</param>
    /// <param name="targetValue">目標値</param>
    /// <param name="debugInfo">PIDデバッグ情報</param>
    /// <param name="forceRecord">強制的に記録するか</param>
    public void Record(float time, float currentValue, float targetValue,
                      PIDDebugInfo debugInfo, bool forceRecord = false)
    {
        // 記録間隔チェック
        if (!forceRecord && time - lastRecordTime < recordInterval)
        {
            return;
        }

        // 最大データポイント数チェック
        if (dataPoints.Count >= maxDataPoints)
        {
            // 古いデータを削除（リングバッファ的な動作）
            dataPoints.RemoveAt(0);
        }

        // データポイントを追加
        DataPoint point = new DataPoint
        {
            time = time,
            currentValue = currentValue,
            targetValue = targetValue,
            pTerm = debugInfo.pTerm,
            iTerm = debugInfo.iTerm,
            dTerm = debugInfo.dTerm,
            controlOutput = debugInfo.output
        };

        dataPoints.Add(point);
        lastRecordTime = time;
    }

    /// <summary>
    /// 記録されたすべてのデータポイントを取得します
    /// </summary>
    /// <returns>データポイントのリスト</returns>
    public List<DataPoint> GetData()
    {
        return new List<DataPoint>(dataPoints);  // コピーを返す
    }

    /// <summary>
    /// データポイントの数を取得します
    /// </summary>
    public int GetDataCount()
    {
        return dataPoints.Count;
    }

    /// <summary>
    /// 記録をクリアします
    /// </summary>
    public void Clear()
    {
        dataPoints.Clear();
        lastRecordTime = 0f;
    }

    /// <summary>
    /// 記録間隔を設定します
    /// </summary>
    public void SetRecordInterval(float interval)
    {
        recordInterval = Mathf.Max(0.001f, interval);
    }

    /// <summary>
    /// 最大データポイント数を設定します
    /// </summary>
    public void SetMaxDataPoints(int maxPoints)
    {
        maxDataPoints = Mathf.Max(100, maxPoints);
    }

    /// <summary>
    /// 特定の時間範囲のデータを取得します
    /// </summary>
    public List<DataPoint> GetDataInRange(float startTime, float endTime)
    {
        List<DataPoint> result = new List<DataPoint>();

        foreach (var point in dataPoints)
        {
            if (point.time >= startTime && point.time <= endTime)
            {
                result.Add(point);
            }
        }

        return result;
    }

    /// <summary>
    /// 最後のデータポイントを取得します
    /// </summary>
    public DataPoint? GetLastDataPoint()
    {
        if (dataPoints.Count > 0)
        {
            return dataPoints[dataPoints.Count - 1];
        }
        return null;
    }
}

/// <summary>
/// 記録されるデータポイント
/// </summary>
[System.Serializable]
public struct DataPoint
{
    public float time;           // 経過時間
    public float currentValue;   // 現在値
    public float targetValue;    // 目標値
    public float pTerm;          // P項の値
    public float iTerm;          // I項の値
    public float dTerm;          // D項の値
    public float controlOutput;  // 制御出力

    /// <summary>
    /// 誤差を計算します
    /// </summary>
    public float GetError()
    {
        return targetValue - currentValue;
    }

    /// <summary>
    /// 誤差の絶対値を計算します
    /// </summary>
    public float GetAbsError()
    {
        return Mathf.Abs(GetError());
    }

    public override string ToString()
    {
        return $"Time: {time:F2}s, Value: {currentValue:F2}, Target: {targetValue:F2}, " +
               $"P: {pTerm:F2}, I: {iTerm:F2}, D: {dTerm:F2}";
    }
}
