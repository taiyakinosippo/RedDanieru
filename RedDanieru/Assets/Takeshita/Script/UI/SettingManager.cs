using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingManager: MonoBehaviour
{
    [SerializeField] private GameObject SettingUI;
    [SerializeField] private GameObject VolumeUI;
    [SerializeField] private GameObject ControllerUI;
    [SerializeField] private GameObject KeyboardUI;

    public void Start()
    {
        SettingUI.SetActive(false);
        VolumeUI.SetActive(true);
        ControllerUI.SetActive(false);
        KeyboardUI.SetActive(false);
    }

    public void SettingUIActive()
    {
        SettingUI.SetActive(true);
    }

    public void SettingUIBack()
    {
        SettingUI.SetActive(false);
        VolumeUI.SetActive(true);
        ControllerUI.SetActive(false);
        KeyboardUI.SetActive(false);
    }

    public void VoluneUIButton()
    {
        VolumeUI.SetActive(true);
        ControllerUI.SetActive(false);
        KeyboardUI.SetActive(false);
    }

    public void ControllerUIButton()
    {
        VolumeUI.SetActive(false);
        ControllerUI.SetActive(true);
        KeyboardUI.SetActive(false);
    }

    public void KeyboardUIButton()
    {
        VolumeUI.SetActive(false);
        ControllerUI.SetActive(false);
        KeyboardUI.SetActive(true);
    }
}
