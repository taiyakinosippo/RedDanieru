using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameStopManager: MonoBehaviour
{
    [SerializeField] private GameObject SettingUI;
    [SerializeField] private GameObject GameStopUI;
    [SerializeField] private GameObject GameStopCautionUI;

    public bool CanOpenPauseMenu { get; private set; }
    public static bool IsPaused { get; private set; }

    private void Update()
    {
        if (!CanOpenPauseMenu)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("ESC");

            if (GameStopUI.activeSelf)
            {
                GameReturnButton();
            }
            else
            {
                GameStop();
            }
        }
    }

    public void EnablePauseMenu()
    {
        CanOpenPauseMenu = true;
    }

    // staticなのでシーンを移動しても残る。残ったままだと次のゲームで動けなくなる
    public static void ResetPauseState()
    {
        IsPaused = false;
    }

    private void OnDestroy()
    {
        ResetPauseState();
    }

    public void DisablePauseMenu()
    {
        CanOpenPauseMenu = false;
    }

    public void Start()
    {
        SettingUI.SetActive(false);
        GameStopUI.SetActive(false);
        GameStopCautionUI.SetActive(false);
    }

    public void GameStop()
    {
        IsPaused = true;

        if (!GameModeManager.IsMultiplayer)
        {
            Time.timeScale = 0f;
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        GameStopUI.SetActive(true);
    }

    public void GameReturnButton()
    {
        IsPaused = false;

        if (!GameModeManager.IsMultiplayer)
        {
            Time.timeScale = 1f;
        }

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        GameStopUI.SetActive(false);
    }

    public void SettingUIButton()
    {
        SettingUI.SetActive(true);
    }

    public void GameStopCautionButton()
    {
        GameStopCautionUI.SetActive(true);
    }

    public void YesButton()
    {
        Time.timeScale = 1f;

        ResetPauseState();

        // 接続を切らずにシーンを移動すると、ルームに自分が残ったままになる
        if (FusionLauncher.Instance != null)
        {
            FusionLauncher.Instance.ShutdownAndLoadTitle("TitleScene");
        }
        else
        {
            SceneManager.LoadScene("TitleScene");
        }
    }

    public void NoButton()
    {
        GameStopCautionUI.SetActive(false);
    }

    public void SettingBackButton()
    {
        SettingUI.SetActive(false);
    }
}
