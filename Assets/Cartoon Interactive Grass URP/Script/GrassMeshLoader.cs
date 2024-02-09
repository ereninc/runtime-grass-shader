using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;


#if UNITY_EDITOR
public class GrassMeshLoader : MonoBehaviour
{
    public ToonGrassOperation_URP toonGrass;

    public void SaveMesh()
    {
        var mesh = toonGrass.GetMesh();
        if (mesh == null) return;
        MeshSaverEditor.SaveMesh(mesh, "Mesh_2", true, true);
    }
}

[CustomEditor(typeof(GrassMeshLoader))]
class DecalMeshHelperEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GrassMeshLoader targetObject = (GrassMeshLoader)target;

        if (GUILayout.Button("Test"))
        {
            targetObject.SaveMesh();
        }
    }
}
#endif