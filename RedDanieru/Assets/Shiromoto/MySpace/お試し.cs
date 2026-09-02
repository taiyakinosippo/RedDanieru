using UnityEngine;

public class お試し : MonoBehaviour
{
    [SerializeField]CameraRenderer cr;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) cr.Bright();
        if (Input.GetKeyDown(KeyCode.Space)) cr.Dark();
    }
}
