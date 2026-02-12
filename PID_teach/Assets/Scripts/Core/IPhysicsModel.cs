using UnityEngine;

/// <summary>
/// 物理モデルのインターフェース
/// すべての制御対象モデルが実装すべきメソッドを定義します
/// </summary>
public interface IPhysicsModel
{
    /// <summary>
    /// 現在の制御対象の値を取得します
    /// </summary>
    /// <returns>現在値（角度、高度、位置など）</returns>
    float GetCurrentValue();

    /// <summary>
    /// 制御入力を設定します
    /// </summary>
    /// <param name="input">制御入力（トルク、推力、速度など）</param>
    void SetControlInput(float input);

    /// <summary>
    /// 目標値を取得します
    /// </summary>
    /// <returns>目標値</returns>
    float GetTargetValue();

    /// <summary>
    /// 目標値を設定します
    /// </summary>
    /// <param name="target">新しい目標値</param>
    void SetTargetValue(float target);

    /// <summary>
    /// システムが安定しているかを判定します
    /// </summary>
    /// <returns>安定している場合true</returns>
    bool IsStable();

    /// <summary>
    /// モデルを初期状態にリセットします
    /// </summary>
    void Reset();

    /// <summary>
    /// 制御対象の値の単位を取得します
    /// </summary>
    /// <returns>単位の文字列（"度", "m", "m/s"など）</returns>
    string GetValueUnit();

    /// <summary>
    /// モデルの名前を取得します
    /// </summary>
    /// <returns>モデル名</returns>
    string GetModelName();

    /// <summary>
    /// モデルのIDを取得します
    /// </summary>
    /// <returns>モデルID（0: Pendulum, 1: Helicopter, 2: Crane）</returns>
    int GetModelId();

    /// <summary>
    /// 現在の速度または角速度を取得します（微分項で使用可能）
    /// </summary>
    /// <returns>速度</returns>
    float GetCurrentVelocity();
}
