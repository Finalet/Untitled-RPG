// Based off of work developed by Tom Kail at Inkle
// Released under the MIT Licence as held at https://opensource.org/licenses/MIT

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditorInternal;
using UnityEditor;
#endif

namespace MalbersAnimations
{
#if UNITY_EDITOR

	/// <summary>
	///  This class contains extension methods for the SerializedProperty
	///  class.  Specifically, methods for dealing with object arrays.
	/// <see cref="SerializedProperty"/> extensions.
	/// </summary>
	public static class SerializedPropertyExtensions
    {
        //// Use this to add an object to an object array represented by a SerializedProperty.
        //public static void AddToObjectArray<T>(this SerializedProperty arrayProperty, T elementToAdd)
        //    where T : UnityEngine.Object
        //{
        //    // If the SerializedProperty this is being called from is not an array, throw an exception.
        //    if (!arrayProperty.isArray)
        //        throw new UnityException("SerializedProperty " + arrayProperty.name + " is not an array.");

        //    // Pull all the information from the target of the serializedObject.
        //    arrayProperty.serializedObject.Update();

        //    // Add a null array element to the start of the array then populate it with the object parameter.
        //    arrayProperty.InsertArrayElementAtIndex(0);
        //    arrayProperty.GetArrayElementAtIndex(0).objectReferenceValue = elementToAdd;
          
        //    // Push all the information on the serializedObject back to the target.
        //    arrayProperty.serializedObject.ApplyModifiedProperties();
        //}

        //// Use this to remove the object at an index from an object array represented by a SerializedProperty.
        //public static void RemoveFromObjectArrayAt(this SerializedProperty arrayProperty, int index)
        //{
        //    // If the index is not appropriate or the serializedProperty this is being called from is not an array, throw an exception.
        //    if (index < 0)
        //        throw new UnityException("SerializedProperty " + arrayProperty.name + " cannot have negative elements removed.");

        //    if (!arrayProperty.isArray)
        //        throw new UnityException("SerializedProperty " + arrayProperty.name + " is not an array.");

        //    if (index > arrayProperty.arraySize - 1)
        //        throw new UnityException("SerializedProperty " + arrayProperty.name + " has only " + arrayProperty.arraySize + " elements so element " + index + " cannot be removed.");

        //    // Pull all the information from the target of the serializedObject.
        //    arrayProperty.serializedObject.Update();

        //    // If there is a non-null element at the index, null it.
        //    if (arrayProperty.GetArrayElementAtIndex(index).objectReferenceValue)
        //        arrayProperty.DeleteArrayElementAtIndex(index);

        //    // Delete the null element from the array at the index.
        //    arrayProperty.DeleteArrayElementAtIndex(index);

        //    // Push all the information on the serializedObject back to the target.
        //    arrayProperty.serializedObject.ApplyModifiedProperties();
        //}
    }
#endif
}