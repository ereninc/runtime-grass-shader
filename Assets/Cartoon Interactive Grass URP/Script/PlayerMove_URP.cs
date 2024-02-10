using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

        public LevelMeshes meshes;

        public Image image;

        //TEST
        public Mesh currentMesh;

        private RaycastHit _hit;
        private Ray _ray;
        
        
        public Color renk1;
        public Color renk2;
        public Color renk3;

        public ParticleSystem particleSystem;


        private Color MixColors(Color renkA, Color renkB, Color renkC)
        {
            float karisikRed = (renkA.r + renkB.r + renkC.r) / 3f;
            float karisikGreen = (renkA.g + renkB.g + renkC.g) / 3f;
            float karisikBlue = (renkA.b + renkB.b + renkC.b) / 3f;

            return new Color(karisikRed, karisikGreen, karisikBlue);
        }

        private void Awake()
        {
            Instance = this;
            currentMesh = new Mesh();
            currentMesh = meshes.mesh;

            psRend = particleSystem.GetComponent<ParticleSystemRenderer>();
        }

        private Color currentCol;
        private ParticleSystemRenderer psRend;
        void Update()
        {
            if (Input.GetMouseButton(0))
            {
                _ray = mainCam.ScreenPointToRay(Input.mousePosition);
                
                if (Physics.Raycast(_ray, out _hit, 100))
                {
                    removalTransform.transform.position = _hit.point;

                    painter1.RemoveGrassAtPosition(removalTransform.position, removalRadius);

                    if (!painter1.AreVerticesEqual(painter1.mesh.vertexCount, painter1.verticesCount))
                    {
                        UpdateStuff();
                        painter1.verticesCount = painter1.mesh.vertexCount;

                        //Particles
                        currentCol = MixColors(painter1.currentCutColor, renk2, renk3);
                        psRend.sharedMaterial.color = currentCol;
                        particleSystem.Play();
                    }
                }
            }
        }

        private void UpdateStuff()
        {
            ope1.UpdateGrasses();
        }
    }
}