using UnityEngine;

public class FloorBlock : MonoBehaviour
{
    // グリッド座標
    public Vector3Int GridPosition;

    [Header("選択色")]
    [SerializeField] private Color selectColor = Color.green;

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