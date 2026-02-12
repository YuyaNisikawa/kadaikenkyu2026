using UnityEngine;

/// <summary>
/// PID制御のコアロジックを実装するクラス
/// </summary>
public class PIDController
{
    // ゲイン
    private float Kp;
    private float Ki;
    private float Kd;

    // 内部状態
    private float integral;
    private float lastError;
    private float lastDerivativeTerm; // 微分項のノイズ対策用
    private float lastTime;

    // 設定
    private float integralLimit = 100f; // 積分ワインドアップ防止用
    private float derivativeFilterFactor = 0.1f; // 微分項のノイズフィルタリング用 (ローパスフィルタ)

    /// <summary>
    /// コンストラクタ
    /// </summary>
    public PIDController(float kp = 0f, float ki = 0f, float kd = 0f)
    {
        SetGains(kp, ki, kd);
        Reset();
    }

    /// <summary>
    /// PIDゲインを設定します
    /// </summary>
    public void SetGains(float kp, float ki, float kd)
    {
        Kp = kp;
        Ki = ki;
        Kd = kd;
    }

    /// <summary>
    /// 内部状態をリセットします
    /// </summary>
    public void Reset()
    {
        integral = 0f;
        lastError = 0f;
        lastDerivativeTerm = 0f;
        lastTime = -1f;
    }

    /// <summary>
    /// PID制御の計算を行い、制御入力を返します
    /// </summary>
    /// <param name="currentValue">現在の値</param>
    /// <param name="targetValue">目標値</param>
    /// <param name="deltaTime">前回の更新からの経過時間</param>
    /// <returns>制御入力</returns>
    public float Update(float currentValue, float targetValue, float deltaTime)
    {
        if (deltaTime <= 0) return 0f;

        float error = targetValue - currentValue;

        // P項 (比例項)
        float pTerm = Kp * error;

        // I項 (積分項)
        integral += error * deltaTime;
        // 積分ワインドアップ防止
        integral = Mathf.Clamp(integral, -integralLimit, integralLimit);
        float iTerm = Ki * integral;

        // D項 (微分項)
        // 微分キック防止のため、現在値の変化量（-d(currentValue)/dt）を使用
        float derivative = (error - lastError) / deltaTime;
        // float derivative = -(currentValue - lastValue) / deltaTime; // 別の書き方

        // 微分項のノイズ対策（ローパスフィルタ）
        float rawDTerm = Kd * derivative;
        float dTerm = Mathf.Lerp(lastDerivativeTerm, rawDTerm, derivativeFilterFactor);
        lastDerivativeTerm = dTerm;

        // 制御入力
        float output = pTerm + iTerm + dTerm;

        // 状態の更新
        lastError = error;
        lastTime = Time.time;

        return output;
    }

    /// <summary>
    /// 現在のPIDデバッグ情報を取得します
    /// </summary>
    public PIDDebugInfo GetDebugInfo()
    {
        // D項の計算にはlastErrorが必要なため、Update後に呼び出すことを想定
        float error = lastError; // Updateで計算された最新の誤差
        float pTerm = Kp * error;
        float iTerm = Ki * integral;
        float dTerm = lastDerivativeTerm; // フィルタリング後のD項

        return new PIDDebugInfo
        {
            error = error,
            pTerm = pTerm,
            iTerm = iTerm,
            dTerm = dTerm,
            output = pTerm + iTerm + dTerm
        };
    }
}

/// <summary>
/// PID制御のデバッグ情報構造体
/// </summary>
public struct PIDDebugInfo
{
    public float error;
    public float pTerm;
    public float iTerm;
    public float dTerm;
    public float output;

    public override string ToString()
    {
        return $"Error: {error:F2}\n" +
               $"P: {pTerm:F2}, I: {iTerm:F2}, D: {dTerm:F2}\n" +
               $"Output: {output:F2}";
    }
}


