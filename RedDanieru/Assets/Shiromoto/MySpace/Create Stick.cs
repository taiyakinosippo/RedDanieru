using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class CreateStick : MonoBehaviour
{
    [SerializeField] Stick stickPrefab;

    private EffectPermission myMode = EffectPermission.OK;

    [SerializeField] private LayerMask effectArea;

    // ポーズ中のクリックEffectモード
    private bool pauseEffectMode = false;

    public enum EffectPermission
    {
        OK,
        depends,
        NG
    };

    void Update()
    {
        if (Camera.main == null)
        {
            if (Input.GetKeyDown(KeyCode.Mouse0)) Inster();
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            // ポーズ中
            if (pauseEffectMode)
            {
                Inster();
                return;
            }

            if (myMode != EffectPermission.OK)
            {
                GameObject player =
                    GameObject.Find("PlayerArmature(Clone)");

                if (player != null)
                {
                    myMode = EffectPermission.NG;
                }
                else
                {
                    myMode = EffectPermission.depends;
                }
            }

            if (EventSystem.current.IsPointerOverGameObject())
            {
                Inster();
            }
            else
            {
                switch (myMode)
                {
                    case EffectPermission.OK:
                        break;

                    case EffectPermission.depends:

                        RaycastHit[] hits =
                            Physics.RaycastAll(ray);

                        foreach (RaycastHit hit in hits)
                        {
                            if ((effectArea.value &
                                (1 << hit.collider.gameObject.layer)) == 0)
                            {
                                return;
                            }
                        }

                        break;

                    case EffectPermission.NG:
                        return;
                }

                Inster();
            }
        }
    }

    private void Inster()
    {
        int rotate = 0;

        while (rotate < 360)
        {
            Stick stick =
                Instantiate(stickPrefab, transform);

            stick.transform.position =
                Input.mousePosition;

            stick.Initialize(
                rotate,
                Random.Range(50f, 60f),
                Random.Range(0.2f, 0.5f),
                Random.Range(15f, 25f)
            );

            int x = Random.Range(25, 40);

            rotate += x;
        }
    }

    public void ChangeMode(EffectPermission mode)
    {
        myMode = mode;
    }

    //==================================================
    // ポーズ中のクリックEffectモード
    //==================================================

    public void SetPauseEffectMode(bool enabled)
    {
        pauseEffectMode = enabled;

        if (enabled)
        {
            // ポーズ中は強制的にEffectを許可
            myMode = EffectPermission.OK;
        }
    }
}