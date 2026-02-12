using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// シミュレーション全体を管理するクラス
/// </summary>
public class SimulationManager : MonoBehaviour
{
    // シングルトンインスタンス
    public static SimulationManager Instance { get; private set; }

    [Header("参照")]
    [SerializeField] private GameObject physicsModelObject;

    [Header("PIDパラメータ")]
    [SerializeField] private PIDController pidController;

    [Header("シミュレーション設定")]
    [SerializeField] private float maxSimulationTime = 30f;
    [SerializeField] private float stabilityCheckDuration = 1.0f;
    [SerializeField] private float stabilityTolerance = 0.05f;  // 目標値の±5%

    [Header("イベント")]
    public UnityEvent OnSimulationStarted;
    public UnityEvent OnSimulationStopped;
    public UnityEvent<float> OnSimulationCompleted;  // 整定時間を渡す
    public UnityEvent<float> OnTimeUpdated;  // 経過時間を渡す

    // 内部状態
    private IPhysicsModel physicsModel;
    private DataRecorder dataRecorder;
    private SimulationState currentState = SimulationState.Idle;
    private float elapsedTime = 0f;
    private float stabilityTimer = 0f;
    private bool isStabilityAchieved = false;
    private float settlingTime = 0f;

    // プロパティ
    public SimulationState CurrentState => currentState;
    public float ElapsedTime => elapsedTime;
    public PIDController PIDController => pidController;
    public DataRecorder DataRecorder => dataRecorder;
    public IPhysicsModel PhysicsModel => physicsModel;

    private void Awake()
    {
        if (pidController == null)
        {
            pidController = new PIDController();
        }
        // シングルトン処理
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // シーン遷移しても破棄されないようにする

        // DataRecorderの初期化
        dataRecorder = new DataRecorder(0.05f, 2000);

        // SceneControllerからモデルとパラメータを取得して初期化
        if (SceneController.Instance != null)
        {
            physicsModel = SceneController.Instance.SelectedModel;
            PIDParameters initialParams = SceneController.Instance.PendingPIDParams;

            if (physicsModel != null)
            {
                // PIDコントローラとモデルの初期化
                Initialize(physicsModel, initialParams.Kp, initialParams.Ki, initialParams.Kd, initialParams.TargetValue);

                // シミュレーション開始
                StartSimulation();
            }
            else
            {
                Debug.LogError("SceneControllerから物理モデルを取得できませんでした。");
            }
        }
        else
        {
            Debug.LogError("SceneControllerが見つかりません。");
        }
    }

    /// <summary>
    /// シミュレーションを初期化します
    /// </summary>
    public void Initialize(IPhysicsModel model, float kp, float ki, float kd, float targetValue)
    {
        // 物理モデルをこのGameObjectの子にする（Unityのヒエラルキー管理のため）
        if (model is MonoBehaviour monoModel)
        {
            monoModel.transform.SetParent(transform);
        }

        physicsModel = model;

        // PIDゲインの設定
        pidController.SetGains(kp, ki, kd);
        pidController.Reset();

        // 目標値の設定
        physicsModel.SetTargetValue(targetValue);
        physicsModel.Reset();

        // データレコーダーのクリア
        dataRecorder.Clear();

        // 状態のリセット
        elapsedTime = 0f;
        stabilityTimer = 0f;
        isStabilityAchieved = false;
        settlingTime = 0f;
        currentState = SimulationState.Idle;

        Debug.Log($"シミュレーション初期化: Kp={kp}, Ki={ki}, Kd={kd}, Target={targetValue}");
    }

    /// <summary>
    /// シミュレーションを開始します
    /// </summary>
    public void StartSimulation()
    {
        if (physicsModel == null)
        {
            Debug.LogError("PhysicsModelが設定されていません！");
            return;
        }

        currentState = SimulationState.Running;
        elapsedTime = 0f;
        stabilityTimer = 0f;
        isStabilityAchieved = false;

        OnSimulationStarted?.Invoke();
        Debug.Log("シミュレーション開始");
    }

    /// <summary>
    /// シミュレーションを停止します
    /// </summary>
    public void StopSimulation()
    {
        if (currentState == SimulationState.Running || currentState == SimulationState.Paused)
        {
            currentState = SimulationState.Stopped;
            OnSimulationStopped?.Invoke();
            Debug.Log("シミュレーション停止");
        }
    }

    /// <summary>
    /// シミュレーションを一時停止します
    /// </summary>
    public void PauseSimulation()
    {
        if (currentState == SimulationState.Running)
        {
            currentState = SimulationState.Paused;
            Debug.Log("シミュレーション一時停止");
        }
    }

    /// <summary>
    /// シミュレーションを再開します
    /// </summary>
    public void ResumeSimulation()
    {
        if (currentState == SimulationState.Paused)
        {
            currentState = SimulationState.Running;
            Debug.Log("シミュレーション再開");
        }
    }

    private void FixedUpdate()
    {
        if (currentState != SimulationState.Running)
        {
            return;
        }

        if (physicsModel == null)
        {
            Debug.LogError("PhysicsModelが設定されていません！");
            StopSimulation();
            return;
        }

        // 時間の更新
        elapsedTime += Time.fixedDeltaTime;
        OnTimeUpdated?.Invoke(elapsedTime);

        // 現在値と目標値の取得
        float currentValue = physicsModel.GetCurrentValue();
        float targetValue = physicsModel.GetTargetValue();

        // PID制御の計算
        float controlInput = pidController.Update(currentValue, targetValue, Time.fixedDeltaTime);

        // 制御入力の適用
        physicsModel.SetControlInput(controlInput);

        // データの記録
        PIDDebugInfo debugInfo = pidController.GetDebugInfo();
        dataRecorder.Record(elapsedTime, currentValue, targetValue, debugInfo);

        // 安定性のチェック
        CheckStability(currentValue, targetValue);

        // 最大時間チェック
        if (elapsedTime >= maxSimulationTime)
        {
            CompleteSimulation();
        }
    }

    /// <summary>
    /// 安定性をチェックします
    /// </summary>
    private void CheckStability(float currentValue, float targetValue)
    {
        // 既に安定を達成している場合はスキップ
        if (isStabilityAchieved)
        {
            return;
        }

        // 許容誤差の計算
        float tolerance = Mathf.Abs(targetValue) * stabilityTolerance;
        if (tolerance < 0.1f) tolerance = 0.1f;  // 最小許容誤差

        float error = Mathf.Abs(currentValue - targetValue);
        float velocity = Mathf.Abs(physicsModel.GetCurrentVelocity());

        // 安定条件：誤差が許容範囲内 かつ 速度が小さい
        bool isCurrentlyStable = error <= tolerance && velocity <= tolerance;

        if (isCurrentlyStable)
        {
            stabilityTimer += Time.fixedDeltaTime;

            // 一定時間安定を維持したら安定達成
            if (stabilityTimer >= stabilityCheckDuration)
            {
                isStabilityAchieved = true;
                settlingTime = elapsedTime - stabilityCheckDuration;
                Debug.Log($"安定達成！整定時間: {settlingTime:F2}秒");

                // シミュレーション完了
                CompleteSimulation();
            }
        }
        else
        {
            // 安定条件を満たさなくなったらタイマーリセット
            stabilityTimer = 0f;
        }
    }

    /// <summary>
    /// シミュレーションを完了します
    /// </summary>
    private void CompleteSimulation()
    {
        currentState = SimulationState.Completed;

        // 安定を達成していない場合、整定時間は最大時間
        if (!isStabilityAchieved)
        {
            settlingTime = maxSimulationTime;
            Debug.Log("最大時間到達。安定は達成されませんでした。");
        }

        OnSimulationCompleted?.Invoke(settlingTime);
        Debug.Log($"シミュレーション完了: 整定時間 = {settlingTime:F2}秒");
    }

    /// <summary>
    /// 整定時間を取得します
    /// </summary>
    public float GetSettlingTime()
    {
        return settlingTime;
    }

    /// <summary>
    /// 安定を達成したかを取得します
    /// </summary>
    public bool IsStabilityAchieved()
    {
        return isStabilityAchieved;
    }

    /// <summary>
    /// PIDゲインを設定します
    /// </summary>
    public void SetPIDGains(float kp, float ki, float kd)
    {
        pidController.SetGains(kp, ki, kd);
    }

    /// <summary>
    /// 目標値を設定します
    /// </summary>
    public void SetTargetValue(float target)
    {
        if (physicsModel != null)
        {
            physicsModel.SetTargetValue(target);
        }
    }

    /// <summary>
    /// 物理モデルを設定します
    /// </summary>
    public void SetPhysicsModel(IPhysicsModel model)
    {
        physicsModel = model;
    }

    /// <summary>
    /// 最大シミュレーション時間を設定します
    /// </summary>
    public void SetMaxSimulationTime(float time)
    {
        maxSimulationTime = Mathf.Max(1f, time);
    }

    /// <summary>
    /// 安定判定の許容誤差を設定します
    /// </summary>
    public void SetStabilityTolerance(float tolerance)
    {
        stabilityTolerance = Mathf.Clamp(tolerance, 0.01f, 0.5f);
    }
}

/// <summary>
/// シミュレーションの状態
/// </summary>
public enum SimulationState
{
    Idle,       // 待機中
    Running,    // 実行中
    Paused,     // 一時停止
    Stopped,    // 停止
    Completed   // 完了
}
