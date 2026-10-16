using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class ShopManager: MonoBehaviour
{
    [SerializeField] private GameObject ShopWindowObj;
    [SerializeField] private TextMeshProUGUI coinText;
    [SerializeField] private GameObject cautionObj;
    [SerializeField] private Button HostShopButton;
    [SerializeField] private Button JoinShopButton;

    public void Start()
    {
        ShopWindowObj.SetActive(false);
        cautionObj.SetActive(false);

        HostShopButton.interactable = false;
        JoinShopButton.interactable = false;

        coinText.text = "10,000";
    }

    private void Update()
    {
        if (FusionLauncher.Instance == null)
            return;

        if (NetworkGameState.Instance != null)
        {
            coinText.text =
                NetworkGameState.Instance.SharedCoin.ToString("N0");
        }

        bool canUseShop = FusionLauncher.Instance.PlayerCount >= 2;

        HostShopButton.interactable = canUseShop;
        JoinShopButton.interactable = canUseShop;
    }

    public void ShopWindowActiveButton()
    {
        ShopWindowObj.SetActive(true);
    }

    public void ShowWindowBackButton()
    {
        ShopWindowObj.SetActive(false);
    }

    public void ItemButton_1()
    {
        BuyItem(500);
    }

    public void ItemButton_2()
    {
        BuyItem(1000);
    }

    public void ItemButton_3()
    {
        BuyItem(3000);
    }

    private void BuyItem(int price)
    {
        if (NetworkGameState.Instance == null)
            return;

        if (NetworkGameState.Instance.SharedCoin < price)
        {
            StartCoroutine(CautionObjSetActive());
            return;
        }

        NetworkGameState.Instance.RPC_BuyItem(price);
    }

    private IEnumerator CautionObjSetActive()
    {
        cautionObj.SetActive(true);
        yield return new WaitForSeconds(1f);
        cautionObj.SetActive(false);
    }
}
