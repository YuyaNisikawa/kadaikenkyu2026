using UnityEngine;

/// <summary>
/// モデル2: ヘリコプターの制御シミュレーションの物理モデル
/// IPhysicsModelインターフェースを実装し、PID制御の対象となる物理挙動を提供します。
/// </summary>
public class HelicopterModel : MonoBehaviour, IPhysicsModel
{
    [Header("物理コンポーネント")]
    [Tooltip("ヘリコプターのRigidbody")]
    [SerializeField] private Rigidbody helicopterRigidbody;

    [Header("モデル設定")]
    [Tooltip("プロペラが出せる最大推力")]
    [SerializeField] private float maxThrust = 200f;

    [Tooltip("安定判定の許容高度誤差（m）")]
    [SerializeField] private float stabilityAltitudeTolerance = 0.1f;

    [Tooltip("安定判定の許容速度（m/s）")]
    [SerializeField] private float stabilityVelocityTolerance = 0.1f;

    // 内部状態
    private float targetAltitude = 10f; // 目標高度
    private Vector3 initialPosition;
    private Quaternion initialRotation;

    private void Awake()
    {
        if (helicopterRigidbody == null)
        {
            helicopterRigidbody = GetComponent<Rigidbody>();
        }
        if (helicopterRigidbody == null)
        {
            Debug.LogError("HelicopterModel: Rigidbodyが設定されていません。");
        }

        // 初期状態を保存
        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }

    /// <summary>
    /// 現在の高度（Y座標）を取得します。
    /// </summary>
    public float GetCurrentValue()
    {
        return transform.position.y;
    }

    /// <summary>
    /// PID制御器から計算された推力を物理エンジンに適用します。
    /// </summary>
    /// <param name="input">制御入力（推力補正値）</param>
    public void SetControlInput(float input)
    {
        // 重力に対抗する基本推力
        float baseThrust = helicopterRigidbody.mass * Physics.gravity.magnitude;

        // PID制御による補正推力を加える
        float totalThrust = baseThrust + input;

        // 推力を制限
        float clampedThrust = Mathf.Clamp(totalThrust, 0, maxThrust);

        // 上向きに推力を適用
        helicopterRigidbody.AddForce(Vector3.up * clampedThrust, ForceMode.Force);
    }

    /// <summary>
    /// 目標高度を取得します。
    /// </summary>
    public float GetTargetValue()
    {
        return targetAltitude;
    }

    /// <summary>
    /// 目標高度を設定します。
    /// </summary>
    /// <param name="target">新しい目標高度（m）</param>
    public void SetTargetValue(float target)
    {
        targetAltitude = Mathf.Max(0f, target); // 高度は0以上
    }

    /// <summary>
    /// システムが安定しているかを判定します。
    /// </summary>
    public bool IsStable()
    {
        float altitudeError = Mathf.Abs(GetCurrentValue() - targetAltitude);
        float verticalVelocity = Mathf.Abs(helicopterRigidbody.linearVelocity.y);

        // 高度誤差と垂直速度が許容範囲内であるか
        return altitudeError < stabilityAltitudeTolerance &&
               verticalVelocity < stabilityVelocityTolerance;
    }

    /// <summary>
    /// モデルを初期状態にリセットします。
    /// </summary>
    public void Reset()
    {
        // 物理状態をリセット
        helicopterRigidbody.linearVelocity = Vector3.zero;
        helicopterRigidbody.angularVelocity = Vector3.zero;

        // 位置と回転を初期状態に戻す
        transform.position = initialPosition;
        transform.rotation = initialRotation;

        helicopterRigidbody.WakeUp();
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
        return "ヘリコプターの制御";
    }

    /// <summary>
    /// モデルのIDを取得します。
    /// </summary>
    public int GetModelId()
    {
        return 1; // モデル2
    }

    /// <summary>
    /// 現在の垂直速度を取得します。
    /// </summary>
    public float GetCurrentVelocity()
    {
        return helicopterRigidbody.linearVelocity.y;
    }
}
