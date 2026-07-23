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
        foreach (Renderer renderer in renderers)
        {
            renderer.material.color = selectColor;
        }
    }

    /// <summary>
    /// 選択解除
    /// </summary>
    public void Deselect()
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].material.color = defaultColors[i];
        }
    }
}