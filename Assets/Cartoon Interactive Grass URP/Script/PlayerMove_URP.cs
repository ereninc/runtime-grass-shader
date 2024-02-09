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
        public TextMeshProUGUI log;

        //TEST
        private void Awake()
        {
            Instance = this;
        }

        private void OnEnable()
        {
            painter1.OnMeshUpdate += UpdateStuff;
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
                }
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                UpdateStuff();
                log.text = "pressed";
            }

            if (Input.GetKeyUp(KeyCode.Space))
            {
                log.text = "";
            }

            //TEST UPDATE
            if (Input.GetKeyDown(KeyCode.S))
            {
                ope1.enabled = false;
                ope1.enabled = true;
            }
            
        }

        public void UpdateStuff()
        {
            StartCoroutine(delayedUpdate());
        }

        //TEST DELAY???
        private IEnumerator delayedUpdate()
        {
            yield return new WaitForSeconds(0.1f);
            ope1.UpdateStuff();
        }
    }
}