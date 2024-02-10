using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Test : MonoBehaviour
{
    [SerializeField] private MeshRenderer rend;
    public Color topTint, bottomTint;
    public float ambientStrength;

    public void SetColors()
    {
        rend.sharedMaterial.SetColor("_TopTint", topTint);
        rend.sharedMaterial.SetColor("_BottomTint", bottomTint);
        rend.sharedMaterial.SetFloat("_AmbientStrength", ambientStrength);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(Test))]
class TestEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        Test targetObject = (Test)target;

        if (GUILayout.Button("Test"))
        {
            targetObject.SetColors();
        }
    }
}
#endif