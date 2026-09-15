using UnityEngine;
using UnityEngine.UI;

public class EditModeManager : MonoBehaviour
{
    public static EditModeManager Instance;

    public EditMode CurrentMode { get; private set; } = EditMode.Dig;

    [Header("モードボタン")]
    [SerializeField] private Button digButton;
    [SerializeField] private Button deleteButton;

    [Header("ボタンカラー")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = Color.yellow;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        SetButtonColor(digButton, selectedColor);
        SetButtonColor(deleteButton, normalColor);
    }

    /// <summary>
    /// 掘削モード
    /// </summary>
    public void SetDigMode()
    {
        CurrentMode = EditMode.Dig;

        SetButtonColor(digButton, selectedColor);
        SetButtonColor(deleteButton, normalColor);

        if (ObjectPaletteManager.Instance != null)
        {
            ObjectPaletteManager.Instance.ResetSelection();
        }
    }

    /// <summary>
    /// 配置モード
    /// </summary>
    public void SetPlaceMode()
    {
        CurrentMode = EditMode.Place;

        SetButtonColor(digButton, normalColor);
        SetButtonColor(deleteButton, normalColor);
    }

    /// <summary>
    /// 削除モード
    /// </summary>
    public void SetDeleteMode()
    {
        CurrentMode = EditMode.Delete;

        SetButtonColor(digButton, normalColor);
        SetButtonColor(deleteButton, selectedColor);

        if (ObjectPaletteManager.Instance != null)
        {
            ObjectPaletteManager.Instance.ResetSelection();
        }
    }

    /// <summary>
    /// ボタンの状態ごとの色を設定
    /// </summary>
    private void SetButtonColor(Button button, Color color)
    {
        if (button == null)
            return;

        ColorBlock colors = button.colors;

        // 通常時
        colors.normalColor = color;

        // マウスを乗せたとき
        colors.highlightedColor =
            Color.Lerp(color, Color.white, 0.2f);

        // 押しているとき
        colors.pressedColor =
            Color.Lerp(color, Color.black, 0.25f);

        // 選択状態
        colors.selectedColor = color;

        button.colors = colors;
    }
}