using Newtonsoft.Json.Bson;
using UnityEngine;

public class TestLightPosition : MonoBehaviour
{
    [SerializeField] private GameObject bodyLight;
    [SerializeField] private GameObject upLight;

    private Vector3 position;
    private Camera charaCamera;
    private Transform headPos;

    void Start()
    {
        Transform parent = transform.parent;
        charaCamera = parent.GetComponentInChildren<Camera>();

        foreach (Transform t in transform.parent.GetComponentsInChildren<Transform>(true))
        {
            if (t.name == "Head")
            {
                headPos = t;
                break;
            }
        }
    }

    void Update()
    {

        Transform parent = transform.parent;
        charaCamera = parent.GetComponentInChildren<Camera>();
        foreach (Transform t in transform.parent.GetComponentsInChildren<Transform>(true))
        {
            if (t.name == "head")
            {
                headPos = t;
                break;
            }
        }


        position = transform.parent.position;

        BodyLight();
        UpLight();
    }

    private void BodyLight()
    {
        bodyLight.transform.position = headPos.position - charaCamera.transform.forward * 0.8f + Vector3.up * 0.3f;
    }

    private void UpLight()
    {
        if (charaCamera == null) return;

        upLight.transform.position = position + charaCamera.transform.forward * 2.5f + Vector3.up * 1.2f;
    }
}
