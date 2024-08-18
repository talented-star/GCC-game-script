#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using System.IO;
using UnityEditor;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.Rendering;
using UnityEditor.VersionControl;
using Cysharp.Threading.Tasks;
using UnityEditor.Experimental.GraphView;
using UnityEditor.SceneManagement;
using Org.BouncyCastle.Asn1.Pkcs;
using GrabCoin.UI.HUD;

public class MinimapBaker : MonoBehaviour
{
    public Camera bakeCamera;
    public Transform center;
    public Transform top;
    public Color baseColor = Color.white;
    public Color shadowColor = Color.white;
    public Vector2Int bakeResolution = Vector2Int.one * 4096;

    private bool previewing => bakeCamera.gameObject.activeSelf;

    private Camera activeCamera;
    private string folder;

    [Button("Enable Preview", EButtonEnableMode.Editor)]
    public void EnableEditing()
    {
        activeCamera = Camera.allCameras.Length > 0 && Camera.allCameras[0] != bakeCamera ? Camera.allCameras[0] : null;
        bakeCamera.gameObject.SetActive(true);
        Camera.SetupCurrent(bakeCamera);
    }

    [ShowIf("previewing")]
    [Button("Disable Preview", EButtonEnableMode.Editor)]
    public void DisableEditing()
    {
        bakeCamera.gameObject.SetActive(false);
        Camera.SetupCurrent(activeCamera);
        activeCamera = null;
    }

    [Button("Select Folder", EButtonEnableMode.Editor)]
    public void SelectFolder()
    {
        folder = EditorUtility.OpenFolderPanel("Open Folder", "Assets\\Minimaps", "");
    }

    [Button("Select Minimap Data", EButtonEnableMode.Editor)]
    public void SelectMinimapData()
    {
        if(TryGetComponent(out MinimapPoser poser))
        {
            string dataPath = EditorUtility.OpenFilePanel("Open Asset", "Assets\\Minimaps", "asset" );

            if (dataPath.StartsWith(Application.dataPath))
            {
                dataPath = "Assets" + dataPath.Substring(Application.dataPath.Length);
            }

            var mapData = (MinimapData)AssetDatabase.LoadAssetAtPath(dataPath, typeof(MinimapData));
                
            poser.SetMinimapData(mapData);
            poser.ApplyPosition();

            bakeCamera.orthographicSize = Mathf.Abs(mapData.top.y);

            EditorUtility.SetDirty(mapData);
            EditorUtility.SetDirty(poser);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }
    }

    [Button("Apply Position", EButtonEnableMode.Editor)]
    public void ApplyPosition()
    {
        top.transform.position = center.transform.position + bakeCamera.orthographicSize * Vector3.forward;
    }

    [Button("Bake", EButtonEnableMode.Editor)]
    public void Bake()
    {
        //SETUP RENDER TEXTURE

        ApplyPosition();
        
        RenderTexture bakeTexture = new RenderTexture(bakeResolution.x, bakeResolution.y, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
       
        bakeTexture.Release();
        bakeTexture.enableRandomWrite = false;
        bakeTexture.filterMode = FilterMode.Bilinear;
        bakeTexture.antiAliasing = 8;
        bakeTexture.ResolveAntiAliasedSurface();
        bakeTexture.Create();


        //SETUP RENDERER        
        Color baseAmbientLight = RenderSettings.ambientLight;
        RenderSettings.ambientLight = baseColor;

        float baseShadowDistance = QualitySettings.shadowDistance;
        QualitySettings.shadowDistance = 0.1f;

        AmbientMode baseAmbientMode = RenderSettings.ambientMode;
        RenderSettings.ambientMode = AmbientMode.Flat;

        Color baseShadowColor = RenderSettings.subtractiveShadowColor;
        RenderSettings.subtractiveShadowColor = shadowColor;

        //RENDER IMAGE
        bakeCamera.targetTexture = bakeTexture;
        RenderTexture.active = bakeTexture;
        bakeCamera.Render();

        //SAVING TEXTURE

        Texture2D imageOverview = new Texture2D(bakeResolution.x, bakeResolution.y, TextureFormat.ARGB32, false, true);

        imageOverview.ReadPixels(new Rect(0, 0, bakeResolution.x, bakeResolution.y), 0, 0);
        imageOverview.Apply();


        bakeCamera.targetTexture = null;
        SaveTextureToFile(imageOverview);

        //CLEANING UP

        DestroyImmediate(imageOverview, true);
        DestroyImmediate(bakeTexture, true);

        //RETURNNG DEFAULT PROPERTIES

        RenderSettings.subtractiveShadowColor = baseShadowColor;
        
        RenderSettings.ambientMode = baseAmbientMode;
        
        QualitySettings.shadowDistance = baseShadowDistance;

        RenderSettings.ambientLight = baseAmbientLight;
    }

    public async void SaveTextureToFile(Texture2D texture)
    {
        if (string.IsNullOrEmpty(folder) || folder.Length <= 0)
        {
            SelectFolder();
        }
        var bytes = texture.EncodeToPNG();

        var path = Path.Combine(folder, "BakedMinimap" + SceneManager.GetActiveScene().name + ".png");

        await File.WriteAllBytesAsync(path, bytes);

        Debug.Log("File saved to " + path);

        if (TryGetComponent(out MinimapPoser poser))
        {
            string relativePath = path;
            if (relativePath.StartsWith(Application.dataPath))
            {
                relativePath = "Assets" + relativePath.Substring(Application.dataPath.Length);
            }

            TextureImporter importer = AssetImporter.GetAtPath(relativePath) as TextureImporter;

            importer.textureType = TextureImporterType.Sprite;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.maxTextureSize = Mathf.Max(bakeResolution.x, bakeResolution.y);
            importer.alphaSource = TextureImporterAlphaSource.None;
            importer.SaveAndReimport();

            AssetDatabase.WriteImportSettingsIfDirty(relativePath);

            AssetDatabase.ImportAsset(relativePath, ImportAssetOptions.ForceSynchronousImport);
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

            Debug.Log(relativePath);

            var sprite = (Sprite)AssetDatabase.LoadAssetAtPath(relativePath, typeof(Sprite));

            var mapData = new MinimapData();
            mapData.map = sprite;
            mapData.top = GetTop();
            mapData.center = GetCenter();

            var dataPath = Path.Combine(folder, "MinimapData" + SceneManager.GetActiveScene().name + ".asset");

            if (dataPath.StartsWith(Application.dataPath))
            {
                dataPath = "Assets" + dataPath.Substring(Application.dataPath.Length);
            }
            AssetDatabase.CreateAsset(mapData, dataPath);

            mapData = (MinimapData)AssetDatabase.LoadAssetAtPath(dataPath, typeof(MinimapData));

            poser.SetMinimapData(mapData);

            EditorUtility.SetDirty(mapData);
            EditorUtility.SetDirty(poser);

            AssetDatabase.SaveAssetIfDirty(mapData);
            AssetDatabase.SaveAssetIfDirty(poser);

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(
            bakeCamera.transform.position + bakeCamera.transform.forward + bakeCamera.orthographicSize * bakeCamera.transform.up + transform.right * bakeCamera.orthographicSize*GetAspectRatio(bakeResolution.x, bakeResolution.y),
            bakeCamera.transform.position + bakeCamera.transform.forward - bakeCamera.orthographicSize * bakeCamera.transform.up + transform.right * bakeCamera.orthographicSize * GetAspectRatio(bakeResolution.x, bakeResolution.y));
        
        Gizmos.DrawLine(
            bakeCamera.transform.position + bakeCamera.transform.forward + bakeCamera.orthographicSize * bakeCamera.transform.up - transform.right * bakeCamera.orthographicSize*GetAspectRatio(bakeResolution.x, bakeResolution.y),
            bakeCamera.transform.position + bakeCamera.transform.forward - bakeCamera.orthographicSize * bakeCamera.transform.up - transform.right * bakeCamera.orthographicSize * GetAspectRatio(bakeResolution.x, bakeResolution.y));
    }

    public Vector2 GetTop()
    {
        return new Vector2(top.position.x, top.position.z);
    }
    public Vector2 GetCenter()
    {
        return new Vector2(center.position.x, center.position.z);
    }

    private float GetAspectRatio(float x, float y)
    {
        return x / y;
    }
}
#endif