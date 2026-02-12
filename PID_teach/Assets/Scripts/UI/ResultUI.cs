using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// シミュレーション結果画面のUIを管理するクラス
/// </summary>
public class ResultUI : MonoBehaviour
{
    [Header("結果表示テキスト")]
    [SerializeField] private TMP_Text settlingTimeText;
    [SerializeField] private TMP_Text overshootText;
    [SerializeField] private TMP_Text steadyStateErrorText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text modelNameText;

    [Header("グラフ表示コンポーネント")]
    [SerializeField] private SimpleGraph resultGraph;

    private SimulationManager simulationManager;
    private DataRecorder dataRecorder;


    private void Start()
    {
        // SimulationManagerの取得
        simulationManager = SimulationManager.Instance;
        if (simulationManager == null)
        {
            Debug.LogError("SimulationManagerが見つかりません。");
            return;
        }

        dataRecorder = simulationManager.DataRecorder;

        // 評価結果の取得
        float settlingTime = simulationManager.GetSettlingTime();
        float stabilityTolerance = 0.05f; // SimulationManagerの設定値に合わせるべきだが、ここでは仮の値

        // PerformanceEvaluatorは静的クラスとして再作成したので、直接呼び出す
        SimulationResult result = PerformanceEvaluator.Evaluate(dataRecorder, settlingTime, stabilityTolerance);

        // UIの更新
        UpdateResultUI(result);

        // グラフの描画
        DrawResultGraph();
    }

    /// <summary>
    /// 結果テキストを更新します
    /// </summary>
    private void UpdateResultUI(SimulationResult result)
    {
        modelNameText.text = $"モデル: {simulationManager.PhysicsModel.GetModelName()}";
        settlingTimeText.text = $"整定時間: {result.SettlingTime:F2} 秒";
        overshootText.text = $"オーバーシュート率: {result.OvershootPercentage:F2} %";
        steadyStateErrorText.text = $"定常偏差: {result.SteadyStateError:F3}";
        scoreText.text = $"総合スコア: {result.Score:F0} 点";
    }

    /// <summary>
    /// 結果グラフを描画します
    /// </summary>
    private void DrawResultGraph()
    {
        if (resultGraph == null || dataRecorder.GetDataCount() == 0) return;

        // グラフ描画用のデータリストを作成
        List<Vector2> currentValuePoints = new List<Vector2>();
        List<Vector2> targetValuePoints = new List<Vector2>();
        List<Vector2> pTermPoints = new List<Vector2>();
        List<Vector2> iTermPoints = new List<Vector2>();
        List<Vector2> dTermPoints = new List<Vector2>();

        // DataRecorderから全データを取得
        List<DataPoint> data = dataRecorder.GetData();

        // データをVector2のリストに変換
        foreach (var point in data)
        {
            currentValuePoints.Add(new Vector2(point.time, point.currentValue));
            targetValuePoints.Add(new Vector2(point.time, point.targetValue));
            pTermPoints.Add(new Vector2(point.time, point.pTerm));
            iTermPoints.Add(new Vector2(point.time, point.iTerm));
            dTermPoints.Add(new Vector2(point.time, point.dTerm));
        }

        // ----------------------------------------------------------------
        // 1. 応答グラフ (現在値 vs 目標値)
        // ----------------------------------------------------------------

        // Y軸の最大値を動的に決定
        float targetValue = data.Last().targetValue;
        float yMaxResponse = dataRecorder.CurrentValueData.Max() * 1.1f;
        if (targetValue * 1.2f > yMaxResponse) yMaxResponse = targetValue * 1.2f;

        resultGraph.DrawGraph(
            new List<List<Vector2>> { currentValuePoints, targetValuePoints },
            new List<Color> { Color.blue, Color.red }, // 現在値:青、目標値:赤
            "Time (s)", // X軸ラベル
            "Value (deg/m)", // Y軸ラベル
            0f, // X軸最小値
            data.Last().time, // X軸最大値
            yMaxResponse // Y軸最大値
        );

        // ----------------------------------------------------------------
        // 2. PID項グラフ (P項, I項, D項) - グラフコンポーネントが1つしかないため、ここでは応答グラフのみ描画
        // ----------------------------------------------------------------
        // 複数のグラフが必要な場合は、ResultUIに複数のSimpleGraphコンポーネントが必要になります。
        // ユーザーの要望に応じて、PID項グラフの描画ロジックはコメントアウトまたは別のSimpleGraphに割り当てます。
        // 現状は応答グラフのみ描画します。
    }

    /// <summary>
    /// メインメニューに戻ります
    /// </summary>
    public void OnBackToMenuClicked()
    {
        SceneManager.LoadScene("MainMenuScene");
    }

    /// <summary>
    /// ランキング画面に遷移します
    /// </summary>
    public void OnViewRankingClicked()
    {
        SceneManager.LoadScene("RankingScene");
    }
}
