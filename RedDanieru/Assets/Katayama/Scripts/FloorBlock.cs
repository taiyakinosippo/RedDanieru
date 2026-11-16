
using UnityEngine;

public class FloorBlock : MonoBehaviour
{
    // グリッド座標
    public Vector3Int GridPosition;

    [Header("選択色")]
    [SerializeField] private Color selectColor = Color.green;

    [Header("配置プレビュー色")]
    [SerializeField] private Color canPlaceColor = Color.green;
    [SerializeField] private Color cannotPlaceColor = Color.red;

    private Renderer[] renderers;
    private Color[] defaultColors;

    private bool isSelected = false;
    private bool isPreviewing = false;
    private Color previewColor;

    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        defaultColors = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            defaultColors[i] = renderers[i].material.color;
        }
    }

    /// <summary>
    /// 選択
    /// </summary>
    public void Select()
    {
        isSelected = true;
        UpdateColor();
    }

    /// <summary>
    /// 選択解除
    /// </summary>
    public void Deselect()
    {
        isSelected = false;
        UpdateColor();
    }

    /// <summary>
    /// 配置プレビュー色を設定
    /// </summary>
    public void SetPreviewColor(bool canPlace)
    {
        isPreviewing = true;
        previewColor = canPlace ? canPlaceColor : cannotPlaceColor;

        UpdateColor();
    }

    /// <summary>
    /// 配置プレビューを解除
    /// </summary>
    public void ClearPreviewColor()
    {
        isPreviewing = false;
        UpdateColor();
    }

    /// <summary>
    /// 現在の状態に合わせて色を更新
    /// </summary>
    private void UpdateColor()
    {
        if (renderers == null)
            return;

        for (int i = 0; i < renderers.Length; i++)
        {
            if (isPreviewing)
            {
                renderers[i].material.color = previewColor;
            }
            else if (isSelected)
            {
                renderers[i].material.color = selectColor;
            }
            else
            {
                renderers[i].material.color = defaultColors[i];
            }
        }
    }
}
