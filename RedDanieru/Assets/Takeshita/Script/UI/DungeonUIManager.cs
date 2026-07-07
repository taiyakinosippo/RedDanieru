using UnityEngine;
using TMPro;

public class DungeonUIManager : MonoBehaviour
{
    public DungeonExporter exporter;
    public DungeonImporter importer;

    public TMP_InputField dungeonNameInput;
    public TMP_InputField creatorNameInput;

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