using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// シミュレーション実行中のUIを管理するクラス
/// </summary>
public class SimulationUI : MonoBehaviour
{
    [Header("UIコンポーネント")]
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private TMP_Text currentValueText;
    [SerializeField] private TMP_Text targetValueText;
    [SerializeField] private TMP_Text errorText;
    [SerializeField] private TMP_Text pidInfoText;
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button stopButton;

    [Header("PID項バーグラフ")]
    [SerializeField] private Image pBar;
    [SerializeField] private Image iBar;
    [SerializeField] private Image dBar;
    [SerializeField] private float maxBarValue = 500f; // バーグラフの最大値（調整が必要）

    [Header("リアルタイムグラフ")]
    [SerializeField] private SimpleGraph realtimeGraph;

    // リアルタイムグラフ描画用のヘルパーメソッド
    private float GetGraphYMax(List<float> data, float target)
    {
        float maxVal = target * 1.2f;
        if (data.Count > 0)
        {
            float dataMax = data.Max();
            if (dataMax > maxVal)
            {
                maxVal = dataMax * 1.1f;
            }
        }
        return maxVal;
    }

    // 内部状態
    private SimulationManager simulationManager;
    private IPhysicsModel currentModel;

    private void Start()
    {
        // SimulationManagerの取得 (シングルトンInstanceを使用)
        simulationManager = SimulationManager.Instance;
        if (simulationManager == null)
        {
            Debug.LogError("SimulationManagerが見つかりません。");
            return;
        }

        // 物理モデルの取得
        currentModel = simulationManager.PhysicsModel;
        if (currentModel == null)
        {
            Debug.LogError("物理モデルがSimulationManagerに設定されていません。");
            return;
        }

        // イベントリスナーの設定
        simulationManager.OnTimeUpdated.AddListener(UpdateTimer);
        simulationManager.OnSimulationCompleted.AddListener(OnSimulationCompleted);
        pauseButton.onClick.AddListener(TogglePause);
        stopButton.onClick.AddListener(OnStopClicked);
    }

    private void Update()
    {
        if (simulationManager.CurrentState == SimulationState.Running)
        {
            UpdateSimulationInfo();
        }
    }

    private void UpdateSimulationInfo()
    {
        float currentValue = currentModel.GetCurrentValue();
        float targetValue = currentModel.GetTargetValue();
        float error = targetValue - currentValue;
        string unit = currentModel.GetValueUnit();

        // テキストの更新
        currentValueText.text = $"現在値: {currentValue:F2}{unit}";
        targetValueText.text = $"目標値: {targetValue:F2}{unit}";
        errorText.text = $"誤差: {error:F2}{unit}";

        // PIDデバッグ情報の更新
        PIDDebugInfo debugInfo = simulationManager.PIDController.GetDebugInfo();
        pidInfoText.text = debugInfo.ToString();

        // バーグラフの更新
        UpdatePIDBars(debugInfo);

        // リアルタイムグラフの描画
        if (realtimeGraph != null && simulationManager != null && simulationManager.DataRecorder != null)
        {
            var dataRecorder = simulationManager.DataRecorder;

            // データがなければ描画しない
            if (dataRecorder.TimeData.Count == 0) return;

            // グラフ描画用のデータリストを作成
            List<Vector2> currentValuePoints = new List<Vector2>();
            List<Vector2> targetValuePoints = new List<Vector2>();

            // データをVector2のリストに変換
            for (int i = 0; i < dataRecorder.TimeData.Count; i++)
            {
                float t = dataRecorder.TimeData[i];
                float currentVal = dataRecorder.CurrentValueData[i];
                float targetVal = dataRecorder.TargetValue[i];

                currentValuePoints.Add(new Vector2(t, currentVal));
                targetValuePoints.Add(new Vector2(t, targetVal));
            }

            // Y軸の最大値を動的に決定
            float yMax = GetGraphYMax(dataRecorder.CurrentValueData, dataRecorder.TargetValue.Last());

            // SimpleGraphに描画を依頼
            realtimeGraph.DrawGraph(
                new List<List<Vector2>> { currentValuePoints, targetValuePoints },
                new List<Color> { Color.blue, Color.red }, // 現在値:青、目標値:赤
                "Time (s)", // X軸ラベル
                "Value (deg/m)", // Y軸ラベル
                0f, // X軸最小値
                dataRecorder.TimeData.Last(), // X軸最大値
                yMax // Y軸最大値
            );
        }
    }

    private void UpdateTimer(float time)
    {
        timeText.text = $"経過時間: {time:F2}秒";
    }

    private void UpdatePIDBars(PIDDebugInfo debugInfo)
    {
        // P項
        float pNorm = Mathf.Clamp01(Mathf.Abs(debugInfo.pTerm) / maxBarValue);
        pBar.fillAmount = pNorm;

        // I項
        float iNorm = Mathf.Clamp01(Mathf.Abs(debugInfo.iTerm) / maxBarValue);
        iBar.fillAmount = iNorm;

        // D項
        float dNorm = Mathf.Clamp01(Mathf.Abs(debugInfo.dTerm) / maxBarValue);
        dBar.fillAmount = dNorm;
    }

    private void TogglePause()
    {
        if (simulationManager.CurrentState == SimulationState.Running)
        {
            simulationManager.PauseSimulation();
            pauseButton.GetComponentInChildren<TMP_Text>().text = "再開";
        }
        else if (simulationManager.CurrentState == SimulationState.Paused)
        {
            simulationManager.ResumeSimulation();
            pauseButton.GetComponentInChildren<TMP_Text>().text = "一時停止";
        }
    }

    private void OnStopClicked()
    {
        simulationManager.StopSimulation();
        // 結果画面への遷移ロジック
        SceneManager.LoadScene("ResultScene");
    }

    private void OnSimulationCompleted(float settlingTime)
    {
        // シミュレーション完了時の処理
        Debug.Log($"シミュレーション完了。整定時間: {settlingTime:F2}秒");

        // 結果画面への遷移ロジック
        SceneManager.LoadScene("ResultScene");
    }
}
