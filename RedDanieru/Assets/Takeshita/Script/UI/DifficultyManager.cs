using UnityEngine;
using UnityEngine.UI;

public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance { get; private set; }

    public enum Difficulty
    {
        Easy,
        Normal,
        Hard
    }

    [Header("難易度ボタン")]
    [SerializeField] private Button easyButton;
    [SerializeField] private Button normalButton;
    [SerializeField] private Button hardButton;

    public Difficulty CurrentDifficulty { get; private set; }
        = Difficulty.Easy;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void Start()
    {
        UpdateButtonState();
    }

    public void SelectEasy()
    {
        CurrentDifficulty = Difficulty.Easy;

        Debug.Log($"難易度 : {CurrentDifficulty}");

        UpdateButtonState();
    }

    public void SelectNormal()
    {
        CurrentDifficulty = Difficulty.Normal;

        Debug.Log($"難易度 : {CurrentDifficulty}");

        UpdateButtonState();
    }

    public void SelectHard()
    {
        CurrentDifficulty = Difficulty.Hard;

        Debug.Log($"難易度 : {CurrentDifficulty}");

        UpdateButtonState();
    }

    private void UpdateButtonState()
    {
        if (easyButton != null)
        {
            easyButton.interactable =
                CurrentDifficulty != Difficulty.Easy;
        }

        if (normalButton != null)
        {
            normalButton.interactable =
                CurrentDifficulty != Difficulty.Normal;
        }

        if (hardButton != null)
        {
            hardButton.interactable =
                CurrentDifficulty != Difficulty.Hard;
        }
    }
}
