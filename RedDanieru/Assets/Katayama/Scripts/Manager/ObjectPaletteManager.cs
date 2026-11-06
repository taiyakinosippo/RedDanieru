
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 現在選択中の配置オブジェクトを管理
/// </summary>
public class ObjectPaletteManager : MonoBehaviour
{
    public static ObjectPaletteManager Instance { get; private set; }

    public PlaceObjectType CurrentObject { get; private set; }

    [Serializable]
    private class ObjectButtonEntry
    {
        public PlaceObjectType type;
        public Button button;
    }

    [Header("配置タイプとボタンの対応")]
    [SerializeField] private ObjectButtonEntry[] objectButtonEntries;

    [Header("ボタンカラー")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = Color.yellow;

    // 配置タイプとボタンの対応表
    private Dictionary<PlaceObjectType, Button> buttonMap;

    // 現在選択中のボタン
    private Button currentObjectButton;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        BuildButtonMap();

        CurrentObject = PlaceObjectType.Chest;
    }

    private void Start()
    {
        ResetSelection();
    }

    private void BuildButtonMap()
    {
        buttonMap = new Dictionary<PlaceObjectType, Button>();

        if (objectButtonEntries == null)
            return;

        foreach (ObjectButtonEntry entry in objectButtonEntries)
        {
            if (entry == null || entry.button == null)
                continue;

            if (buttonMap.ContainsKey(entry.type))
            {
                Debug.LogWarning(
                    $"配置タイプが重複しています: {entry.type}");
                continue;
            }

            buttonMap.Add(entry.type, entry.button);
        }
    }

    /// <summary>
    /// 配置するオブジェクトを変更
    /// ButtonのOnClickから呼び出す
    /// </summary>
    public void SelectObject(int type)
    {
        SelectObject((PlaceObjectType)type);
    }

    /// <summary>
    /// 配置タイプを指定して選択
    /// </summary>
    public void SelectObject(PlaceObjectType type)
    {
        CurrentObject = type;

        // 前のボタンを通常色に戻す
        if (currentObjectButton != null)
        {
            SetButtonColor(currentObjectButton, normalColor);
        }

        // 新しく選択したボタンを取得
        currentObjectButton = null;

        if (buttonMap != null &&
            buttonMap.TryGetValue(type, out Button selectedButton))
        {
            currentObjectButton = selectedButton;

            SetButtonColor(
                currentObjectButton,
                selectedColor
            );
        }
        else
        {
            Debug.LogWarning(
                $"選択タイプのボタンが登録されていません: {type}");
        }

        // 配置モードに切り替える
        if (EditModeManager.Instance != null)
        {
            EditModeManager.Instance.SetPlaceMode();

            // モード切替後に選択色を再設定
            if (currentObjectButton != null)
            {
                SetButtonColor(
                    currentObjectButton,
                    selectedColor
                );
            }
        }

        Debug.Log($"選択中 : {CurrentObject}");
    }

    /// <summary>
    /// オブジェクト選択を解除
    /// </summary>
    public void ResetSelection()
    {
        if (objectButtonEntries != null)
        {
            foreach (ObjectButtonEntry entry in objectButtonEntries)
            {
                if (entry == null || entry.button == null)
                    continue;

                SetButtonColor(entry.button, normalColor);
            }
        }

        currentObjectButton = null;
    }

    /// <summary>
    /// ボタンの色を設定
    /// </summary>
    private void SetButtonColor(Button button, Color color)
    {
        if (button == null)
            return;

        ColorBlock colors = button.colors;

        colors.normalColor = color;
        colors.highlightedColor =
            Color.Lerp(color, Color.white, 0.2f);
        colors.pressedColor =
            Color.Lerp(color, Color.black, 0.25f);
        colors.selectedColor = color;

        button.colors = colors;
    }
}
