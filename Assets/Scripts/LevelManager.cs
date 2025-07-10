using System;
using System.IO;
using UnityEngine;

/// <summary>
/// Saves and loads levels data
/// </summary>
public class LevelManager : MonoBehaviour
{
    // ___ SINGELTON ___
    public static LevelManager Instance;

    // ___ PUBLIC PROPERTIES ___
    public const int LevelCount = 9; 

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

    }

    // ___ PRIVATE METHODS ___
    private void CreateSaveFile()
    {
        // Create and populate the save data model
        SaveDataModel saveData = new SaveDataModel();
        saveData.levelDatas = new LevelData[9];
        for (int i = 0; i < saveData.levelDatas.Length; i++)
        {
            saveData.levelDatas[i] = new LevelData
            {
                levelIndex = i,
                star1 = false,
                star2 = false,
                star3 = false
            };
        }

        // Save to JSON file
        string json = JsonUtility.ToJson(saveData);
        File.WriteAllText(Application.persistentDataPath + "/save.json", json);
    }

    private void ValidateSaveFile()
    {
        // Check if the save file exists
        if (!File.Exists(Application.persistentDataPath + "/save.json"))
        {
            CreateSaveFile();
        }

        // Check if the save file is populated correctly
        var saveDataModel = JsonUtility.FromJson<SaveDataModel>(File.ReadAllText(Application.persistentDataPath + "/save.json"));
        if (saveDataModel.levelDatas == null || saveDataModel.levelDatas.Length == 0)
        {
            CreateSaveFile();
            Debug.LogWarning("Save file was empty, created a new one.");
        }
    }

    // ___ PUBLIC METHODS ___
    public void SaveData(int levelIndex, LevelData data)
    {
        // Check if the save file exists, if not create it
        ValidateSaveFile();

        // Load the existing save data
        SaveDataModel saveDataModel = JsonUtility.FromJson<SaveDataModel>(File.ReadAllText(Application.persistentDataPath + "/save.json"));
        // Check if the level index is valid
        if (levelIndex < 0 || levelIndex >= saveDataModel.levelDatas.Length)
        {
            Debug.LogError("Invalid level index: " + levelIndex);
            return;
        }

        // Save the updated data back to the file
        saveDataModel.levelDatas[levelIndex] = data;

        string json = JsonUtility.ToJson(saveDataModel, true);
        File.WriteAllText(Application.persistentDataPath + "/save.json", json);
    }

    public bool LoadData(int levelIndex, out LevelData data)
    {
        data = new LevelData();

        // Check if the save file exists
        if (!File.Exists(Application.persistentDataPath + "/save.json"))
            return false;


        // Load the save file
        var saveDataModel = JsonUtility.FromJson<SaveDataModel>(File.ReadAllText(Application.persistentDataPath + "/save.json"));
        // Check if the level index is valid
        if (levelIndex < 0 || levelIndex >= saveDataModel.levelDatas.Length)
        {
            Debug.LogError("Invalid level index: " + levelIndex);
            return false;
        }

        // Get the level data for the specified index
        data = saveDataModel.levelDatas[levelIndex];
        return true;
    }

    // For convinience
    public void QuitGame()
    {
        Application.Quit();

    }

}

[Serializable]
public struct LevelData
{
    public int levelIndex;
    public bool star1;
    public bool star2;
    public bool star3;
}

[Serializable]
public class SaveDataModel
{
    // Array holding all the levels data
    public LevelData[] levelDatas;
}
