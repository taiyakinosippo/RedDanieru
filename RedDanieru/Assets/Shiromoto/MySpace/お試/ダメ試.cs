using UnityEngine;

public class ダメ試 : MonoBehaviour
{
    [SerializeField] GetDamageManager g;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.D)) g.GetDamage();
    }
}
