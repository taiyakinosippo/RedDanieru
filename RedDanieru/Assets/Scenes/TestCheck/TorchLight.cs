using System.Collections.Generic;
using UnityEngine;

public class TorchLight : MonoBehaviour
{
    [SerializeField] private ObjectPrefab torch;
    private List<GameObject> torches = new();

    void Update()
    {
        if (!Input.GetKey(KeyCode.Mouse0)) return;

        if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition),out RaycastHit hit)) return;
        if (hit.collider.gameObject.layer != LayerMask.NameToLayer("Floor")) return;

        SetTorch(hit);
    }

    private void SetTorch(RaycastHit hit)
    {
        foreach (var placedTorch in torches)
        {
            if (Vector3.Distance(hit.point,placedTorch.transform.position) < 8f) return;
        }

        GameObject newTorch = Instantiate(torch.prefab, hit.point, Quaternion.identity);
        torches.Add(newTorch);
    }
}
