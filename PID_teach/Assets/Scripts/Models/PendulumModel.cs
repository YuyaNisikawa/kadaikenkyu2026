using UnityEngine;

/// <summary>
/// モデル1: 棒の振り上げシミュレーションの物理モデル
/// IPhysicsModelインターフェースを実装し、PID制御の対象となる物理挙動を提供します。
/// </summary>
public class PendulumModel : MonoBehaviour, IPhysicsModel
{
    [Header("物理コンポーネント")]
    [Tooltip("棒のRigidbody2D")]
    [SerializeField] private Rigidbody2D pendulumRigidbody;

    [Tooltip("モーターのHingeJoint2D")]
    [SerializeField] private HingeJoint2D motorJoint;

    [Header("モデル設定")]
    [Tooltip("モーターが出せる最大トルク")]
    [SerializeField] private float maxTorque = 100f;

    [Tooltip("安定判定の許容角度誤差（度）")]
    [SerializeField] private float stabilityAngleTolerance = 2.0f;

    [Tooltip("安定判定の許容角速度（度/秒）")]
    [SerializeField] private float stabilityAngularVelocityTolerance = 5.0f;

    // 内部状態
    private float targetAngle = 90f; // 目標角度
    private Vector3 initialPosition;
    private Quaternion initialRotation;


    // ▼▼▼ 修正ポイント 1: Unityエディタ用の Reset() を追加 ▼▼▼
    /// <summary>
    /// Unityエディタがコンポーネントアタッチ時（またはInspectorでReset時）に呼び出すメソッド
    /// </summary>
    private void Reset()
    {
        // Inspectorが空の場合、自動的にGetComponentを試みる
        if (pendulumRigidbody == null)
        {
            pendulumRigidbody = GetComponent<Rigidbody2D>();
        }
        if (motorJoint == null)
        {
            motorJoint = GetComponent<HingeJoint2D>();
        }

        // 注意: ここではゲーム状態のリセット（初期位置に戻すなど）は行わない
    }
    // ▲▲▲ 修正ここまで ▲▲▲


    private void Awake()
    {
        if (pendulumRigidbody == null)
        {
            pendulumRigidbody = GetComponent<Rigidbody2D>();
        }
        if (pendulumRigidbody == null)
        {
            Debug.LogError("PendulumModel: Rigidbody2Dが設定されていません。");
        }

        // 初期状態を保存
        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }

    /// <summary>
    /// 現在の棒の角度を取得します。
    /// </summary>
    public float GetCurrentValue()
    {
        // Unityの角度は0-360度で、Z軸回転が時計回り正。
        // 制御しやすいように、-180度から180度の範囲に正規化します。
        float angle = pendulumRigidbody.rotation;

        // 0度を真下、90度を水平右、180度を真上、-90度を水平左とします。
        // Unityのrotationは時計回り正なので、そのまま使用します。
        // 必要に応じて-180から180に変換
        if (angle > 180f)
        {
            angle -= 360f;
        }

        return angle;
    }

    /// <summary>
    /// PID制御器から計算されたトルクを物理エンジンに適用します。
    /// </summary>
    /// <param name="input">制御入力（トルク）</param>
    public void SetControlInput(float input)
    {
        // トルクを最大値で制限
        float clampedTorque = Mathf.Clamp(input, -maxTorque, maxTorque);

        // AddTorqueはZ軸周りの回転力を適用します
        pendulumRigidbody.AddTorque(clampedTorque);
    }

    /// <summary>
    /// 目標角度を取得します。
    /// </summary>
    public float GetTargetValue()
    {
        return targetAngle;
    }

    /// <summary>
    /// 目標角度を設定します。
    /// </summary>
    /// <param name="target">新しい目標角度（度）</param>
    public void SetTargetValue(float target)
    {
        // 目標角度を-180から180の範囲に制限
        targetAngle = Mathf.Clamp(target, -180f, 180f);
    }

    /// <summary>
    /// システムが安定しているかを判定します。
    /// </summary>
    public bool IsStable()
    {
        float angleError = Mathf.Abs(GetCurrentValue() - targetAngle);
        float angularVelocity = Mathf.Abs(pendulumRigidbody.angularVelocity);

        // 角度誤差と角速度が許容範囲内であるか
        return angleError < stabilityAngleTolerance &&
               angularVelocity < stabilityAngularVelocityTolerance;
    }


    // ▼▼▼ 修正ポイント 2: インターフェースの Reset() を明示的に実装 ▼▼▼
    /// <summary>
    /// モデルを初期状態にリセットします。（IPhysicsModelの実装）
    /// </summary>
    void IPhysicsModel.Reset() // ← "public" を消し、"IPhysicsModel." を追加
    {
        // このメソッドは「ゲーム実行中」にインターフェース経由でのみ呼ばれる。
        // Unityエディタ（アタッチ時）に呼ばれることはなくなる。

        // 実行時には Awake() が完了しているため、pendulumRigidbody や
        // initialPosition が正しく設定されている前提で動作する。

        // (念のため、実行時エラーを防ぐためにnullチェックを追加)
        if (pendulumRigidbody == null)
        {
            Debug.LogError("Resetが呼ばれましたが、Rigidbodyがnullです。");
            return;
        }

        // 物理状態をリセット
        pendulumRigidbody.linearVelocity = Vector2.zero;
        pendulumRigidbody.angularVelocity = 0f; // (エラー発生箇所だった)

        // 位置と回転を初期状態に戻す
        transform.position = initialPosition;
        transform.rotation = initialRotation;

        // 物理エンジンがリセットを認識するまで待つ
        pendulumRigidbody.WakeUp();
    }
    // ▲▲▲ 修正ここまで ▲▲▲

    /// <summary>
    /// 制御対象の値の単位を取得します。
    /// </summary>
    public string GetValueUnit()
    {
        return "度";
    }

    /// <summary>
    /// モデルの名前を取得します。
    /// </summary>
    public string GetModelName()
    {
        return "棒の振り上げ";
    }

    /// <summary>
    /// モデルのIDを取得します。
    /// </summary>
    public int GetModelId()
    {
        return 0; // モデル1
    }

    /// <summary>
    /// 現在の角速度を取得します。
    /// </summary>
    public float GetCurrentVelocity()
    {
        return pendulumRigidbody.angularVelocity;
    }
}