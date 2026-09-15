using UnityEngine;

public class MoveCanvas : MonoBehaviour
{
    private static MoveCanvas instance;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Debug.Log("参上");

        instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
