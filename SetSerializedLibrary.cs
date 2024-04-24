using System;
using System.IO;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class SetSerializedLibrary : MonoBehaviour
{
    [Header("ARTrackedImageManager")]
    public ARTrackedImageManager arTrackedImageManager;
    [Space(10)]

    [Header("TMP INFO")]
    public TextMeshProUGUI text;

    public void SetAnchor()
    {
        string filePath = "";

#if UNITY_ANDROID
        filePath = Path.Combine(Application.persistentDataPath, "FBXLibrary/Anchor.fbx");
#else
        filePath = Path.Combine(Application.dataPath, "Resources/FBXLibrary/Anchor.fbx");
#endif

        bool fileExists = CheckFileExists(filePath);

        if (fileExists)
        {
            FieldInfo infos_m_TrackedImagePrefab = typeof(ARTrackedImageManager).GetField("m_TrackedImagePrefab", BindingFlags.NonPublic | BindingFlags.Instance);
            if (infos_m_TrackedImagePrefab != null)
            {
                infos_m_TrackedImagePrefab.SetValue(arTrackedImageManager, Convert.ChangeType(Resources.Load<GameObject>("FBXLibrary/Anchor"), infos_m_TrackedImagePrefab.FieldType));
                text.text = "File Active";
                Debug.Log("File Active");
            }
            else
            {
                text.text = "File not Active";
                Debug.Log("File not Active");
            }
        }
        else
        {
            text.text = "File not found";
            Debug.Log("File not found");
        }
    }

    private bool CheckFileExists(string filePath)
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            AndroidJavaObject file = new AndroidJavaObject("java.io.File", filePath);
            return file.Call<bool>("File already exists");
        }
        else
        {
            return File.Exists(filePath);
        }
    }
}
