using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// シミュレーション結果の性能を評価するクラス
/// </summary>
public static class PerformanceEvaluator
{
    /// <summary>
    /// 性能評価を実行します
    /// </summary>
    public static SimulationResult Evaluate(DataRecorder dataRecorder, float settlingTime, float stabilityTolerance)
    {
        SimulationResult result = new SimulationResult();

        // 1. 整定時間
        result.SettlingTime = settlingTime;

        // データがない場合は評価をスキップ
        if (dataRecorder.GetDataCount() == 0)
        {
            result.OvershootPercentage = 0f;
            result.SteadyStateError = 0f;
            result.Score = 0f;
            return result;
        }

        List<DataPoint> data = dataRecorder.GetData();
        float targetValue = data.Last().targetValue;

        // 2. オーバーシュート率
        result.OvershootPercentage = CalculateOvershoot(data, targetValue);

        // 3. 定常偏差 (最後のデータポイントの誤差)
        result.SteadyStateError = CalculateSteadyStateError(data, targetValue);

        // 4. 総合スコア (評価基準は任意に設定)
        result.Score = CalculateScore(result, dataRecorder.GetDataCount());

        return result;
    }

    /// <summary>
    /// オーバーシュート率を計算します
    /// </summary>
    private static float CalculateOvershoot(List<DataPoint> data, float targetValue)
    {
        if (targetValue == 0f) return 0f;

        // 目標値からの最大偏差を計算
        float maxDeviation = 0f;
        foreach (var point in data)
        {
            float deviation = point.currentValue - targetValue;
            if (Mathf.Abs(deviation) > Mathf.Abs(maxDeviation))
            {
                maxDeviation = deviation;
            }
        }

        // オーバーシュート率 (%) = (最大偏差 / 目標値) * 100
        // 負のオーバーシュート（アンダーシュート）は無視
        if (maxDeviation <= 0) return 0f;

        return (maxDeviation / Mathf.Abs(targetValue)) * 100f;
    }

    /// <summary>
    /// 定常偏差を計算します
    /// </summary>
    private static float CalculateSteadyStateError(List<DataPoint> data, float targetValue)
    {
        // 最後のデータポイントの誤差を定常偏差とする
        float lastValue = data.Last().currentValue;
        return Mathf.Abs(targetValue - lastValue);
    }

    /// <summary>
    /// 総合スコアを計算します (独自の評価基準)
    /// </summary>
    private static float CalculateScore(SimulationResult result, int dataCount)
    {
        // スコアリングの例:
        // 1. 整定時間が短いほど高得点
        // 2. オーバーシュート率が低いほど高得点
        // 3. 定常偏差が低いほど高得点

        float maxTime = 30f; // SimulationManagerのmaxSimulationTimeと合わせるべきだが、ここでは仮に30秒
        float timeScore = Mathf.Max(0f, maxTime - result.SettlingTime) / maxTime * 50f; // 最大50点

        float overshootPenalty = result.OvershootPercentage * 0.5f; // オーバーシュート1%につき0.5点減点
        float overshootScore = Mathf.Max(0f, 30f - overshootPenalty); // 最大30点

        float steadyStatePenalty = result.SteadyStateError * 100f; // 定常偏差0.01につき1点減点
        float steadyStateScore = Mathf.Max(0f, 20f - steadyStatePenalty); // 最大20点

        // データが少ない場合はペナルティ
        if (dataCount < 100)
        {
            timeScore *= 0.5f;
        }

        return timeScore + overshootScore + steadyStateScore;
    }
}

/// <summary>
/// シミュレーション結果を格納する構造体
/// </summary>
public struct SimulationResult
{
    public float SettlingTime;          // 整定時間 (秒)
    public float OvershootPercentage;   // オーバーシュート率 (%)
    public float SteadyStateError;      // 定常偏差 (絶対値)
    public float Score;                 // 総合スコア
}
