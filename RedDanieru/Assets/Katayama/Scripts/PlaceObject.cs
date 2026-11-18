
using UnityEngine;

public class PlaceObject : MonoBehaviour
{
    public PlaceObjectType objectType;

    // 配置した基準マス
    public Vector3Int GridPosition { get; set; }

    [Header("占有マス設定")]
    [SerializeField, Min(1)] private int gridWidth = 1;
    [SerializeField, Min(1)] private int gridDepth = 1;

    public int GridWidth => Mathf.Max(1, gridWidth);
    public int GridDepth => Mathf.Max(1, gridDepth);

    [Header("選択色")]
    [SerializeField] private Color selectColor = Color.red;

    private Renderer[] renderers;
    private Color[] defaultColors;
    private string[] colorProperties;

    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>(true);
        defaultColors = new Color[renderers.Length];
        colorProperties = new string[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            Material material = renderers[i].material;

            if (material.HasProperty("_BaseColor"))
            {
                colorProperties[i] = "_BaseColor";
                defaultColors[i] = material.GetColor("_BaseColor");
            }
            else if (material.HasProperty("_Color"))
            {
                colorProperties[i] = "_Color";
                defaultColors[i] = material.GetColor("_Color");
            }
            else
            {
                colorProperties[i] = null;
            }
        }
    }

    public void Select()
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            if (string.IsNullOrEmpty(colorProperties[i]))
                continue;

            renderers[i].material.SetColor(
                colorProperties[i],
                selectColor
            );
        }
    }

    public void Deselect()
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            if (string.IsNullOrEmpty(colorProperties[i]))
                continue;

            renderers[i].material.SetColor(
                colorProperties[i],
                defaultColors[i]
            );
        }
    }
}
