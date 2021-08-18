using UnityEngine;
using System.Collections;
using System;
using System.Reflection;
using MalbersAnimations;
using System.Collections.Generic;
using System.Linq;

public static class MalbersAnimationsExtensions
{

    public static void Resize<T>(this List<T> list, int size, T element = default(T))
    {
        int count = list.Count;

        if (size < count)
        {
            list.RemoveRange(size, count - size);
        }
        else if (size > count)
        {
            if (size > list.Capacity)   // Optimization
                list.Capacity = size;

            list.AddRange(Enumerable.Repeat(element, size - count));
        }
    }



    /// <summary> Find the first transform grandchild with this name inside this transform</summary>
    public static Transform FindGrandChild(this Transform aParent, string aName)
    {
        var result = aParent.ChildContainsName(aName);
        
        if (result != null)   return result;
         
        foreach (Transform child in aParent)
        {
            result = child.FindGrandChild(aName);
            if (result != null)
                return result;
        }
        return null;
    }

    public static Transform ChildContainsName(this Transform aParent, string aName)
    {
        foreach (Transform child in aParent)
        {
            if (child.name.Contains(aName))
                return child;
        }
        return null;
    }

    /// <summary>The GameObject is a prefab, Meaning in not in any scene</summary>
    public static bool IsPrefab(this GameObject go) => !go.scene.IsValid();

   

    public static T FindComponent<T>(this GameObject c) where T: Component
    {
        T Ttt = c.GetComponent<T>();
        if (Ttt != null) return Ttt;

        Ttt = c.GetComponentInParent<T>();
        if (Ttt != null) return Ttt;

        Ttt = c.GetComponentInChildren<T>();
        if (Ttt != null) return Ttt;

        return default;
    }

    public static T[] FindComponents<T>(this GameObject c) where T : Component
    {
        T[] Ttt = c.GetComponents<T>();
        if (Ttt != null) return Ttt;

        Ttt = c.GetComponentsInParent<T>();
        if (Ttt != null) return Ttt;

        Ttt = c.GetComponentsInChildren<T>();
        if (Ttt != null) return Ttt;

        return default;
    }



    /// <summary>Search for the Component in the root of the Object </summary>
    public static T FindComponentInRoot<T>(this GameObject c) where T : Component
    {
        var root = c.transform.root;
        T Ttt = root.GetComponent<T>();
        if (Ttt != null) return Ttt;

        Ttt = root.GetComponentInChildren<T>();
        if (Ttt != null) return Ttt;

        return default;
    }


    public static T FindInterface<T>(this GameObject c)
    {
        T Ttt = c.GetComponent<T>();
        if (Ttt != null) return Ttt;

        Ttt = c.GetComponentInParent<T>();
        if (Ttt != null) return Ttt;

        Ttt = c.GetComponentInChildren<T>();
        if (Ttt != null) return Ttt; 

        return default;
    } 

    /// <summary>Search for the Component in the hierarchy Up or Down</summary>
    public static T FindComponent<T>(this Component c) where T : Component => c.gameObject.FindComponent<T>();

    public static T FindInterface<T>(this Component c) => c.gameObject.FindInterface<T>();
     
   

    /// <summary>Search for the Component in the root of the Object </summary>
    public static T FindComponentInRoot<T>(this Component c) where T : Component => c.gameObject.FindComponentInRoot<T>();


    /// <summary>returns the delta position from a rotation.</summary>
    public static Vector3 DeltaPositionFromRotate(this Transform transform, Vector3 point, Vector3 axis, float deltaAngle)
    {
        var pos = transform.position;
        var direction = pos - point;
        var rotation = Quaternion.AngleAxis(deltaAngle, axis);
        direction = rotation * direction;

        pos = point + direction - pos;
        pos.y = 0;                                                      //the Y is handled by the Fix Position method

        return pos;
    }

    /// <summary>returns the delta position from a rotation.</summary>
    public static Vector3 DeltaPositionFromRotate(this Transform transform, Transform platform, Quaternion deltaRotation)
    {
        var pos = transform.position;

        var direction = pos - platform.position;
        var directionAfterRotation = deltaRotation * direction;

        var NewPoint = platform.position + directionAfterRotation;


        pos = NewPoint - transform.position;

        return pos;
    }


    /// <summary>
    /// Checks if a GameObject has been destroyed.
    /// </summary>
    /// <param name="gameObject">GameObject reference to check for destructedness</param>
    /// <returns>If the game object has been marked as destroyed by UnityEngine</returns>
    public static bool IsDestroyed(this GameObject gameObject)
    {
        // UnityEngine overloads the == opeator for the GameObject type
        // and returns null when the object has been destroyed, but 
        // actually the object is still there but has not been cleaned up yet
        // if we test both we can determine if the object has been destroyed.
        return gameObject == null && !ReferenceEquals(gameObject, null);
    }


    /// <summary>Resets the Local Position and rotation of a transform</summary>

    public static void ResetLocal(this Transform transform)
    {
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
    }

    /// <summary>Resets the Local Position and rotation of a transform</summary>
    public static void SetLocalTransform(this Transform transform, Vector3 LocalPos, Vector3 LocalRot, Vector3 localScale)
    {
        transform.localPosition = LocalPos;
        transform.localEulerAngles = LocalRot;
        transform.localScale = localScale;
    }

    /// <summary>Resets the Local Position and rotation of a transform</summary>
    public static void SetLocalTransform(this Transform transform, TransformOffset offset)
    {
        transform.localPosition = offset.Position;
        transform.localEulerAngles = offset.Rotation;
        transform.localScale = offset.Scale;
    }

    /// <summary> Invoke with Parameters </summary>
    public static bool InvokeWithParams(this MonoBehaviour sender, string method, object args)
    {
        Type argType = null;

        if (args != null) argType = args.GetType();
      

        MethodInfo methodPtr = null;

        if (argType != null)
        {
            methodPtr = sender.GetType().GetMethod(method, new Type[] { argType });
        }
        else
        {
            methodPtr = sender.GetType().GetMethod(method);
        }

        if (methodPtr != null)
        {
            if (args != null)
            {
                var arguments = new object[1] { args };
                methodPtr.Invoke(sender, arguments);
                return true;
            }
            else
            {
                methodPtr.Invoke(sender, null);
                return true;
            }
        }

        PropertyInfo property = sender.GetType().GetProperty(method);

        if (property != null)
        {
            property.SetValue(sender, args, null);
            return true;

        }
        return false;
    }


    /// <summary>Invoke with Parameters and Delay </summary>
    public static void InvokeDelay(this MonoBehaviour behaviour, string method, object options, YieldInstruction wait)
    {
        behaviour.StartCoroutine(_invoke(behaviour, method, wait, options));
    }

    private static IEnumerator _invoke(this MonoBehaviour behaviour, string method, YieldInstruction wait, object options)
    {
        yield return wait;

        Type instance = behaviour.GetType();
        MethodInfo mthd = instance.GetMethod(method);
        mthd.Invoke(behaviour, new object[] { options });

        yield return null;
    }


    /// <summary>Invoke with Parameters for Scriptable objects</summary>
    public static void Invoke(this ScriptableObject sender, string method, object args)
    {
        var methodPtr = sender.GetType().GetMethod(method);

        if (methodPtr != null)
        {
            if (args != null)
            {
                var arguments = new object[1] { args };
                methodPtr.Invoke(sender, arguments);
            }
            else
            {
                methodPtr.Invoke(sender, null);
            }
        }
    }


    public static void SetLayer(this GameObject parent, int layer, bool includeChildren = true)
    {
        parent.layer = layer;
        if (includeChildren)
        {
            foreach (Transform trans in parent.transform.GetComponentsInChildren<Transform>(true))
            {
                trans.gameObject.layer = layer;
            }
        }
    }


    /// --------------- EDITOR EXTENSIONS -------------------------

#if UNITY_EDITOR

#endif

}