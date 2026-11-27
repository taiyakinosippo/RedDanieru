using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// Enemy タグの敵すべてに、控えめなアウトラインを付ける
// ・ある程度近づくと、暗い中でも敵の輪郭がうっすら見える
// ・ゲーム開始時に自動で動くので、シーンやプレハブに置く必要はない
// ・色・濃さ・太さ・見える距離は Assets/Resources/EnemyOutline.mat で変える（再生中に変えてもすぐ反映）
// ・元のマテリアルはそのまま残し、アウトライン用のマテリアルを足すだけ
public class EnemyOutlineApplier : MonoBehaviour
{
    private const string MaterialPath = "EnemyOutline";  // Resources からのパス
    private const string OutlineChildName = "EnemyOutline";
    private const float CheckInterval = 0.5f;  // 後から出てくる敵を拾う間隔(秒)

    private Material outlineMaterial;
    private readonly HashSet<Renderer> appliedRenderers = new HashSet<Renderer>();
    private float timer;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Create()
    {
        Material material = Resources.Load<Material>(MaterialPath);

        if (material == null)
        {
            Debug.LogWarning("EnemyOutlineApplier: Resources/" + MaterialPath + ".mat が見つからないのでアウトラインを付けません。");
            return;
        }

        GameObject applierObject = new GameObject("EnemyOutlineApplier");
        DontDestroyOnLoad(applierObject);

        EnemyOutlineApplier applier = applierObject.AddComponent<EnemyOutlineApplier>();
        applier.outlineMaterial = material;
    }

    private void Update()
    {
        timer -= Time.deltaTime;

        if (timer > 0f)
        {
            return;
        }

        timer = CheckInterval;

        // 倒された敵の分を消す
        appliedRenderers.RemoveWhere(rend => rend == null);

        foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            foreach (Renderer rend in enemy.GetComponentsInChildren<Renderer>(true))
            {
                if (appliedRenderers.Add(rend))
                {
                    ApplyOutline(rend);
                }
            }
        }
    }

    private void ApplyOutline(Renderer rend)
    {
        // 自分で作ったアウトライン用の Renderer には付けない
        if (rend.gameObject.name == OutlineChildName)
        {
            return;
        }

        Mesh mesh = GetMesh(rend);
        Material[] materials = rend.sharedMaterials;

        if (mesh == null || materials.Length == 0 || IsTransparent(materials))
        {
            return;
        }

        // サブメッシュもマテリアルも1つなら、マテリアルを1枚足すだけでメッシュ全体に線が出る
        if (mesh.subMeshCount == 1 && materials.Length == 1)
        {
            rend.sharedMaterials = new Material[] { materials[0], outlineMaterial };
            return;
        }

        // 複数あると足したマテリアルは最後のサブメッシュにしか出ないので、
        // 同じメッシュを使う子オブジェクトを作り、全サブメッシュにアウトラインを付ける
        CreateOutlineChild(rend, mesh);
    }

    private void CreateOutlineChild(Renderer rend, Mesh mesh)
    {
        Material[] outlines = new Material[mesh.subMeshCount];

        for (int i = 0; i < outlines.Length; i++)
        {
            outlines[i] = outlineMaterial;
        }

        GameObject child = new GameObject(OutlineChildName);
        child.layer = rend.gameObject.layer;
        child.transform.SetParent(rend.transform, false);

        Renderer copy;

        if (rend is SkinnedMeshRenderer skinned)
        {
            SkinnedMeshRenderer skinnedCopy = child.AddComponent<SkinnedMeshRenderer>();
            skinnedCopy.sharedMesh = skinned.sharedMesh;
            skinnedCopy.bones = skinned.bones;
            skinnedCopy.rootBone = skinned.rootBone;
            skinnedCopy.localBounds = skinned.localBounds;
            skinnedCopy.updateWhenOffscreen = skinned.updateWhenOffscreen;
            copy = skinnedCopy;
        }
        else
        {
            child.AddComponent<MeshFilter>().sharedMesh = mesh;
            copy = child.AddComponent<MeshRenderer>();
        }

        copy.sharedMaterials = outlines;
        copy.shadowCastingMode = ShadowCastingMode.Off;
        copy.receiveShadows = false;
    }

    // MeshRenderer・SkinnedMeshRenderer 以外（パーティクルなど）は null
    private static Mesh GetMesh(Renderer rend)
    {
        if (rend is SkinnedMeshRenderer skinned)
        {
            return skinned.sharedMesh;
        }

        if (rend is MeshRenderer)
        {
            MeshFilter filter = rend.GetComponent<MeshFilter>();
            return filter != null ? filter.sharedMesh : null;
        }

        return null;
    }

    // 半透明のエフェクト（スタンの星など）には線を付けない
    private static bool IsTransparent(Material[] materials)
    {
        foreach (Material material in materials)
        {
            if (material != null && material.renderQueue >= (int)RenderQueue.Transparent)
            {
                return true;
            }
        }

        return false;
    }
}
