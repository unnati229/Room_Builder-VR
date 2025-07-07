using UnityEngine;
using System.Collections.Generic;

public class MeshClass
{
    public Asset assettype;
    public bool isIlluminated;
    public PermissibleLocation permissibleLocation;
    public List<GameObject> prefabs;
    public List<GameObject> instantiatedObjects = new List<GameObject>(); 

    // Constructor
    public MeshClass(Asset assettype, bool isIlluminated, PermissibleLocation permissibleLocation)
    {
        this.assettype = assettype;
        this.isIlluminated = isIlluminated;
        this.permissibleLocation = permissibleLocation;
        this.prefabs = AssetManagerService.LoadMeshes(this.assettype);
    }
}
