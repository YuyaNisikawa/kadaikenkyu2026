using UnityEngine;

/// <summary>
/// PID制御のパラメータを保持する構造体
/// </summary>
[System.Serializable]
public struct PIDParameters
{
    public float Kp;
    public float Ki;
    public float Kd;
    public float TargetValue;

    public PIDParameters(float kp, float ki, float kd, float target)
    {
        Kp = kp;
        Ki = ki;
        Kd = kd;
        TargetValue = target;
    }

    public override string ToString()
    {
        return $"Kp: {Kp:F2}, Ki: {Ki:F2}, Kd: {Kd:F2}, Target: {TargetValue:F2}";
    }
}

/// <summary>
/// PIDパラメータのプリセット
/// </summary>
[System.Serializable]
public struct PIDPreset
{
    public string name;
    public float kp;
    public float ki;
    public float kd;
    [TextArea(3, 5)]
    public string description;
}
