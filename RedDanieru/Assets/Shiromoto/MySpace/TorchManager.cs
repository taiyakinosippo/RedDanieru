using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class TorchManager : MonoBehaviour
{
    [SerializeField] private ObjectPrefab torch;

    private List<GameObject> walls = new List<GameObject>();
    private List<Vector3> canSetPosition = new List<Vector3>();
    private List<Vector3> lightList = new List<Vector3>();

    private float xMax;
    private float xMin;
    private float zMax;
    private float zMin;

    [SerializeField] public float lightDistance = 8.0f;


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q)) SetTorch();
    }


    public void SetTorch()
    {
        walls.Clear();
        canSetPosition.Clear();
        SearchOutLine();
        SearchCanPosition();
        Action();
    }

    private void SearchOutLine()
    {
        var objects = GameObject.Find("MapManager").GetComponentsInChildren<Transform>();
        foreach (Transform obj in objects)
        {
            if (obj.gameObject.layer == LayerMask.NameToLayer("Wall"))
            {
                walls.Add(obj.gameObject);
            }
        }

        xMax = walls.Max(x => x.transform.position.x);
        xMin = walls.Min(x => x.transform.position.x);
        zMax = walls.Max(z => z.transform.position.z);
        zMin = walls.Min(z => z.transform.position.z);
    }


    private void SearchCanPosition()
    {
        foreach (GameObject wall in walls)
        {
            CheckCanSet(wall);
        }
    }


    private void Action()
    {
        foreach (Vector3 pos in canSetPosition)
        {
            bool canPlace = true;

            foreach (Vector3 lightPos in lightList)
            {
                if (Vector3.Distance(pos, lightPos) < lightDistance)
                {
                    canPlace = false;
                    break;
                }
            }

            if (!canPlace)
                continue;

            Vector3 spawnPos =
                new Vector3(pos.x, 0f, pos.z);

            Instantiate(
                torch.prefab,
                spawnPos,
                Quaternion.identity);

            lightList.Add(spawnPos);
        }
    }


    Vector3[] directions =
    {
        Vector3.forward,
        Vector3.back,
        Vector3.left,
        Vector3.right
    };

    private void CheckCanSet(GameObject wall)
    {
        foreach (Vector3 dir in directions)
        {
            Ray ray = new Ray(wall.transform.position, dir);

            if (!Physics.Raycast(ray, out RaycastHit hit, 1.0f, LayerMask.GetMask("Wall")))
            {
                IsInside(wall.transform.position, dir);
            }
        }
    }

    private void IsInside(Vector3 pos, Vector3 dir)
    {
        Vector3 lightPos = pos + dir;

        if (lightPos.x < xMin) return;
        if (lightPos.x > xMax) return;

        if (lightPos.z < zMin) return;
        if (lightPos.z > zMax) return;

        foreach(Vector3 csp in canSetPosition)
        {
            if (csp == lightPos) return;
        }

        canSetPosition.Add(lightPos);
    }
}
