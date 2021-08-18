using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace MalbersAnimations.Controller
{
    public class PointClickData : ScriptableObject
    {
        public Action<BaseEventData> baseDataPointerClick;
        public Action<BaseEventData> baseDataPointerDrag;
        public void Invoke(BaseEventData data) => baseDataPointerClick(data);
    }
}