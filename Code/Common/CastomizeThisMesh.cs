using UnityEngine;
using System.Collections;
using NibiruLibrary;

public class CastomizeThisMesh : MonoBehaviour
{
    [Range(1, 180)]
    public float angle;
    [Range(1, 100)]
    public float distance;

    public void MyStart()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        FragmentMeshCreator.Create(meshFilter, angle, distance, 2);
    }

    //private void OnValidate()
    //{
    //    MeshFilter meshFilter = GetComponent<MeshFilter>();
    //    FragmentMeshCreator.Create(meshFilter, angle, distance, 2);
    //}
}
