using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEditor;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

public class FBXDownloader : MonoBehaviour
{
    private string saveDirectory = "C:/Users/PARD1/Desktop/HSML_Models";
    private string jsonDirectory = "C:/Users/PARD1/Desktop/HSML_Models/HSML";

    void Start()
    {
        StartCoroutine(ProcessAllJsonFiles());
    }

    IEnumerator ProcessAllJsonFiles()
    {
        if (!Directory.Exists(jsonDirectory))
        {
            Debug.LogError("JSON directory not found: " + jsonDirectory);
            yield break;
        }

        string[] jsonFiles = Directory.GetFiles(jsonDirectory, "*.json");
        List<IEnumerator> downloadCoroutines = new List<IEnumerator>();

        foreach (string jsonFilePath in jsonFiles)
        {
            JObject jsonData = ReadJsonData(jsonFilePath);
            if (jsonData == null) continue;

            string fileName = (string)jsonData["name"] + ".fbx";
            string googleDriveFileID = ExtractGoogleDriveID((string)jsonData["url"]);

            if (!string.IsNullOrEmpty(googleDriveFileID))
            {
                downloadCoroutines.Add(DownloadAndLoadFBX(googleDriveFileID, fileName, jsonData));
            }
        }

        foreach (var coroutine in downloadCoroutines)
        {
            yield return StartCoroutine(coroutine);
        }
    }

    JObject ReadJsonData(string jsonFilePath)
    {
        if (!File.Exists(jsonFilePath))
        {
            Debug.LogError("JSON file not found: " + jsonFilePath);
            return null;
        }

        string jsonText = File.ReadAllText(jsonFilePath);
        return JObject.Parse(jsonText);
    }

    string ExtractGoogleDriveID(string url)
    {
        Match match = Regex.Match(url, @"[-\w]{25,}");
        return match.Success ? match.Value : string.Empty;
    }

    IEnumerator DownloadAndLoadFBX(string googleDriveFileID, string fileName, JObject jsonData)
    {
        if (string.IsNullOrEmpty(googleDriveFileID))
        {
            Debug.LogError("Invalid Google Drive File ID");
            yield break;
        }

        if (!Directory.Exists(saveDirectory))
        {
            Directory.CreateDirectory(saveDirectory);
        }

        string filePath = Path.Combine(saveDirectory, fileName);
        string fileURL = $"https://drive.google.com/uc?export=download&id={googleDriveFileID}";

        using (UnityWebRequest webRequest = UnityWebRequest.Get(fileURL))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Download Failed: " + webRequest.error);
            }
            else
            {
                File.WriteAllBytes(filePath, webRequest.downloadHandler.data);
                Debug.Log("Download Successful: " + filePath);
                ImportFBXToAssets(filePath, jsonData);
            }
        }
    }

    void ImportFBXToAssets(string filePath, JObject jsonData)
    {
        string assetPath = "Assets/DownloadedModels/" + Path.GetFileName(filePath);
        string assetDirectory = Path.GetDirectoryName(assetPath);

        if (!Directory.Exists(assetDirectory))
        {
            Directory.CreateDirectory(assetDirectory);
        }

        File.Copy(filePath, assetPath, true);
        AssetDatabase.ImportAsset(assetPath);
        LoadFBXModel(assetPath, jsonData);
    }

    void LoadFBXModel(string assetPath, JObject jsonData)
    {
        Vector3 position = ReadPositionFromJson(jsonData);
        Quaternion rotation = ReadRotationFromJson(jsonData);
        Vector3 scale = ReadScaleFromJson(jsonData);

        GameObject loadedModel = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        if (loadedModel != null)
        {
            GameObject instance = Instantiate(loadedModel, position, rotation);
            instance.transform.localScale = scale;
            Debug.Log("Model Loaded Successfully at Position: " + position + " with Scale: " + scale);
        }
        else
        {
            Debug.LogError("Failed to Load Model. Ensure it's properly imported.");
        }
    }

    Vector3 ReadPositionFromJson(JObject jsonData)
    {
        float x = (float)jsonData["additionalProperty"][0]["value"];
        float y = (float)jsonData["additionalProperty"][1]["value"];
        float z = (float)jsonData["additionalProperty"][2]["value"];

        return new Vector3(x, y, z);
    }

    Quaternion ReadRotationFromJson(JObject jsonData)
    {
        float rx = (float)jsonData["additionalProperty"][3]["value"];
        float ry = (float)jsonData["additionalProperty"][4]["value"];
        float rz = (float)jsonData["additionalProperty"][5]["value"];
        float w = (float)jsonData["additionalProperty"][6]["value"];

        return new Quaternion(rx, ry, rz, w);
    }

    Vector3 ReadScaleFromJson(JObject jsonData)
    {
        float scaleValue = (float)jsonData["additionalProperty"][7]["value"];
        return new Vector3(scaleValue, scaleValue, scaleValue);
    }
}