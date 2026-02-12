using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// メインメニュー画面のUIを管理するクラス
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    [Header("モデル選択ボタン")]
    [SerializeField] private Button pendulumButton;
    [SerializeField] private Button helicopterButton;
    [SerializeField] private Button craneButton;

    [Header("その他ボタン")]
    [SerializeField] private Button rankingButton;
    [SerializeField] private Button quitButton;
    [Header("モデルPrefab")]
    [SerializeField] private GameObject pendulumPrefab; // 追加
    [SerializeField] private GameObject helicopterPrefab; // 追加
    [SerializeField] private GameObject cranePrefab; // 追加

    private void Start()
    {
        // ボタンのリスナー設定
        pendulumButton.onClick.AddListener(() => OnModelSelected(0));
        helicopterButton.onClick.AddListener(() => OnModelSelected(1));
        craneButton.onClick.AddListener(() => OnModelSelected(2));

        rankingButton.onClick.AddListener(OnRankingClicked);
        quitButton.onClick.AddListener(OnQuitClicked);

        // 全てのモデルを有効化
        helicopterButton.interactable = true;
        craneButton.interactable = true;
    }


    private void OnModelSelected(int modelId)
    {
        GameObject modelObject = null;
        IPhysicsModel model = null;
        switch (modelId)
        {

            case 0: // 棒の振り上げ
                modelObject = Instantiate(pendulumPrefab); // Prefabをインスタンス化
                model = modelObject.GetComponent<PendulumModel>();
                break;
            case 1: // ヘリコプター
                modelObject = Instantiate(helicopterPrefab); // Prefabをインスタンス化
                model = modelObject.GetComponent<HelicopterModel>();
                break;
            case 2: // クレーン
                modelObject = Instantiate(cranePrefab); // Prefabをインスタンス化
                model = modelObject.GetComponent<CraneModel>();
                break;
            default:
                Debug.LogError($"未定義のモデルID: {modelId}");
                return;
        }
        if (modelObject != null)
        {
            DontDestroyOnLoad(modelObject); // シーン遷移後も保持
        }

        // SceneControllerにモデルを渡し、パラメータ設定シーンへ
        SceneController.Instance.LoadParameterSettingScene(model);

        // 仮のモデルインスタンスはDontDestroyOnLoadで引き継がれ、SimulationManagerで親が設定される
    }

    private void OnRankingClicked()
    {
        SceneController.Instance.LoadRankingScene();
    }

    private void OnQuitClicked()
    {
        // Unityエディタまたはビルド後のアプリケーションを終了
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}
