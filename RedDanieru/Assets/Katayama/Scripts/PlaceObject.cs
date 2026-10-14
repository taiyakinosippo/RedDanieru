using UnityEngine;

public class PlaceObject : MonoBehaviour
{
    public PlaceObjectType objectType;

    // 配置したマス
    public Vector3Int GridPosition { get; set; }

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

    /// <summary>
    /// 選択
    /// </summary>
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

    /// <summary>
    /// 選択解除
    /// </summary>
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