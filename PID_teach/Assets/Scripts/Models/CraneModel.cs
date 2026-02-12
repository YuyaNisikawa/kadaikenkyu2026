using UnityEngine;

/// <summary>
/// モデル3: クレーン制御シミュレーションの物理モデル
/// IPhysicsModelインターフェースを実装し、PID制御の対象となる物理挙動を提供します。
/// </summary>
public class CraneModel : MonoBehaviour, IPhysicsModel
{
    [Header("物理コンポーネント")]
    [Tooltip("クレーン本体のRigidbody")]
    [SerializeField] private Rigidbody craneRigidbody;

    [Tooltip("鉄球のRigidbody")]
    [SerializeField] private Rigidbody ballRigidbody;

    [Tooltip("クレーンと鉄球を繋ぐジョイント")]
    [SerializeField] private SpringJoint springJoint;

    [Header("モデル設定")]
    [Tooltip("クレーンが出せる最大力")]
    [SerializeField] private float maxForce = 500f;

    [Tooltip("安定判定の許容位置誤差（m）")]
    [SerializeField] private float stabilityPositionTolerance = 0.1f;

    [Tooltip("安定判定の許容速度（m/s）")]
    [SerializeField] private float stabilityVelocityTolerance = 0.1f;

    // 内部状態
    private float targetPosition = 5f; // 目標位置（X座標）
    private Vector3 initialPosition;
    private Quaternion initialRotation;

    private void Awake()
    {
        if (craneRigidbody == null)
        {
            craneRigidbody = GetComponent<Rigidbody>();
        }
        if (craneRigidbody == null)
        {
            Debug.LogError("CraneModel: Rigidbodyが設定されていません。");
        }

        // 初期状態を保存
        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }

    /// <summary>
    /// 現在のクレーンの水平位置（X座標）を取得します。
    /// </summary>
    public float GetCurrentValue()
    {
        return transform.position.x;
    }

    /// <summary>
    /// PID制御器から計算された力を物理エンジンに適用します。
    /// </summary>
    /// <param name="input">制御入力（力）</param>
    public void SetControlInput(float input)
    {
        // 力を制限
        float clampedForce = Mathf.Clamp(input, -maxForce, maxForce);

        // 水平方向（X軸）に力を適用
        craneRigidbody.AddForce(Vector3.right * clampedForce, ForceMode.Force);
    }

    /// <summary>
    /// 目標位置を取得します。
    /// </summary>
    public float GetTargetValue()
    {
        return targetPosition;
    }

    /// <summary>
    /// 目標位置を設定します。
    /// </summary>
    /// <param name="target">新しい目標位置（X座標）</param>
    public void SetTargetValue(float target)
    {
        targetPosition = target;
    }

    /// <summary>
    /// システムが安定しているかを判定します。
    /// </summary>
    public bool IsStable()
    {
        float positionError = Mathf.Abs(GetCurrentValue() - targetPosition);
        float velocity = Mathf.Abs(craneRigidbody.linearVelocity.x);

        // 鉄球の揺れも考慮する必要があるが、ここでは簡易的にクレーン本体で判定
        // 鉄球の揺れを判定するには、鉄球の水平速度や角度をチェックする必要がある

        // 位置誤差と速度が許容範囲内であるか
        return positionError < stabilityPositionTolerance &&
               velocity < stabilityVelocityTolerance;
    }

    /// <summary>
    /// モデルを初期状態にリセットします。
    /// </summary>
    public void Reset()
    {
        // 物理状態をリセット
        craneRigidbody.linearVelocity = Vector3.zero;
        craneRigidbody.angularVelocity = Vector3.zero;
        ballRigidbody.linearVelocity = Vector3.zero;
        ballRigidbody.angularVelocity = Vector3.zero;

        // 位置と回転を初期状態に戻す
        transform.position = initialPosition;
        transform.rotation = initialRotation;

        craneRigidbody.WakeUp();
        ballRigidbody.WakeUp();
    }

    /// <summary>
    /// 制御対象の値の単位を取得します。
    /// </summary>
    public string GetValueUnit()
    {
        return "m";
    }

    /// <summary>
    /// モデルの名前を取得します。
    /// </summary>
    public string GetModelName()
    {
        return "クレーンの制御";
    }

    /// <summary>
    /// モデルのIDを取得します。
    /// </summary>
    public int GetModelId()
    {
        return 2; // モデル3
    }

    /// <summary>
    /// 現在の水平速度を取得します。
    /// </summary>
    public float GetCurrentVelocity()
    {
        return craneRigidbody.linearVelocity.x;
    }
}
