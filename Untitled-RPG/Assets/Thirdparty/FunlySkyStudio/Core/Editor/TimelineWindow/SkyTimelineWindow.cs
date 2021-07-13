using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Funly.SkyStudio
{
  // The timeline window is an animation editor for SkyProfiles.
  public class SkyTimelineWindow : EditorWindow
  {
    private const float TIME_HEADER_HEIGHT = 50.0f;
    private const float CONTENT_INSET = 20.0f;
    private const float PLAYHEAD_WIDTH = 32.0f;
    private const float CURSOR_LINE_WIDTH = 8.0f;
    private const float NAME_COLUMN_WIDTH = 225.0f;
    private const float COLOR_ROW_HEIGHT = 30.0f;
    private const float SPHERE_POINT_ROW_HEIGHT = 30.0f;
    private const float NUMBER_ROW_HEIGHT = 45.0f;
    private const float ROW_PADDING = 22.0f;
    private const float MIN_WINDOW_WIDTH = 580.0f;
    private const float MIN_WINDOW_HEIGHT = 250.0f;
    private const float VALUE_COLUMN_INSET = 1.0f;
    private const float HORIZONTAL_DIVIDER_HEIGHT = 2.0f;
    private const float VERTICAL_DIVIDER_WIDTH = 2.0f;
    private const float MAX_TIME_VALUE = .9999f;
    private const float EMPTY_MESSAGE_Y_OFFSET = 50.0f;
    private const float EMPTY_ADD_BUTTON_Y_OFFSET = 20.0f;
    private const float ICON_BUTTON_SIZE = 20.0f;
    private const float LEFT_INSET = 5.0f;

    private Vector2 m_ScrollPosition = Vector2.zero;
    private Texture2D m_PlayheadTexture;
    private List<ProfileGroupDefinition> m_TimelineDefinitions;
    private SkyProfile m_ActiveSkyProfile;
    private TimeOfDayController m_ActiveTimeController;
    private GUIStyle m_ButtonStyle;
    private GUIStyle m_GroupTitleStyle;
    private GUIStyle m_EmptyTitleStyle;
    private GUIStyle m_TimeLabelStyle;

    private const int MAX_DEBUG_POINTS = 100;
    private Vector4[] m_DebugPoints = new Vector4[MAX_DEBUG_POINTS];

    [MenuItem("Window/Sky Studio/Sky Timeline")]
    public static void ShowWindow()
    {
      TimelineSelection.Clear();

      SkyTimelineWindow window = EditorWindow.GetWindow<SkyTimelineWindow>();
      window.ConfigureWindow();
      window.minSize = new Vector2(MIN_WINDOW_WIDTH, MIN_WINDOW_HEIGHT);
      window.Show();
    }

    public void ConfigureWindow() {
			this.name = "Sky Timeline";
			this.titleContent = new GUIContent("Sky Timeline");
			this.wantsMouseMove = true;
			this.wantsMouseEnterLeaveWindow = true;
    }

    private void OnEnable()
    {
      ConfigureWindow();
    }

    private void OnDisable()
    {
      HideDebugPoints();
    }

    private void OnDestroy()
    {
      TimelineSelection.Clear();
    }

    private void OnInspectorUpdate()
    {
      Repaint();
    }

    private void OnGUI()
    {
      LoadStyles();

      TimeOfDayController timeController = FindObjectOfType<TimeOfDayController>() as TimeOfDayController;

      // Render a setup helper UI.
      if (timeController == null) {
        RenderNeedsSkySetupLayout();
        return;
      }

      // Render a profile help message UI.
      if (timeController.skyProfile == null) {
        RenderNeedsProfileLayout();
        return;
      }

      m_ActiveTimeController = timeController;
      m_ActiveSkyProfile = timeController ? timeController.skyProfile : null;

      RebuildTimelineDefinitions(timeController.skyProfile);
      float contentHeight = CalculateWindowContentHeight(timeController.skyProfile);
      float scrollbarInset = 0;

      // Select the first colorGroup if one isn't selected.
      if (TimelineSelection.selectedGroupUUID == null &&
          timeController.skyProfile.timelineManagedKeys.Count > 0) {
        IKeyframeGroup group = timeController.skyProfile.GetGroup(timeController.skyProfile.timelineManagedKeys[0]);
        if (group != null) {
          TimelineSelection.selectedGroupUUID = group.id;
        }
      }

      // Inset content on the right to make room for scroll bar.
      if (contentHeight > position.height) {
        scrollbarInset = CONTENT_INSET;
      }

			// Timeline rect.
			Rect contentRect = new Rect(
        0,
        0,
        position.width - scrollbarInset,
			  position.height);

			// Check if mouse left the window, and cancel drag operations.
			if (Event.current.type == EventType.MouseLeaveWindow ||
          contentRect.Contains(Event.current.mousePosition) == false) {
				SkyEditorUtility.CancelTimelineDrags();
			}

			// Loads the list of timeline groups to render.
      RenderTimelineEditor(contentRect, timeController, contentHeight);

      // Save the edits to the profile object.
      if (timeController != null) {
        EditorUtility.SetDirty(timeController.skyProfile);

        // Keep the scene view rendering in sync for live editing.
        timeController.UpdateSkyForCurrentTime();
      }
    }

    private void LoadStyles() {
      m_ButtonStyle = new GUIStyle(GUI.skin.button);
      m_ButtonStyle.padding = new RectOffset(2, 2, 2, 2);

      m_GroupTitleStyle = new GUIStyle(GUI.skin.label);
      m_GroupTitleStyle.normal.textColor = Color.white;

      m_EmptyTitleStyle = new GUIStyle(GUI.skin.label);
      m_EmptyTitleStyle.normal.textColor = Color.white;

      m_TimeLabelStyle = new GUIStyle();
      m_TimeLabelStyle.fontStyle = FontStyle.Normal;
      m_TimeLabelStyle.fontSize = 24;
      m_TimeLabelStyle.normal.textColor = GUI.color;
    }

    private float CalculateWindowContentHeight(SkyProfile profile)
    {
      if (profile == null)
      {
        return 0;
      }

      int colorRowCount = 0;
      int numericRowCount = 0;
      int spherePointRowCount = 0;

      foreach (ProfileGroupDefinition groupInfo in m_TimelineDefinitions) {
        if (profile.IsManagedByTimeline(groupInfo.propertyKey) == false) {
          continue;
        }

        if (groupInfo.type == ProfileGroupDefinition.GroupType.Number) {
          numericRowCount += 1;
        } else if (groupInfo.type == ProfileGroupDefinition.GroupType.Color) {
          colorRowCount += 1;
        } else if (groupInfo.type == ProfileGroupDefinition.GroupType.SpherePoint)
        {
          spherePointRowCount += 1;
        }
      }

      float colorsHeight = colorRowCount * (COLOR_ROW_HEIGHT + ROW_PADDING);
      float numbersHeight = numericRowCount * (NUMBER_ROW_HEIGHT + ROW_PADDING);
      float spherePointHeight = spherePointRowCount * (SPHERE_POINT_ROW_HEIGHT + ROW_PADDING);
      float contentHeight =  colorsHeight + numbersHeight + spherePointHeight + TIME_HEADER_HEIGHT;

      return contentHeight;
    }

    private void RenderTimelineEditor(Rect rect, TimeOfDayController timeController, float contentHeight)
    {
      float nameColMinX = rect.x;
      float valueColMinX = nameColMinX + NAME_COLUMN_WIDTH;
      float valueColMaxX = rect.xMax;
      float valueColWidth = rect.width - NAME_COLUMN_WIDTH;

      // Check for the end of a timeline drag.
      if (TimelineSelection.isDraggingTimeline && Event.current.type == EventType.MouseUp) {
        TimelineSelection.isDraggingTimeline = false;
      }

      // If we're busy dragging the timeline, consume the events so child views don't see them.
      if (Event.current.type == EventType.MouseDrag && TimelineSelection.isDraggingTimeline)
      {
        Event.current.Use();
      }

      // Check if user dragged in the time ruler so we don't click things behind it.
      if (DidDragTimeRuler()) {
        SkyEditorUtility.CancelTimelineDrags();
        TimelineSelection.isDraggingTimeline = true;
        Event.current.Use();
      }

      float fullContentHeight = Mathf.Max(position.height, contentHeight);

      // Background style.
      RenderBackground();

      // Render timeline buttons at header.
      Rect toolbarRect = new Rect(nameColMinX,
        rect.y, NAME_COLUMN_WIDTH, TIME_HEADER_HEIGHT);
      RenderHeaderButtons(toolbarRect, timeController);

      // Render Scrubber.
      Rect timeScrubberRect = new Rect(valueColMinX, rect.y,
        valueColWidth - VALUE_COLUMN_INSET, TIME_HEADER_HEIGHT);
      RenderTimeRuler(timeScrubberRect, timeController.timeOfDay);

      // Show an empty content help message.
      if (timeController.skyProfile.timelineManagedKeys.Count == 0) {
        RenderEmptyTimelineMessage();
      }

      Rect innerScrollViewContent = new Rect(
        0,
        0,
        rect.width,
        contentHeight - TIME_HEADER_HEIGHT);

      Rect scrollWindowPosition = new Rect(0, TIME_HEADER_HEIGHT, position.width, rect.height - TIME_HEADER_HEIGHT);
      m_ScrollPosition = GUI.BeginScrollView(scrollWindowPosition, m_ScrollPosition, innerScrollViewContent, false, false);

      // Render all the content rows.
      Rect rowsRect = new Rect(rect.x, 0, rect.width, 0);
      RenderAllRows(rowsRect, timeController.skyProfile);
      RenderSpherePointGroupDebugPointsIfSelected();

      GUI.EndScrollView();

      // Draw the cursor, which overlaps the other content.
      Rect cursorRect = new Rect(
        valueColMinX, rect.y, valueColWidth - VALUE_COLUMN_INSET, fullContentHeight);
      RenderTimelineCursor(cursorRect, timeController);
    }

    // If sphere point group is selected, we render dots in the skybox to help user position things.
    private void RenderSpherePointGroupDebugPointsIfSelected()
    {
      if (m_ActiveSkyProfile == null || TimelineSelection.selectedGroupUUID == null)
      {
        return;
      }

      IKeyframeGroup group = m_ActiveSkyProfile.GetGroupWithId(TimelineSelection.selectedGroupUUID);
      if (group is SpherePointKeyframeGroup)
      {
        ShowSpherePointKeyframesInSkybox(group as SpherePointKeyframeGroup);
      }
      else
      {
        HideDebugPoints();
      }
    }

    private void RenderNeedsSkySetupLayout() {
      RenderNonInteractiveBaseLayout();

      if (RenderCenteredTimelineMessage("Your scene needs an active sky system.", "Create Sky System...")) {
        EditorWindow.GetWindow<SkySetupWindow>().Show();
      }
    }

    private void RenderNeedsProfileLayout() {
      RenderNonInteractiveBaseLayout();
      RenderCenteredTimelineMessage("The time controller in the scene has no profile assigned.", null);
    }

    private void RenderNonInteractiveBaseLayout() {
      // Background style.
      RenderBackground();

      // Time ruler.
      Rect timeScrubberRect = new Rect(
        NAME_COLUMN_WIDTH, 
        0,
        position.size.x - NAME_COLUMN_WIDTH,
        TIME_HEADER_HEIGHT);
      RenderTimeRuler(timeScrubberRect, 0.0f);

      // Divider.
      Rect dividerRect = new Rect(
        0,
        TIME_HEADER_HEIGHT - HORIZONTAL_DIVIDER_HEIGHT,
        position.width,
        HORIZONTAL_DIVIDER_HEIGHT);
      RenderHorizontalDivider(dividerRect);

      RenderTimeLabel(0.0f);
    }

    private bool DidDragTimeRuler() {
      if (Event.current.isMouse == false || Event.current.type != EventType.MouseDrag) {
        return false;
      }

      Rect rulerRect = new Rect(
        NAME_COLUMN_WIDTH, 
        0, 
        position.width - NAME_COLUMN_WIDTH, 
        TIME_HEADER_HEIGHT);

      return rulerRect.Contains(Event.current.mousePosition);
    }

    private void RenderBackground()
    {
      Rect fullWindowRect = new Rect(
        0,
        0,
        position.width,
        position.height);
      
      float windowBg = .20f;
      EditorGUI.DrawRect(fullWindowRect, new Color(windowBg, windowBg, windowBg, 1));

      Rect nameColRect = new Rect(
        0,
        0,
        NAME_COLUMN_WIDTH,
        position.height);

      float nameBg = .38f;
      EditorGUI.DrawRect(nameColRect, new Color(nameBg, nameBg, nameBg, 1));

      Rect vertDivider = new Rect(
        NAME_COLUMN_WIDTH - VERTICAL_DIVIDER_WIDTH,
        TIME_HEADER_HEIGHT,
        VERTICAL_DIVIDER_WIDTH,
        fullWindowRect.height);
      RenderVerticalDivider(vertDivider);
    }

    private void RenderHeaderButtons(Rect rect, TimeOfDayController tc)
    {
      RenderTimeLabel(tc.timeOfDay);

      Rect buttonsRect = new Rect(
        rect.x, rect.y + ICON_BUTTON_SIZE, 
        rect.width, rect.height - ICON_BUTTON_SIZE);
      RenderGlobalTimelineButtons(buttonsRect, tc);

      // Divider.
      Rect dividerRect = new Rect(
        rect.x,
        rect.y + TIME_HEADER_HEIGHT - HORIZONTAL_DIVIDER_HEIGHT,
        rect.width,
        HORIZONTAL_DIVIDER_HEIGHT);
      RenderHorizontalDivider(dividerRect);
    }

    private void RenderTimeLabel(float time) {
      GUIContent labelContent = new GUIContent(TimeStringFromPercent(time));
      Vector2 labelSize = m_TimeLabelStyle.CalcSize(labelContent);
      Rect timeRect = new Rect(
        5.0f,
        TIME_HEADER_HEIGHT - labelSize.y - 5.0f,
        labelSize.x,
        labelSize.y);

      EditorGUI.LabelField(timeRect, labelContent, m_TimeLabelStyle);
    }

    private static string TimeStringFromPercent(float percent)
    {
      float hoursFract = percent * 24.0f;
      int hours = (int) hoursFract;
      int minutes = (int)((hoursFract - hours) * 60.0f);

      string hourStr = hours < 10 ? "0" + hours : hours.ToString();
      string minuteStr = minutes < 10 ? "0" + minutes : minutes.ToString();

      return hourStr + ":" + minuteStr;
    }

    // These are the buttons next to the current time of day at the top.
    private void RenderGlobalTimelineButtons(Rect rect, TimeOfDayController tc)
    {
      GUILayout.BeginArea(rect);
      GUILayout.BeginHorizontal();
      GUILayout.FlexibleSpace();

      float buttonSize = 20.0f;
      Color originalContentColor = GUI.contentColor;
      GUI.contentColor = GUI.skin.label.normal.textColor;

      GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);

      buttonStyle.padding = new RectOffset(2, 2, 2, 2);


      Texture2D addTexture = SkyEditorUtility.LoadEditorResourceTexture("AddIcon");

      bool didClickAddButton = GUILayout.Button(
        new GUIContent(addTexture, "Add a sky property to the timeline."),
        buttonStyle, GUILayout.Width(buttonSize), GUILayout.Height(buttonSize));
      GUILayout.Space(LEFT_INSET);
      if (didClickAddButton)
      {
        SkyGUITimelineMenu.ShowAddTimelinePropertyMenu(m_ActiveSkyProfile);
      }

      GUI.contentColor = originalContentColor;
      GUILayout.EndHorizontal();
      GUILayout.EndArea();
    }

    private void DidClickAddNewKeyframe(TimeOfDayController tc)
    {
      IKeyframeGroup selectedGroup = tc.skyProfile.GetGroupWithId(TimelineSelection.selectedGroupUUID);
      if (selectedGroup == null)
      {
        Debug.LogError("Can't insert keyframe since no group was fould for selected UUID.");
        return;
      }

      Undo.RecordObject(tc.skyProfile, "Keyframe inserted into group.");

      if (selectedGroup is ColorKeyframeGroup)
      {
        InsertKeyframeInColorGroup(tc.timeOfDay, selectedGroup as ColorKeyframeGroup);
      }
      else if (selectedGroup is NumberKeyframeGroup)
      {
        InsertKeyframeInNumericGroup(tc.timeOfDay, selectedGroup as NumberKeyframeGroup);
      }
      else if (selectedGroup is SpherePointKeyframeGroup)
      {
        InsertKeyframeInSpherePointGroup(tc.timeOfDay, selectedGroup as SpherePointKeyframeGroup);
      }

      EditorUtility.SetDirty(tc.skyProfile);
      Repaint();
    }

    private void InsertKeyframeInColorGroup(float time, ColorKeyframeGroup group)
    {
      ColorKeyframe previousKeyFrame = group.GetPreviousKeyFrame(time);
      //Color keyColor = previousKeyFrame != null ? previousKeyFrame.color : Color.white;
      ColorKeyframe newKeyFrame = new ColorKeyframe(previousKeyFrame);
      newKeyFrame.time = time;
      group.AddKeyFrame(newKeyFrame);

      KeyframeInspectorWindow.SetKeyframeData(
        newKeyFrame, group, KeyframeInspectorWindow.KeyType.Color, m_ActiveSkyProfile);
    }

    private void InsertKeyframeInNumericGroup(float time, NumberKeyframeGroup group)
    {
      NumberKeyframe previousKeyframe = group.GetPreviousKeyFrame(time);
      NumberKeyframe newKeyFrame = new NumberKeyframe(previousKeyframe);
      newKeyFrame.time = time;
      group.AddKeyFrame(newKeyFrame);

      KeyframeInspectorWindow.SetKeyframeData(
        newKeyFrame, group, KeyframeInspectorWindow.KeyType.Numeric, m_ActiveSkyProfile);
    }

    private void InsertKeyframeInSpherePointGroup(float time, SpherePointKeyframeGroup group)
    {
      SpherePointKeyframe previousKeyFrame = group.GetPreviousKeyFrame(time);
      SpherePointKeyframe newKeyFrame = new SpherePointKeyframe(previousKeyFrame);
      newKeyFrame.time = time;
      group.AddKeyFrame(newKeyFrame);

      KeyframeInspectorWindow.SetKeyframeData(
        newKeyFrame, group, KeyframeInspectorWindow.KeyType.SpherePoint, m_ActiveSkyProfile);
    }

    private void RenderTimeRuler(Rect rect, float currentTime)
    {
      // 1 notch for every hour of the day.
      const int lineIncrements = 24;
      const float notchWidth = 1.0f;
      const float notchLabelYOffset = 10.0f;

      const int largeNotchPerHour = 3;
      float smallNotchHeight = 10.0f;
      float largeNotchHeight = 20.0f;

      float notchSectionWidth = rect.width / lineIncrements;

      Rect backgroundRect = new Rect(NAME_COLUMN_WIDTH, rect.y, position.width - NAME_COLUMN_WIDTH, TIME_HEADER_HEIGHT);
      EditorGUI.DrawRect(backgroundRect, Color.black);

      GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
      labelStyle.normal.textColor = GUI.color;

      // Draw the notches on the timeline.
      for (int i = 0; i < lineIncrements + 1; i++)
      {
        float notchHeight = smallNotchHeight;

        // Draw large notch every 6 hours.
        if (i % largeNotchPerHour == 0)
        {
          notchHeight = largeNotchHeight;

          float notchLabelXOffset = 0;
          string hourString = i.ToString();

          if (i == 0)
          {
            notchLabelXOffset = 0;
          } else if (i == lineIncrements)
          {
            notchLabelXOffset = -15.0f;
          }
          else
          {
            if (hourString.Length == 1)
            {
              notchLabelXOffset = -5.0f;
            }
            else
            {
              notchLabelXOffset = -8.0f;
            }
          }

          Rect timeLabelRect = new Rect(
            rect.x + i * notchSectionWidth + notchLabelXOffset,
            rect.y + notchLabelYOffset,
            20.0f,
            30.0f);
          
          EditorGUI.LabelField(timeLabelRect, new GUIContent(i.ToString()), labelStyle);
        }

        Rect finalNotchRect = new Rect(
          rect.x + i * notchSectionWidth - (notchWidth / 2.0f),
          rect.y + TIME_HEADER_HEIGHT - notchHeight - HORIZONTAL_DIVIDER_HEIGHT,
          notchWidth,
          notchHeight);  

        EditorGUI.DrawRect(finalNotchRect, Color.white);
      }

      // Divider.
      Rect dividerRect = new Rect(
        rect.x,
        rect.y + TIME_HEADER_HEIGHT - HORIZONTAL_DIVIDER_HEIGHT,
        rect.width,
        HORIZONTAL_DIVIDER_HEIGHT);
      RenderHorizontalDivider(dividerRect);
    }

    private void RenderTimelineCursor(Rect rect, TimeOfDayController timeController)
    {
      // Flag the start of a timeline drag.
      if (TimelineSelection.isDraggingTimeline == false &&
        (Event.current.type == EventType.MouseDrag) &&
        rect.Contains(Event.current.mousePosition))
      {
        TimelineSelection.isDraggingTimeline = true;
      }

      if (TimelineSelection.isDraggingTimeline)
      {
        float percent = Mathf.Clamp((Event.current.mousePosition.x - rect.x) / rect.width, 0, MAX_TIME_VALUE);
        timeController.skyTime = percent;
        EditorUtility.SetDirty(timeController);
      }

      if (Event.current.type != EventType.Repaint)
      {
        return;
      }

      float playHeadHeight = PLAYHEAD_WIDTH / 2.0f;

      float xCursorPos = SkyEditorUtility.GetXPositionForPercent(rect, timeController.timeOfDay);

      // Draw the line that overlaps all the content rows.
      const float extensionSize = 5.0f;
      Rect lineRect = new Rect(
        xCursorPos - (CURSOR_LINE_WIDTH / 2.0f), 
        rect.y + TIME_HEADER_HEIGHT - extensionSize,
        CURSOR_LINE_WIDTH, 
        rect.height - TIME_HEADER_HEIGHT + extensionSize);
      GUI.DrawTexture(lineRect, SkyEditorUtility.LoadEditorResourceTexture("CursorLine"));

      // Draw the playhead arrow.
      if (m_PlayheadTexture == null)
      {
        m_PlayheadTexture = SkyEditorUtility.LoadEditorResourceTexture("PlayheadArrow");
      }

      Rect headRect = new Rect(
        xCursorPos - (PLAYHEAD_WIDTH / 2.0f),
        rect.y + TIME_HEADER_HEIGHT - playHeadHeight,
        PLAYHEAD_WIDTH,
        playHeadHeight);

      GUI.DrawTexture(headRect, m_PlayheadTexture, ScaleMode.StretchToFill, true);
    }

    private bool RenderCenteredTimelineMessage(string emptyMessage, string buttonText) {
      Rect rect = new Rect(0, TIME_HEADER_HEIGHT, position.width, position.height);

      GUIContent emptyMessageContent = new GUIContent(emptyMessage);
      Vector2 labelWidth = m_EmptyTitleStyle.CalcSize(emptyMessageContent);
      float valueColumnWidth = rect.width - NAME_COLUMN_WIDTH;

      // Label message.
      Rect msgRect = new Rect(NAME_COLUMN_WIDTH + (valueColumnWidth - labelWidth.x) / 2.0f,
        rect.y + EMPTY_MESSAGE_Y_OFFSET, labelWidth.x, labelWidth.y);
      EditorGUI.LabelField(msgRect, new GUIContent(emptyMessage), m_EmptyTitleStyle);

      // Button to add first item to timeline.
      if (buttonText != null) {
        GUIContent addButtonContent = new GUIContent(buttonText);
        Vector2 addButtonSize = GUI.skin.button.CalcSize(addButtonContent);
        Rect addButtonRect = new Rect(NAME_COLUMN_WIDTH + (valueColumnWidth - addButtonSize.x) / 2.0f,
          msgRect.y + labelWidth.y + EMPTY_ADD_BUTTON_Y_OFFSET, addButtonSize.x, addButtonSize.y);

        return GUI.Button(addButtonRect, addButtonContent);
      }

      return false;
    }

    private void RenderEmptyTimelineMessage() {
      bool didClickAddProperty = RenderCenteredTimelineMessage(
          "Animate sky properties by adding them to the timeline.",
          "Add to Timeline");

      if (didClickAddProperty)
      {
        SkyGUITimelineMenu.ShowAddTimelinePropertyMenu(m_ActiveSkyProfile);
      }
      return;
    }

    private void RenderAllRows(Rect rect, SkyProfile profile)
    {
      Rect rowRect = new Rect(rect.x, rect.y + ROW_PADDING / 2.0f, rect.width, COLOR_ROW_HEIGHT);

      // Render all rows that are managed by the timeline.
      foreach (ProfileGroupDefinition groupInfo in m_TimelineDefinitions) {
        if (profile.IsManagedByTimeline(groupInfo.propertyKey) == false) {
          continue;
        }

        if (groupInfo.type == ProfileGroupDefinition.GroupType.Number) {
          RenderNumericRowAndAdvance(
            ref rowRect, 
            profile, 
            profile.GetGroup<NumberKeyframeGroup>(groupInfo.propertyKey),
            groupInfo);
        } else if (groupInfo.type == ProfileGroupDefinition.GroupType.Color) {
          RenderGradientRowAndAdvance(
            ref rowRect, 
            profile,
            profile.GetGroup<ColorKeyframeGroup>(groupInfo.propertyKey),
            groupInfo);
        } else if (groupInfo.type == ProfileGroupDefinition.GroupType.SpherePoint)
        {
          RenderSpherePointRowAndAdvance(
            ref rowRect,
            profile,
            profile.GetGroup<SpherePointKeyframeGroup>(groupInfo.propertyKey),
            groupInfo);
        }
      }
    }

    private void RebuildTimelineDefinitions(SkyProfile profile)
    {
      if (m_TimelineDefinitions == null)
      {
        m_TimelineDefinitions = new List<ProfileGroupDefinition>();
      }

      m_TimelineDefinitions.Clear();

      foreach (string groupKey in profile.timelineManagedKeys)
      {
        ProfileGroupDefinition groupDefinition = profile.GetGroupDefinitionForKey(groupKey);
        if (groupDefinition == null)
        {
          //Debug.LogError("Failed to get group definition for key: " + groupKey);
          continue;
        }

        m_TimelineDefinitions.Add(groupDefinition);
      }
    }

    // Load the rects to use for this row colorGroup.
    private void LoadRowInformation(ref Rect rect, string groupUUID, float rowHeight, out Rect valueRowRect, out Rect nameRowRect, out bool isActive)
    {
      valueRowRect = new Rect(rect.x + NAME_COLUMN_WIDTH, rect.y, rect.width - NAME_COLUMN_WIDTH - VALUE_COLUMN_INSET, rowHeight);
      nameRowRect = new Rect(rect.x, rect.y, NAME_COLUMN_WIDTH, rowHeight);
      isActive = TimelineSelection.selectedGroupUUID != null && TimelineSelection.selectedGroupUUID == groupUUID;
      rect.y += rowHeight + ROW_PADDING;
    }

    // Render a timeline of gradient keyframes.
    private void RenderGradientRowAndAdvance(ref Rect rect, SkyProfile profile, ColorKeyframeGroup group, ProfileGroupDefinition groupDefinition)
    {
      rect.height = COLOR_ROW_HEIGHT;
      UpdateActiveSelectedRow(rect, group.id, groupDefinition.propertyKey);

      Rect valueRowRect;
      Rect nameRowRect;
      bool isActive;
      LoadRowInformation(ref rect, group.id, COLOR_ROW_HEIGHT, out valueRowRect, out nameRowRect, out isActive);

      RenderRowTitle(nameRowRect, group.name, isActive, groupDefinition);
      ColorTimelineRow.RenderColorGroupRow(valueRowRect, profile, group);
    }

    // Render a timeline of sphere point keyframes.
    private void RenderSpherePointRowAndAdvance(ref Rect rect, SkyProfile profile, SpherePointKeyframeGroup group, ProfileGroupDefinition groupDefinition)
    {
      rect.height = SPHERE_POINT_ROW_HEIGHT;
      UpdateActiveSelectedRow(rect, group.id, groupDefinition.propertyKey);

      Rect valueRowRect;
      Rect nameRowRect;
      bool isActive;
      LoadRowInformation(ref rect, group.id, SPHERE_POINT_ROW_HEIGHT, out valueRowRect, out nameRowRect, out isActive);

      // Render debug points if this is active.
      if (isActive)
      {
        ShowSpherePointKeyframesInSkybox(group);
      }

      RenderRowTitle(nameRowRect, group.name, isActive, groupDefinition);
      SpherePointTimelineRow.RenderSpherePointRow(valueRowRect, profile, group);
    }

    // Render a timeline of numeric keyframe positions.
    private void RenderNumericRowAndAdvance(ref Rect rect, SkyProfile profile, NumberKeyframeGroup group, ProfileGroupDefinition groupDefinition)
    {
      rect.height = NUMBER_ROW_HEIGHT;
      UpdateActiveSelectedRow(rect, group.id, groupDefinition.propertyKey);

      Rect valueRowRect;
      Rect nameRowRect;
      bool isActive;
      LoadRowInformation(ref rect, group.id, NUMBER_ROW_HEIGHT, out valueRowRect, out nameRowRect, out isActive);

      RenderRowTitle(nameRowRect, group.name, isActive, groupDefinition);
      NumberTimelineRow.RenderNumberGroup(valueRowRect, profile, group);
    }

    private void UpdateActiveSelectedRow(Rect rect, string groupUUID, string propertyKey)
    {
      Rect clickableRowRect = new Rect(rect.x, rect.y, rect.width + VALUE_COLUMN_INSET, rect.height + ROW_PADDING);
      if (clickableRowRect.Contains(Event.current.mousePosition) && 
          Event.current.type == EventType.MouseDown)
      {
        TimelineSelection.selectedGroupUUID = groupUUID;
        UpdateExternalWindowsWithActiveGroupSelection(propertyKey);
      }
    }

    private bool RenderGroupButton(Rect rowRect, int buttonIndex, string iconName, string tooltip)
    {
      Texture2D texIcon = SkyEditorUtility.LoadEditorResourceTexture(iconName);
      if (texIcon == null)
      {
        Debug.LogError("Failed to load icon for group button.");
        return false;
      }

      Color originalColor = GUI.color;
      GUI.contentColor = GUI.skin.label.normal.textColor;

      const float buttonPadding = 2.0f;
      const float buttonEndPadding = 5.0f;
      Rect btnRect = new Rect(
        NAME_COLUMN_WIDTH - buttonEndPadding - ((buttonIndex + 1) * ICON_BUTTON_SIZE) - ((buttonIndex) * buttonPadding),
        rowRect.y + (rowRect.height - ICON_BUTTON_SIZE) / 2.0f,
        ICON_BUTTON_SIZE,
        ICON_BUTTON_SIZE
      );

      bool didClickButton = GUI.Button(
        btnRect,
        new GUIContent(null, texIcon, tooltip),
        m_ButtonStyle);

      GUI.contentColor = originalColor;
      return didClickButton;
    }

    private void RenderRowTitle(Rect rect, string rowTitle, bool isActive, ProfileGroupDefinition groupDefinition)
		{
      GUILayout.Space(LEFT_INSET);
      m_GroupTitleStyle.fontStyle = isActive ? FontStyle.Bold : FontStyle.Normal;

      const float labelHeight = 18;
      Rect labelRect = new Rect(
        5, rect.y + ((rect.height - labelHeight) / 2.0f), NAME_COLUMN_WIDTH, labelHeight);
      EditorGUI.LabelField(labelRect, new GUIContent(rowTitle, groupDefinition.tooltip), m_GroupTitleStyle);

      // Render buttons over the active row.
      if (isActive) {
        if (RenderGroupButton(rect, 1, "AddIcon", "Add a keyframe at the current cursor position.")) {
          DidClickAddNewKeyframe(m_ActiveTimeController);
        }

        if (RenderGroupButton(rect, 0, "HelpIcon", "Show help information about this group property.")) {
          GroupHelpWindow.SetHelpItem(m_ActiveSkyProfile, groupDefinition.propertyKey, true);
        }
      }

      // Draw a divider between rows.
		  float dividerYPosition = rect.y + rect.height + (ROW_PADDING / 2.0f) - HORIZONTAL_DIVIDER_HEIGHT;
      RenderHorizontalDivider(new Rect(rect.x, dividerYPosition, rect.width, HORIZONTAL_DIVIDER_HEIGHT));
    }

    private void RenderHorizontalDivider(Rect dividerRect, string imageName = "HorizontalDividerOverlay")
    {
      Texture2D dividerTexture = SkyEditorUtility.LoadEditorResourceTexture(imageName);
      GUI.DrawTexture(dividerRect, dividerTexture);
    }

    private void RenderVerticalDivider(Rect dividerRect, string imageName = "VerticalDividerOverlay")
    {
      Texture2D dividerTexture = SkyEditorUtility.LoadEditorResourceTexture(imageName);
      GUI.DrawTexture(dividerRect, dividerTexture);
    }

    private void UpdateExternalWindowsWithActiveGroupSelection(string propertyKey)
    {
      GroupHelpWindow.SetHelpItem(m_ActiveSkyProfile, propertyKey, false);
    }

    private void ShowSpherePointKeyframesInSkybox(SpherePointKeyframeGroup group)
    {
      int debugPoints = 0;

      for (int i = 0; i < group.keyframes.Count; i++)
      {
        SpherePointKeyframe keyframe = group.keyframes[i];
        Vector3 direction = keyframe.spherePoint.GetWorldDirection();
        float isActiveKeyframe = SkyEditorUtility.IsKeyframeActiveInInspector(keyframe) ? 1.0f : 0.0f;
        Vector4 pointData = new Vector4(direction.x, direction.y, direction.z, isActiveKeyframe);
        
        if (i < MAX_DEBUG_POINTS) {
          m_DebugPoints[i] = pointData;
          debugPoints += 1;
        }
      }

      ShowDebugPoints(m_DebugPoints, debugPoints);
    }

    private void ShowDebugPoints(Vector4[] points, int count)
    {
      if (m_ActiveSkyProfile == null || m_ActiveSkyProfile.skyboxMaterial == null || points == null)
      {
        return;
      }

      m_ActiveSkyProfile.skyboxMaterial.EnableKeyword(ShaderKeywords.RenderDebugPoints);

      m_ActiveSkyProfile.skyboxMaterial.SetInt("_DebugPointsCount", count);
      m_ActiveSkyProfile.skyboxMaterial.SetVectorArray("_DebugPoints", points);
    }

    private void HideDebugPoints()
    {
      if (m_ActiveSkyProfile == null || m_ActiveSkyProfile.skyboxMaterial == null)
      {
        return;
      }

      m_ActiveSkyProfile.skyboxMaterial.SetInt("_DebugPointsCount", 0);
      m_ActiveSkyProfile.skyboxMaterial.DisableKeyword(ShaderKeywords.RenderDebugPoints);
    }
  }
}

