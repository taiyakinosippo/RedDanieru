using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 現在選択中の配置オブジェクトを管理
/// </summary>
public class ObjectPaletteManager : MonoBehaviour
{
    public static ObjectPaletteManager Instance { get; private set; }

    public PlaceObjectType CurrentObject { get; private set; }

    [Header("オブジェクト選択ボタン")]
    [SerializeField] private Button[] objectButtons;

    [Header("ボタンカラー")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = Color.yellow;

    private Button currentObjectButton;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        CurrentObject = PlaceObjectType.Chest;
    }

    private void Start()
    {
        ResetSelection();
    }

    /// <summary>
    /// 配置するオブジェクトを変更
    /// </summary>
    public void SelectObject(int type)
    {
        CurrentObject = (PlaceObjectType)type;

        // 前に選択していたボタンを通常状態に戻す
        if (currentObjectButton != null)
        {
            SetButtonColor(
                currentObjectButton,
                normalColor
            );
        }

        // 新しく選択したボタンを黄色にする
        if (objectButtons != null &&
            type >= 0 &&
            type < objectButtons.Length)
        {
            currentObjectButton = objectButtons[type];

            SetButtonColor(
                currentObjectButton,
                selectedColor
            );
        }

        // オブジェクトを選択したら配置モードにする
        if (EditModeManager.Instance != null)
        {
            EditModeManager.Instance.SetPlaceMode();

            // SetPlaceModeでモードボタンが通常色になるため、
            // オブジェクトボタンの選択状態をもう一度設定する
            if (currentObjectButton != null)
            {
                SetButtonColor(
                    currentObjectButton,
                    selectedColor
                );
            }
        }

        Debug.Log($"選択中 : {CurrentObject}");
    }

    /// <summary>
    /// オブジェクト選択を解除
    /// </summary>
    public void ResetSelection()
    {
        if (objectButtons != null)
        {
            for (int i = 0; i < objectButtons.Length; i++)
            {
                SetButtonColor(
                    objectButtons[i],
                    normalColor
                );
            }
        }

        currentObjectButton = null;
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