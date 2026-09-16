using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CreateStick : MonoBehaviour
{
    [SerializeField] Stick stickPrefab;
    private bool can = false;

    [SerializeField]private List<string> NGScene = new List<string>()
    {
        
    };

    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (NGScene.Contains(SceneManager.GetActiveScene().name))
            {
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    if (can == false) return;
                }
            }

            Inster();
        }
    }

    private void Inster()
    {
        int rotate = 0;

        while(rotate < 360){
            Stick stick = Instantiate(stickPrefab, transform);
            stick.transform.position = Input.mousePosition;
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

    public void EffectTrue()
    {
        can = true;
    }

    public void EffectFalse()
    {
        can = false;
    }
}
