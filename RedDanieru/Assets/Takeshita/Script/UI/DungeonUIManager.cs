using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DungeonUIManager : MonoBehaviour
{
    public DungeonExporter exporter;
    public DungeonImporter importer;

    public InputField dungeonNameInput;
    public InputField creatorNameInput;

    public void UploadDungeon()
    {
        exporter.ExportDungeon(
            dungeonNameInput.text,
            creatorNameInput.text
        );
    }

    public void DownloadDungeon()
    {
        importer.ImportDungeon(
            dungeonNameInput.text
        );
    }
}