using UnityEngine;
using System.Collections.Generic;

public class AssetManagerService : MonoBehaviour
{
    public static List<GameObject> LoadMeshes(Asset asset )
    {
        string[] parts = asset.assetType.ToString().Split('.');
        string path = parts[parts.Length - 1];


        List<GameObject> meshes=new List<GameObject>();
        meshes.AddRange(Resources.LoadAll<GameObject>(path));
        return meshes;
    }
}
