using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BendInteractor : MonoBehaviour
{
    void Update()
    {
        Shader.SetGlobalVector("_PositionMoving", transform.position);
    }
}