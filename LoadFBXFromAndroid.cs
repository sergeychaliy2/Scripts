using UnityEngine;
using UnityEngine.XR.ARSubsystems;
using System;
using System.IO;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.XR.ARFoundation;
using System.Collections;
using UnityEditor.XR.ARSubsystems;

public class LoadFBXFromAndroid : MonoBehaviour
{
    [Header("ImageLibrary")]
    public ARTrackedImageManager trackedImageManager;

    [Header("TMP")]
    [Space(10)]
    public TextMeshProUGUI selectedFileText;
    public TextMeshProUGUI selectedFileImage;
    public TextMeshProUGUI selectedFileFBX;

    [SerializeField]
    [Space(10)]
    private Texture2D imageTexture;

    [SerializeField]
    [Space(10)]
    private string imagePath;

    [SerializeField]
    [Space(10)]
    private string fbxPath;

    [SerializeField]
    [Space(10)]
    private XRReferenceImageLibrary m_SerializedLibrary;

    [SerializeField]
    [Space(10)]
    private string libraryPath;

    void Start()
    {
        m_SerializedLibrary = ScriptableObject.CreateInstance<XRReferenceImageLibrary>();
    }
    public void CreateNewLibrary()
    {
        if (m_SerializedLibrary != null)
        {
            string existingLibraryPath = AssetDatabase.GetAssetPath(m_SerializedLibrary);
            AssetDatabase.DeleteAsset(existingLibraryPath);
        }

        m_SerializedLibrary = ScriptableObject.CreateInstance<XRReferenceImageLibrary>();

        string folderPath = Path.Combine(Application.dataPath, "ImageLibrary");
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string libraryPath = Path.Combine("Assets", "Resources", "ImageLibrary", "ReferenceImageLibrary.asset");

        if (File.Exists(libraryPath))
        {
            File.Delete(libraryPath);
        }

        AssetDatabase.CreateAsset(m_SerializedLibrary, libraryPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        AddImageToLibrary();
    }


    public void OpenImageExplorer()
    {
        AndroidJavaObject context = GetUnityActivity();
        AndroidJavaObject intent = new AndroidJavaObject("android.content.Intent", "android.intent.action.OPEN_DOCUMENT");
        intent.Call<AndroidJavaObject>("addCategory", "android.intent.category.OPENABLE");
        intent.Call<AndroidJavaObject>("setType", "image/*");
        context.Call("startActivityForResult", intent, 0);
        StartCoroutine(LoadImage());

    }

    public void OpenFBXExplorer()
    {
        AndroidJavaObject context = GetUnityActivity();
        AndroidJavaObject intent = new AndroidJavaObject("android.content.Intent", "android.intent.action.OPEN_DOCUMENT");
        intent.Call<AndroidJavaObject>("addCategory", "android.intent.category.OPENABLE");
        intent.Call<AndroidJavaObject>("setType", "model/*");
        context.Call("startActivityForResult", intent, 0);
        StartCoroutine(LoadFBX());
    }

    private AndroidJavaObject GetUnityActivity()
    {
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        return unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
    }

    public void AddImageToLibrary()
    {
        if (imageTexture != null)
        {
            XRReferenceImage referenceImage = new XRReferenceImage(SerializableGuid.empty, SerializableGuid.empty, null, imageTexture.name, imageTexture);

            if (m_SerializedLibrary == null)
            {
                CreateNewLibrary();
            }
            m_SerializedLibrary.Add();

            int index = m_SerializedLibrary.count - 1;

            m_SerializedLibrary.SetTexture(index, imageTexture, true);
            m_SerializedLibrary.SetName(index, imageTexture.name);

            imageTexture = null;

            selectedFileText.text = "Image added to library: " + referenceImage.name;
            selectedFileImage.text = "Selected Image: " + referenceImage.name;
            if (trackedImageManager != null)
            {
                trackedImageManager.referenceLibrary = m_SerializedLibrary;
            }
            else
            {
                Debug.LogWarning("ARTrackedImageManager reference is not set!");
            }
        }
        else
        {
            Debug.LogWarning("No image selected!");
        }
    }


    public void OnActivityResult(string result)
    {
        string extension = Path.GetExtension(result).ToLower();
        if (extension == ".png" || extension == ".jpg" || extension == ".jpeg")
        {
        }
        else if (extension == ".fbx")
        {
            StartCoroutine(LoadFBX());
        }
        else
        {
            selectedFileText.text = "Unsupported file type: " + extension;
        }
    }

    IEnumerator LoadImage()
    {
        WWW www = new WWW("file://" + imagePath);
        yield return www;

        if (string.IsNullOrEmpty(www.error))
        {
            Texture2D texture = www.texture;

            XRReferenceImage referenceImage = new XRReferenceImage(SerializableGuid.empty, SerializableGuid.empty, null, texture.name, texture);

            if (m_SerializedLibrary == null)
            {
                CreateNewLibrary();
            }

            m_SerializedLibrary.Add();
            int index = m_SerializedLibrary.count - 1;
            m_SerializedLibrary.SetTexture(index, texture, true);
            m_SerializedLibrary.SetName(index, texture.name);

            selectedFileText.text = "Image added to library: " + referenceImage.name;
            selectedFileImage.text = "Selected Image: " + referenceImage.name;

            if (trackedImageManager != null)
            {
                trackedImageManager.referenceLibrary = m_SerializedLibrary;
            }
            else
            {
                Debug.LogWarning("ARTrackedImageManager reference is not set!");
            }
        }
        else
        {
            selectedFileText.text = "Error loading image file: " + www.error;
        }
    }

    IEnumerator LoadFBX()
    {
        WWW www = new WWW("file://" + fbxPath);
        yield return www;

        if (string.IsNullOrEmpty(www.error))
        {
            byte[] bytes = www.bytes;

            string directoryPath = Application.persistentDataPath + "/Resources/";
            string fileName = Path.GetFileName(fbxPath);
            string filePath = Path.Combine(directoryPath, fileName);

            File.WriteAllBytes(filePath, bytes);
            selectedFileText.text = "FBX file saved to: " + filePath;
            selectedFileFBX.text = "Selected FBX: " + fileName;

            GameObject loadedObject = Resources.Load<GameObject>(fileName);

            if (loadedObject != null)
            {
                loadedObject.name = "Anchor";
                loadedObject.tag = "Anchor";
            }
            else
            {
                Debug.LogError("Failed to load object from resources: " + fileName);
            }
        }
        else
        {
            selectedFileText.text = "Error loading FBX file: " + www.error;
        }
    }


    private void AddReferenceImageToLibrary(string name, string filePath)
    {
        byte[] fileData = File.ReadAllBytes(filePath);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(fileData);

        XRReferenceImage referenceImage = new XRReferenceImage(SerializableGuid.empty, SerializableGuid.empty, null, name, texture);

        int index = m_SerializedLibrary.count; 
        m_SerializedLibrary.Add(); 
        m_SerializedLibrary.SetName(index, name); 
        m_SerializedLibrary.SetTexture(index, texture, true);
    }


    void OnDestroy()
    {
        if (m_SerializedLibrary != null)
        {
            string libraryPath = AssetDatabase.GetAssetPath(m_SerializedLibrary);
            AssetDatabase.DeleteAsset(libraryPath);
        }
    }
}

   
