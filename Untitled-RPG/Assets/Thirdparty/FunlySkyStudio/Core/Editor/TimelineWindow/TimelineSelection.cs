using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Helper class for tracking selection context.
public abstract class TimelineSelection : System.Object
{
  public static bool isDraggingTimeline = false;
  public static string selectedGroupUUID;
  public static string selectedControlUUID;
  public static Vector2 startingMouseOffset;

  public static void Clear()
  {
    isDraggingTimeline = false;
    selectedGroupUUID = null;
    selectedControlUUID = null;
  }
}
