using UnityEngine;

public class Handsお試 : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            FindFirstObjectByType<HandEffectActive>().HandsOn();
        }

        if (Input.GetKeyUp(KeyCode.Mouse1))
        {
            FindFirstObjectByType<HandEffectActive>().HandsOff();
        }
    }
}
