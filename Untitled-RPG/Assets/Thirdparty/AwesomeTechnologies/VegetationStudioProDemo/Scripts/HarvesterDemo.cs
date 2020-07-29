using System.Collections;
using System.Collections.Generic;
using AwesomeTechnologies.Vegetation;
using AwesomeTechnologies.Vegetation.Masks;
using UnityEngine;

namespace AwesomeTechnologies.Demo
{
    public class HarvesterDemo : MonoBehaviour
    {
        public Texture2D AxeCursor;
        public bool ChangeCursor = false;
        public GameObject SpawnEffect;

        void Update()
        {
            if (ChangeCursor) SetHarvestCursor();
            if (Input.GetMouseButtonDown(0))
            {
                RaycastForHarvestObject();
            }
        }

        void SetHarvestCursor()
        {
            if (!Camera.main) return;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit raycastHit;
            if (Physics.Raycast(ray, out raycastHit))
            {
                GameObject hitObject = raycastHit.collider.gameObject;
                VegetationItemInstanceInfo vegetationItemInstanceInfo =
                    hitObject.GetComponent<VegetationItemInstanceInfo>();

                if (vegetationItemInstanceInfo)
                {
                    Cursor.SetCursor(AxeCursor, new UnityEngine.Vector2(0, 0), CursorMode.Auto);
                }
                else
                {
                    Cursor.SetCursor(null, new UnityEngine.Vector2(0, 0), CursorMode.Auto);
                }
            }
        }

        void RaycastForHarvestObject()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit raycastHit;
            if (Physics.Raycast(ray, out raycastHit))
            {
                GameObject hitObject = raycastHit.collider.gameObject;
                VegetationItemInstanceInfo vegetationItemInstanceInfo = hitObject.GetComponent<VegetationItemInstanceInfo>();
                if (!vegetationItemInstanceInfo) return;

                //We have detected a collider spawned from the collider system. Here we can add more logic to confirm that this is a object we can harvest. In this example we accept all types as harvestable

                //Now we create a new GameObject with the VegetationItemMask component. This will mask out the selected VegetationItem. If you want this item to respawn include a script with logic that will remove it when time has passed.
                GameObject vegetationItemMaskObject = new GameObject {name = "VegetationItemMask - " + hitObject.name};
                vegetationItemMaskObject.transform.position = vegetationItemInstanceInfo.Position;
                vegetationItemMaskObject.AddComponent<VegetationItemMask>().SetVegetationItemInstanceInfo(vegetationItemInstanceInfo);

                //Spawning an particle system for effect
                SpawnHarvestEffect(vegetationItemInstanceInfo.Position);
            }
        }

        void SpawnHarvestEffect(Vector3 position)
        {
            if (!SpawnEffect) return;
            GameObject effect = Instantiate(SpawnEffect, position, Quaternion.identity);
            StartCoroutine(DestroyEffect(effect, 1.5f));
        }

        IEnumerator DestroyEffect(GameObject go, float time)
        {
            yield return new WaitForSeconds(time);
            Destroy(go);
        }
    }
}

