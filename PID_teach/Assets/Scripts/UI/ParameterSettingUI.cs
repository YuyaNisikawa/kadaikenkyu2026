using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// PIDパラメータ設定画面のUIを管理するクラス
/// </summary>
public class ParameterSettingUI : MonoBehaviour
{
    [Header("UIコンポーネント")]
    [SerializeField] private Slider kpSlider;
    [SerializeField] private Slider kiSlider;
    [SerializeField] private Slider kdSlider;
    [SerializeField] private TMP_InputField targetInputField;
    [SerializeField] private TMP_Text kpValueText;
    [SerializeField] private TMP_Text kiValueText;
    [SerializeField] private TMP_Text kdValueText;
    [SerializeField] private TMP_Text targetUnitText;
    [SerializeField] private Button startButton;
    [SerializeField] private Button backButton;
    [SerializeField] private TMP_Text modelNameText;

    [Header("プリセット")]
    [SerializeField] private PIDPreset[] presets;
    [SerializeField] private TMP_Dropdown presetDropdown; //修正箇所

    // 内部状態
    private IPhysicsModel currentModel;
    private PIDParameters currentParams;

    private void Start()
    {
        // SceneControllerから選択されたモデルを取得
        if (SceneController.Instance != null && SceneController.Instance.SelectedModel != null)
        {
            currentModel = SceneController.Instance.SelectedModel;
            InitializeUI();
        }
        else
        {
            Debug.LogError("選択されたモデルがありません。メインメニューに戻ります。");

            if (SceneController.Instance != null)
            {
                SceneController.Instance.LoadMainMenu();
            }
            else
            {
                // SceneController自体がない場合は、強制的にシーンをロードするか、何もしない
                // 必要であれば: UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
                Debug.LogWarning("SceneControllerが見つかりません。単体テスト中である可能性があります。");
            }
        }
    }

    private void InitializeUI()
    {
        // モデル名表示
        modelNameText.text = currentModel.GetModelName();
        targetUnitText.text = currentModel.GetValueUnit();

        // スライダーのリスナー設定
        kpSlider.onValueChanged.AddListener(OnKpChanged);
        kiSlider.onValueChanged.AddListener(OnKiChanged);
        kdSlider.onValueChanged.AddListener(OnKdChanged);

        // ボタンのリスナー設定
        startButton.onClick.AddListener(OnStartSimulation);
        backButton.onClick.AddListener(OnBackToMainMenu);

        // プリセットドロップダウンの設定
        InitializePresets();

        // 初期値設定（デフォルト値）
        SetDefaultValues();
    }

    private void InitializePresets()
    {
        presetDropdown.ClearOptions();
        List<string> options = new List<string> { "カスタム設定" };

        // プリセットの追加
        presets = GetDefaultPresets();
        foreach (var preset in presets)
        {
            options.Add(preset.name);
        }

        presetDropdown.AddOptions(options);
        presetDropdown.onValueChanged.AddListener(OnPresetSelected);
    }

    private PIDPreset[] GetDefaultPresets()
    {
        // モデルごとに適切なデフォルトプリセットを設定
        if (currentModel.GetModelId() == 0) // 棒の振り上げ
        {
            return new PIDPreset[]
            {
                new PIDPreset { name = "安定重視", kp = 50, ki = 0.5f, kd = 10,
                                description = "ゆっくりだが確実に目標角度に到達します。"},
                new PIDPreset { name = "バランス", kp = 100, ki = 1f, kd = 20,
                                description = "応答速度と安定性のバランスが良い設定です。"},
                new PIDPreset { name = "速度重視", kp = 200, ki = 2f, kd = 5,
                                description = "素早く目標に到達しますが、オーバーシュートしやすい設定です。"}
            };
        }
        // 他のモデルのプリセットは後で追加
        return new PIDPreset[0];
    }

    private void SetDefaultValues()
    {
        // スライダーの範囲設定（モデルに応じて調整が必要）
        kpSlider.minValue = 0f; kpSlider.maxValue = 300f;
        kiSlider.minValue = 0f; kiSlider.maxValue = 5f;
        kdSlider.minValue = 0f; kdSlider.maxValue = 50f;

        // 初期値
        kpSlider.value = 100f;
        kiSlider.value = 1f;
        kdSlider.value = 20f;
        targetInputField.text = "90"; // 棒の振り上げのデフォルト目標角度

        // テキストの更新
        OnKpChanged(kpSlider.value);
        OnKiChanged(kiSlider.value);
        OnKdChanged(kdSlider.value);

        // パラメータの更新
        UpdateCurrentParams();
    }

    private void OnKpChanged(float value)
    {
        kpValueText.text = value.ToString("F2");
        UpdateCurrentParams();
    }

    private void OnKiChanged(float value)
    {
        kiValueText.text = value.ToString("F2");
        UpdateCurrentParams();
    }

    private void OnKdChanged(float value)
    {
        kdValueText.text = value.ToString("F2");
        UpdateCurrentParams();
    }

    private void OnPresetSelected(int index)
    {
        if (index == 0) // カスタム設定
        {
            // 何もしない（現在のスライダー値を維持）
            return;
        }

        // プリセットを適用
        PIDPreset selectedPreset = presets[index - 1];
        kpSlider.value = selectedPreset.kp;
        kiSlider.value = selectedPreset.ki;
        kdSlider.value = selectedPreset.kd;

        // スライダーのリスナーが呼ばれてUIとパラメータが更新される
    }

    private void UpdateCurrentParams()
    {
        float targetValue;
        if (!float.TryParse(targetInputField.text, out targetValue))
        {
            targetValue = 0f; // パース失敗時は0
        }

        currentParams = new PIDParameters(
            kpSlider.value,
            kiSlider.value,
            kdSlider.value,
            targetValue
        );
    }

    private void OnStartSimulation()
    {
        UpdateCurrentParams();

        // パラメータをSimulationManagerに渡してシミュレーション開始
        // ここではまだSimulationManagerへの接続は実装しない（次のUIフェーズで）
        Debug.Log($"シミュレーション開始リクエスト: {currentParams}");

        // SceneControllerにパラメータを保存
        SceneController.Instance.SetPendingPIDParams(currentParams);

        // シミュレーションシーンに遷移
        SceneController.Instance.LoadSimulationScene();
    }

    private void OnBackToMainMenu()
    {
        SceneController.Instance.LoadMainMenu();
    }

    /// <summary>
    /// 現在設定されているPIDパラメータを取得します
    /// </summary>
    public PIDParameters GetCurrentParameters()
    {
        UpdateCurrentParams();
        return currentParams;
    }
}
