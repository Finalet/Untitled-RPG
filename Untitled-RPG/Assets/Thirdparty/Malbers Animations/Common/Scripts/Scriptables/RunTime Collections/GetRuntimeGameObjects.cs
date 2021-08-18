using MalbersAnimations.Events;
using UnityEngine;

namespace MalbersAnimations.Scriptables
{
    [AddComponentMenu("Malbers/Runtime Vars/Get Runtime GameObjects")]

    public class GetRuntimeGameObjects : MonoBehaviour
    {
        [RequiredField] public RuntimeGameObjects Collection;

        public FloatReference delay = new FloatReference();
        public enum RuntimeSetTypeGameObject {First, Random, Index, ByName , Closest }
        public RuntimeSetTypeGameObject type = RuntimeSetTypeGameObject.Random; 
        [Hide("showIndex",true,false)]
        public int Index = 0;
       
        [Hide("showName", true, false)]
        public string m_name;
        public GameObjectEvent Raise = new GameObjectEvent();

        public void SetCollection(RuntimeGameObjects col) => Collection = col;

        private void Start()
        {
            if (Collection != null && Collection.items != null && Collection.items.Count > 0)
            {
                if (delay > 0)
                    Invoke(nameof(GetSet), delay);
                else
                    GetSet();
            }
        }



        private void GetSet()
        {
            if (Collection != null)
            {
                switch (type)
                {
                    case RuntimeSetTypeGameObject.First:
                        Raise.Invoke(Collection.Item_GetFirst());
                        break;
                    case RuntimeSetTypeGameObject.Random:
                        Raise.Invoke(Collection.Item_GetRandom());
                        break;
                    case RuntimeSetTypeGameObject.Index:
                        Raise.Invoke(Collection.Item_Get(Index));
                        break;
                    case RuntimeSetTypeGameObject.ByName:
                        Raise.Invoke(Collection.Item_Get(m_name));
                        break;
                    case RuntimeSetTypeGameObject.Closest:
                        Raise.Invoke(Collection.Item_GetClosest(gameObject));
                        break;
                    default:
                        break;
                }
            }
        }

        [HideInInspector]  public bool showIndex;
      [HideInInspector]  public bool showName;
        private void OnValidate()
        {
            showIndex = type == RuntimeSetTypeGameObject.Index;
            showName = type == RuntimeSetTypeGameObject.ByName;   
        }
    }
}