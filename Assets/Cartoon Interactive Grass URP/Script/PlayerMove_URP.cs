using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace AppsTools.URP
{
    public class PlayerMove_URP : MonoBehaviour
    {
        public static PlayerMove_URP Instance;
        public ToonGrassPainter_URP painter1;
        public ToonGrassOperation_URP ope1;

        public float removalRadius = 2f;
        public Transform removalTransform;

        public Camera mainCam;
        public TextMeshProUGUI text;

        public LevelMeshes meshes;

        //TEST
        public Mesh currentMesh;

        private void Awake()
        {
            Instance = this;
            currentMesh = new Mesh();
            currentMesh = meshes.mesh;
        }

        void Update()
        {
            if (Input.GetMouseButton(0))
            {
                RaycastHit hit;
                Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
                Debug.DrawRay(this.transform.position, ray.direction);
                if (Physics.Raycast(ray, out hit, 100))
                {
                    removalTransform.transform.position = hit.point;

                    painter1.RemoveGrassAtPosition(removalTransform.position, removalRadius);

                    if (!painter1.AreVerticesEqual(painter1.mesh.vertexCount, painter1.verticesCount))
                    {
                        UpdateStuff();
                        painter1.verticesCount = painter1.mesh.vertexCount;
                    }
                }
            }
        }

        public void UpdateStuff()
        {
            Debug.Log("UPDATING MESH");
            ope1.UpdateStuff();
        }
    }
}