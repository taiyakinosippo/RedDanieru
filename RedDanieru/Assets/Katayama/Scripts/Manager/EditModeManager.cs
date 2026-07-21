using UnityEngine;

public class EditModeManager : MonoBehaviour
{
    public static EditModeManager Instance;

    public EditMode CurrentMode { get; private set; } = EditMode.Dig;

    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// 掘削モード
    /// </summary>
    public void SetDigMode()
    {
        CurrentMode = EditMode.Dig;
    }

    /// <summary>
    /// 配置モード
    /// </summary>
    public void SetPlaceMode()
    {
        CurrentMode = EditMode.Place;
    }

    /// <summary>
    /// 削除モード
    /// </summary>
    public void SetDeleteMode()
    {
        CurrentMode = EditMode.Delete;
    }
}