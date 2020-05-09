using UnityEngine;
using UnityEngine.EventSystems;
using MalbersAnimations.Events;
using UnityEngine.AI;
using MalbersAnimations.Utilities;

namespace MalbersAnimations.Controller
{
    public class PointClick : MonoBehaviour
    {
        public PointClickData pointClickData;
        public GameObject PointUI;
        public float radius = 0.2f;
        private const float navMeshSampleDistance = 4f;

        [Header("Events")]
        public Vector3Event OnPointClick = new Vector3Event();
        public TransformEvent OnInteractableClick = new TransformEvent();

        protected Collider[] interactables;

        void OnEnable()
        {
            if (pointClickData)
            {
                pointClickData.baseDataEvent.AddListener(OnGroundClick);
            }
        }


        void OnDisable()
        {
            if (pointClickData)
            {
                pointClickData.baseDataEvent.RemoveListener(OnGroundClick);
            }
        }

        Vector3 destinationPosition;

        public void OnGroundClick(BaseEventData data)   
        {
            PointerEventData pData = (PointerEventData)data;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(pData.pointerCurrentRaycast.worldPosition, out hit, navMeshSampleDistance, NavMesh.AllAreas))
                destinationPosition = hit.position;
            else
                destinationPosition = pData.pointerCurrentRaycast.worldPosition;

           

            interactables = Physics.OverlapSphere(destinationPosition, radius);

            foreach (var inter in interactables)
            {
                if (inter.GetComponent<IDestination>() != null)
                {
                    OnInteractableClick.Invoke(inter.transform.root); //Invoke only the first interactable found

                    if (PointUI)
                        Instantiate(PointUI, inter.transform.position, Quaternion.FromToRotation(PointUI.transform.up, pData.pointerCurrentRaycast.worldNormal));

                    return;
                }
            }

            if (PointUI)
                Instantiate(PointUI, destinationPosition, Quaternion.FromToRotation(PointUI.transform.up, pData.pointerCurrentRaycast.worldNormal));

            OnPointClick.Invoke(destinationPosition);
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (Application.isPlaying)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(destinationPosition, 0.1f);
                Gizmos.DrawSphere(destinationPosition, 0.1f);
            }
        }

        private void Reset()
        {
            pointClickData = MalbersTools.GetInstance<PointClickData>("PointClickData");
            PointUI = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Malbers Animations/Common/Prefabs/Interactables/ClickPoint.prefab");

            var aiControl = GetComponent<IAIControl>();


            if (aiControl != null)
            {
                UnityEditor.Events.UnityEventTools.AddPersistentListener(OnPointClick, aiControl.SetDestination);
                UnityEditor.Events.UnityEventTools.AddPersistentListener(OnInteractableClick, aiControl.SetTarget);
            }
        }

#endif
    }
}