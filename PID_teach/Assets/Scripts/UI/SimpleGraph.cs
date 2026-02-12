using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// グラフ描画システム
/// RawImageとTexture2Dを使用して複数ラインのグラフを描画します。
/// </summary>
public class SimpleGraph : MonoBehaviour
{
    [SerializeField] private RawImage graphImage;
    [SerializeField] private Color backgroundColor = Color.black;
    [SerializeField] private Color gridColor = new Color(0.2f, 0.2f, 0.2f, 1f);
    [SerializeField] private int gridLineCount = 5;

    private Texture2D graphTexture;
    private Color[] pixelBuffer;
    private int textureWidth;
    private int textureHeight;

    void Awake()
    {
        if (graphImage == null)
        {
            graphImage = GetComponent<RawImage>();
            if (graphImage == null)
            {
                Debug.LogError("SimpleGraph: RawImageコンポーネントが見つかりません。");
                enabled = false;
                return;
            }
        }
        InitializeTexture();
    }

    void OnRectTransformDimensionsChange()
    {
        // RectTransformのサイズが変更されたらテクスチャを再初期化
        if (graphImage != null && (graphImage.rectTransform.rect.width != textureWidth || graphImage.rectTransform.rect.height != textureHeight))
        {
            InitializeTexture();
        }
    }

    private void InitializeTexture()
    {
        textureWidth = (int)graphImage.rectTransform.rect.width;
        textureHeight = (int)graphImage.rectTransform.rect.height;

        if (textureWidth <= 0 || textureHeight <= 0)
        {
            // サイズが0の場合は初期化をスキップ
            return;
        }

        graphTexture = new Texture2D(textureWidth, textureHeight);
        pixelBuffer = new Color[textureWidth * textureHeight];
        graphImage.texture = graphTexture;
    }

    /// <summary>
    /// グラフを描画します。
    /// </summary>
    /// <param name="dataSets">描画するデータセットのリスト (各データセットはVector2のリスト: x=時間, y=値)</param>
    /// <param name="colors">各データセットに対応する色</param>
    /// <param name="xAxisLabel">X軸のラベル</param>
    /// <param name="yAxisLabel">Y軸のラベル</param>
    /// <param name="xAxisMin">X軸の最小値</param>
    /// <param name="xAxisMax">X軸の最大値</param>
    /// <param name="yAxisMax">Y軸の最大値</param>
    public void DrawGraph(
        List<List<Vector2>> dataSets,
        List<Color> colors,
        string xAxisLabel,
        string yAxisLabel,
        float xAxisMin,
        float xAxisMax,
        float yAxisMax)
    {
        if (graphTexture == null || pixelBuffer == null || textureWidth <= 0 || textureHeight <= 0)
        {
            InitializeTexture();
            if (graphTexture == null) return; // 初期化失敗
        }

        // 背景をクリア
        for (int i = 0; i < pixelBuffer.Length; i++)
        {
            pixelBuffer[i] = backgroundColor;
        }

        // グリッド線の描画
        DrawGrid(xAxisMin, xAxisMax, yAxisMax);

        // データセットの描画
        for (int i = 0; i < dataSets.Count; i++)
        {
            if (i < colors.Count)
            {
                DrawLine(dataSets[i], colors[i], xAxisMin, xAxisMax, yAxisMax);
            }
        }

        graphTexture.SetPixels(pixelBuffer);
        graphTexture.Apply();
    }

    private void DrawGrid(float xAxisMin, float xAxisMax, float yAxisMax)
    {
        // X軸グリッド線
        for (int i = 0; i <= gridLineCount; i++)
        {
            float yRatio = (float)i / gridLineCount;
            DrawHorizontalLine(yRatio, gridColor);
        }

        // Y軸グリッド線 (時間軸)
        for (int i = 0; i <= gridLineCount; i++)
        {
            float xRatio = (float)i / gridLineCount;
            DrawVerticalLine(xRatio, gridColor);
        }
    }

    private void DrawHorizontalLine(float yRatio, Color color)
    {
        int y = (int)(yRatio * textureHeight);
        for (int x = 0; x < textureWidth; x++)
        {
            SetPixel(x, y, color);
        }
    }

    private void DrawVerticalLine(float xRatio, Color color)
    {
        int x = (int)(xRatio * textureWidth);
        for (int y = 0; y < textureHeight; y++)
        {
            SetPixel(x, y, color);
        }
    }

    private void DrawLine(List<Vector2> dataPoints, Color color, float xAxisMin, float xAxisMax, float yAxisMax)
    {
        if (dataPoints == null || dataPoints.Count < 2) return;

        float xRange = xAxisMax - xAxisMin;
        if (xRange <= 0) xRange = 1f; // ゼロ除算防止
        if (yAxisMax <= 0) yAxisMax = 1f; // ゼロ除算防止

        for (int i = 0; i < dataPoints.Count - 1; i++)
        {
            Vector2 p1 = dataPoints[i];
            Vector2 p2 = dataPoints[i + 1];

            // 座標をテクスチャのピクセル範囲に正規化
            int x1 = (int)(((p1.x - xAxisMin) / xRange) * textureWidth);
            int y1 = (int)((p1.y / yAxisMax) * textureHeight);
            int x2 = (int)(((p2.x - xAxisMin) / xRange) * textureWidth);
            int y2 = (int)((p2.y / yAxisMax) * textureHeight);

            DrawLineSegment(x1, y1, x2, y2, color);
        }
    }

    private void DrawLineSegment(int x1, int y1, int x2, int y2, Color color)
    {
        bool steep = Mathf.Abs(y2 - y1) > Mathf.Abs(x2 - x1);
        if (steep)
        {
            Swap(ref x1, ref y1);
            Swap(ref x2, ref y2);
        }
        if (x1 > x2)
        {
            Swap(ref x1, ref x2);
            Swap(ref y1, ref y2);
        }

        int dx = x2 - x1;
        int dy = Mathf.Abs(y2 - y1);
        int error = dx / 2;
        int ystep = (y1 < y2) ? 1 : -1;
        int y = y1;

        for (int x = x1; x <= x2; x++)
        {
            if (steep)
            {
                SetPixel(y, x, color);
            }
            else
            {
                SetPixel(x, y, color);
            }

            error -= dy;
            if (error < 0)
            {
                y += ystep;
                error += dx;
            }
        }
    }

    private void SetPixel(int x, int y, Color color)
    {
        if (x >= 0 && x < textureWidth && y >= 0 && y < textureHeight)
        {
            pixelBuffer[y * textureWidth + x] = color;
        }
    }

    private void Swap(ref int a, ref int b)
    {
        int temp = a;
        a = b;
        b = temp;
    }
}
