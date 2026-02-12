using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// シーン遷移とモデル選択を管理するコントローラ
/// </summary>
public class SceneController : MonoBehaviour
{
    public static SceneController Instance { get; private set; }

    // シーン名
    private const string MAIN_MENU_SCENE = "MainMenuScene";
    private const string SIMULATION_SCENE = "SimulationScene";
    private const string TUTORIAL_SCENE = "TutorialScene";
    private const string RANKING_SCENE = "RankingScene";

    // 選択されたモデル
    public IPhysicsModel SelectedModel { get; private set; }

    // シミュレーション開始時に引き継ぐPIDパラメータ
    public PIDParameters PendingPIDParams { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// メインメニューシーンに遷移します
    /// </summary>
    public void LoadMainMenu()
    {
        SceneManager.LoadScene(MAIN_MENU_SCENE);
    }

    /// <summary>
    /// シミュレーション開始時に引き継ぐPIDパラメータを設定します
    /// </summary>
    public void SetPendingPIDParams(PIDParameters parameters)
    {
        PendingPIDParams = parameters;
    }

    /// <summary>
    /// パラメータ設定シーンに遷移します
    /// </summary>
    public void LoadParameterSettingScene(IPhysicsModel model)
    {
        SelectedModel = model;
        SceneManager.LoadScene("ParameterSettingScene"); // 仮のシーン名
    }

    /// <summary>
    /// シミュレーションシーンに遷移します
    /// </summary>
    public void LoadSimulationScene()
    {
        SceneManager.LoadScene(SIMULATION_SCENE);
    }

    /// <summary>
    /// ランキングシーンに遷移します
    /// </summary>
    public void LoadRankingScene()
    {
        SceneManager.LoadScene(RANKING_SCENE);
    }

    /// <summary>
    /// パラメータ設定シーンに遷移します
    /// </summary>
    public void LoadTutorialScene(IPhysicsModel model)
    {
        SelectedModel = model;
        SceneManager.LoadScene(TUTORIAL_SCENE); // 仮のシーン名
    }

    /// <summary>
    /// 現在のシーンがシミュレーションシーンか判定します
    /// </summary>
    public bool IsSimulationScene()
    {
        return SceneManager.GetActiveScene().name == SIMULATION_SCENE;
    }
}
