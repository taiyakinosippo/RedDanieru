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
    [SerializeField] private GameObject MapSelectObj;
    [SerializeField] private GameObject ShopBackCautionObj;
    [SerializeField] private Button HostShopButton;
    [SerializeField] private Button JoinShopButton;

    [SerializeField] private TextMeshProUGUI multiPlayerCountText;
    [SerializeField] private TextMeshProUGUI GameStartCountText;

    private bool opened;
    private bool isReady;

    public void Start()
    {
        ShopWindowObj.SetActive(false);
        cautionObj.SetActive(false);
        ShopBackCautionObj.SetActive(false);

        HostShopButton.interactable = false;
        JoinShopButton.interactable = false;

        coinText.text = "10,000";

        if (FusionLauncher.Instance != null)
        {
            FusionLauncher.Instance.GameStarted += OnGameStarted;
        }

        GameStartCountText.text = "ゲームスタート";
    }

    private void OnDestroy()
    {
        if (FusionLauncher.Instance != null)
        {
            FusionLauncher.Instance.GameStarted -= OnGameStarted;
        }
    }

    private void OnGameStarted()
    {
        ShopBackCautionObj.SetActive(false);
        ShopWindowObj.SetActive(false);
        MapSelectObj.SetActive(false);
    }

    private void Update()
    {
        if (FusionLauncher.Instance == null)
            return;

        multiPlayerCountText.gameObject.SetActive(
      GameModeManager.IsMultiplayer);

        if (NetworkGameState.Instance != null)
        {
            coinText.text =
                NetworkGameState.Instance.SharedCoin.ToString("N0");
        }

        if (NetworkGameState.Instance != null && NetworkGameState.Instance.ShopOpened && !opened)
        {
            opened = true;

            ShopWindowObj.SetActive(true);
        }

        bool canUseShop =
     FusionLauncher.Instance.PlayerCount >= 2 &&
     !isReady;

        HostShopButton.interactable = canUseShop;
        JoinShopButton.interactable = canUseShop;

        if (GameModeManager.IsMultiplayer &&
        FusionLauncher.Instance != null)
        {
            multiPlayerCountText.text =
      $"{NetworkGameState.Instance.ShopReadyCount}/" +
      $"{FusionLauncher.Instance.PlayerCount}";
        }

        if (GameModeManager.IsMultiplayer &&
    NetworkGameState.Instance != null)
        {
            int countdown =
                NetworkGameState.Instance.StartCountdown;

            if (countdown > 0)
            {
                GameStartCountText.text =
                    countdown.ToString();
            }
            else
            {
                GameStartCountText.text =
                    "ゲームスタート";
            }
        }
    }

    public void ShopWindowActiveButton()
    {
        ShopWindowObj.SetActive(true);
    }

    public void ShopWindowBackButton()
    {
        ShopBackCautionObj.SetActive(true);
    }

    public void ShopBackYesButton()
    {
        ShopBackCautionObj.SetActive(false);
        ShopWindowObj.SetActive(false);
        MapSelectObj.SetActive(true);
    }

    public void ShopBackNoButton()
    {
        ShopBackCautionObj.SetActive(false);
    }

    public void ShopGameStartButton()
    {
        if (!GameModeManager.IsMultiplayer)
        {
            ShopBackCautionObj.SetActive(false);
            ShopWindowObj.SetActive(false);
            MapSelectObj.SetActive(false);

            Time.timeScale = 1f;

            FusionLauncher.Instance.StartSolo();
            return;
        }

        if (isReady)
            return;

        isReady = true;

        NetworkGameState.Instance.RPC_AddShopReady();

        //連打防止
        HostShopButton.interactable = false;
        JoinShopButton.interactable = false;
    }

    public void BuyItem(int price)
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
