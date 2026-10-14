using UnityEngine;
using UnityEngine.SceneManagement;

public class ShopSceneManager: MonoBehaviour
{
    [SerializeField] private GameObject ShopWindowObj;

    public void Start()
    {
        ShopWindowObj.SetActive(false);
    }

    public void ShopWindowActiveButton()
    {
        ShopWindowObj.SetActive(true);
    }

    public void ShowWindowBackButton()
    {
        ShopWindowObj.SetActive(false);
    }
}
