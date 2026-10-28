
using System.Collections.Generic;
using UnityEngine;

public class TorchLight : MonoBehaviour
{
    [SerializeField] private ObjectPrefab torch;

    private List<GameObject> torches = new List<GameObject>();

    void Update()
    {
        if (!Input.GetKey(KeyCode.Mouse0)) return;

        if (!Physics.Raycast(
            Camera.main.ScreenPointToRay(Input.mousePosition),
            out RaycastHit hit))
            return;

        if (hit.collider.gameObject.layer !=
            LayerMask.NameToLayer("Floor"))
            return;

        SetTorch(hit);
    }

    private void SetTorch(RaycastHit hit)
    {
        foreach (var placedTorch in torches)
        {
            if (placedTorch == null) continue;

            if (Vector3.Distance(
                hit.point,
                placedTorch.transform.position) < 8f)
                return;
        }

        if (torch == null || torch.prefab == null)
        {
            Debug.LogError("松明のPrefabが設定されていません。");
            return;
        }

        GameObject newTorch = Instantiate(
            torch.prefab,
            new Vector3(hit.point.x, 0f, hit.point.z),
            Quaternion.identity
        );

        torches.Add(newTorch);

        // マップ変更を通知
        SaveManager saveManager = FindObjectOfType<SaveManager>();
        if (saveManager != null)
        {
            saveManager.SetMapModified();
        }
    }

    //==================================================
    // 松明の保存データ作成
    //==================================================

    public TorchSaveData CreateTorchSaveData()
    {
        TorchSaveData data = new TorchSaveData();

        foreach (GameObject placedTorch in torches)
        {
            if (placedTorch == null) continue;

            TorchInfo info = new TorchInfo();

            info.position = placedTorch.transform.position;
            info.rotation = placedTorch.transform.rotation;

            data.torches.Add(info);
        }

        return data;
    }

    //==================================================
    // 松明の復元
    //==================================================

    public void LoadTorches(TorchSaveData data)
    {
        ClearTorches();

        if (data == null || data.torches == null)
            return;

        if (torch == null || torch.prefab == null)
        {
            Debug.LogError("松明のPrefabが設定されていません。");
            return;
        }

        foreach (TorchInfo info in data.torches)
        {
            if (info == null) continue;

            GameObject newTorch = Instantiate(
                torch.prefab,
                info.position,
                info.rotation
            );

            torches.Add(newTorch);
        }
    }

    //==================================================
    // 松明を全削除
    //==================================================

    public void ClearTorches()
    {
        foreach (GameObject placedTorch in torches)
        {
            if (placedTorch != null)
                Destroy(placedTorch);
        }

        torches.Clear();
    }
}
