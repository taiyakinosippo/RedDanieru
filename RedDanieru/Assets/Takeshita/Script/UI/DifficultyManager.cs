using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance { get; private set; }

    public enum Difficulty
    {
        Easy,
        Normal,
        Hard
    }

    public Difficulty CurrentDifficulty { get; private set; }
        = Difficulty.Easy;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public void SetDifficulty(int index)
    {
        CurrentDifficulty = (Difficulty)index;

        Debug.Log($"難易度 : {CurrentDifficulty}");
    }
}