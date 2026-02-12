using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// ランキング画面のUIを管理するクラス
/// </summary>
public class RankingUI : MonoBehaviour
{
    [Header("UIコンポーネント")]
    [SerializeField] private Button backButton;
    [SerializeField] private TMP_Text modelNameText;
    [SerializeField] private Transform contentParent; // ランキングエントリの親
    [SerializeField] private GameObject rankingEntryPrefab; // ランキングの行のプレハブ
    [SerializeField] private Button[] modelTabs; // モデル選択タブ

    // 内部状態
    private int currentModelId = 0; // 0: Pendulum, 1: Helicopter, 2: Crane

    private void Start()
    {
        backButton.onClick.AddListener(OnBackClicked);
        
        // モデルタブのリスナー設定
        for (int i = 0; i < modelTabs.Length; i++)
        {
            int id = i;
            modelTabs[i].onClick.AddListener(() => DisplayRanking(id));
        }
        
        // 初期表示
        DisplayRanking(currentModelId);
    }

    private void OnBackClicked()
    {
        SceneController.Instance.LoadMainMenu();
    }

    /// <summary>
    /// 指定されたモデルIDのランキングを表示します
    /// </summary>
    public void DisplayRanking(int modelId)
    {
        currentModelId = modelId;
        
        // 既存のエントリをクリア
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // モデル名の更新
        string modelName = GetModelNameById(modelId);
        modelNameText.text = $"{modelName} ランキング";

        // ランキングデータの取得
        List<RankingEntry> ranking = RankingManager.Instance.GetRanking(modelId, 10); // 上位10件

        if (ranking.Count == 0)
        {
            // データがない場合のメッセージ表示
            GameObject noData = new GameObject("NoDataText");
            noData.transform.SetParent(contentParent);
            TMP_Text text = noData.AddComponent<TextMeshProUGUI>();
            text.text = "まだランキングデータがありません。シミュレーションを実行して登録しましょう！";
            text.alignment = TextAlignmentOptions.Center;
            text.fontSize = 36;
            return;
        }

        // ランキングエントリの生成と表示
        for (int i = 0; i < ranking.Count; i++)
        {
            RankingEntry entry = ranking[i];
            
            // プレハブをインスタンス化
            GameObject entryObject = Instantiate(rankingEntryPrefab, contentParent);
            
            // エントリのコンポーネントを取得し、データを設定
            // プレハブの構造に依存するため、ここでは仮の処理
            TMP_Text[] texts = entryObject.GetComponentsInChildren<TMP_Text>();
            
            if (texts.Length >= 5)
            {
                texts[0].text = (i + 1).ToString(); // 順位
                texts[1].text = entry.playerName; // プレイヤー名
                texts[2].text = $"{entry.settlingTime:F2}秒"; // 整定時間
                texts[3].text = $"Kp:{entry.kp:F1} Ki:{entry.ki:F2} Kd:{entry.kd:F1}"; // PIDゲイン
                texts[4].text = $"{entry.totalScore:F0}点"; // スコア
            }
            else
            {
                Debug.LogWarning("ランキングエントリのプレハブに必要なTMP_Textコンポーネントが不足しています。");
            }
        }
    }
    
    private string GetModelNameById(int id)
    {
        switch (id)
        {
            case 0: return "棒の振り上げ";
            case 1: return "ヘリコプターの制御";
            case 2: return "クレーンの制御";
            default: return "不明なモデル";
        }
    }
}
