using UnityEngine;

public class EditModeManager : MonoBehaviour
{
    public static EditModeManager Instance;

    public EditMode CurrentMode { get; private set; } = EditMode.Dig;

    void Awake()
    {
        Instance = this;
    }

    public void SetDigMode()
    {
        CurrentMode = EditMode.Dig;
    }

    public void SetPlaceMode()
    {
        CurrentMode = EditMode.Place;
    }
}