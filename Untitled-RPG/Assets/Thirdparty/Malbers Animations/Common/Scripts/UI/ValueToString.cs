using UnityEngine;
using MalbersAnimations.Events;

namespace MalbersAnimations.Utilities
{
    /// <summary>Converts a value to string </summary>
    [AddComponentMenu("Malbers/UI/Value to String")]
    public class ValueToString : MonoBehaviour
    {
        public int FloatDecimals = 2;
        public string Prefix;
        public string Suffix;

        public StringEvent toString = new StringEvent();

        public virtual void ToString(float value) => toString.Invoke(Prefix + value.ToString("F" + FloatDecimals) + Suffix);
        public virtual void ToString(int value) => toString.Invoke(Prefix + value.ToString() + Suffix);
        public virtual void ToString(bool value) => toString.Invoke(Prefix + value.ToString() + Suffix);
        public virtual void ToString(string value) => toString.Invoke(Prefix + value + Suffix);
        public virtual void SetPrefix(string value) => Prefix = value;
        public virtual void SetSufix(string value) => Suffix  = value;
        public virtual void ToString(UnityEngine.Object value) => toString.Invoke(Prefix + value.name.ToString() + Suffix);
        public virtual void ToString(Vector3 value) => toString.Invoke(Prefix + value.ToString() + Suffix);
        public virtual void ToString(Vector2 value) => toString.Invoke(Prefix + value.ToString() + Suffix);
    }
}