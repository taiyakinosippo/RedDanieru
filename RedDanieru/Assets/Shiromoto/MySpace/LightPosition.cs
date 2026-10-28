using Newtonsoft.Json.Bson;
using UnityEngine;

public class LightPosition : MonoBehaviour
{
    [SerializeField] private GameObject bodyLight;
    [SerializeField] private GameObject upLight;

    private Vector3 position;
    private Camera charaCamera;
    private Transform headPos;

    [SerializeField] private float kyori;
    [SerializeField] private float takasa;

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
        bodyLight.transform.position = headPos.position;
    }

    private void UpLight()
    {
        if (charaCamera == null) return;

        upLight.transform.position = position - charaCamera.transform.forward * kyori + Vector3.up * takasa;
    }
}
