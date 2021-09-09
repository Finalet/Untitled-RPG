using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using TMPro;
using UnityEngine.UI;

public struct Watcher {
    public string name;
    public TextMeshProUGUI label;
    public FieldInfo fieldInfo;
    public PropertyInfo propertyInfo;
    public object target;

    public Watcher(string name, TextMeshProUGUI label, FieldInfo fieldInfo, PropertyInfo propertyInfo, object target)
    {
        this.name = string.IsNullOrEmpty(name) ? fieldInfo != null ? fieldInfo.Name : propertyInfo != null ? propertyInfo.Name : name : name;
        this.label = label;
        this.fieldInfo = fieldInfo;
        this.propertyInfo = propertyInfo;
        this.target = target;

        label.gameObject.SetActive(true);
    }

    public void UpdateWatcher () {
        string value = fieldInfo == null ? propertyInfo.GetValue(target).ToString() : fieldInfo.GetValue(target).ToString();
        label.text = $"{name}: {value}";
    }
}

public class DEBUG_Watchers : MonoBehaviour
{
    TextMeshProUGUI labelTemplate;
    VerticalLayoutGroup watchersGrid;

    List<Watcher> watchers = new List<Watcher>();

    void Awake() {
        labelTemplate = GetComponentInChildren<TextMeshProUGUI>();
        labelTemplate.gameObject.SetActive(false);
        
        watchersGrid = GetComponent<VerticalLayoutGroup>();
    }

    void Update() {
        UpdateWatchers();
    }

    public void CreateWatcher (string name, FieldInfo field, object target) {
        watchers.Add(new Watcher(name, Instantiate(labelTemplate, watchersGrid.transform), field, null, target));
    }
    public void CreateWatcher (string name, PropertyInfo property, object target) {
        watchers.Add(new Watcher(name, Instantiate(labelTemplate, watchersGrid.transform), null, property, target));
    }

    public void DeleteWatcher (string name) {
        int atIndex = -1;

        for (int i = 0; i < watchers.Count; i++) {
            if (watchers[i].name == name) {
                atIndex = i;
                break;
            }
        }

        if (atIndex == -1) throw new Exception($"Could not find watcher with name \"{name}\".");

        Destroy(watchers[atIndex].label.gameObject);
        watchers.RemoveAt(atIndex);

        if (watchers.Count == 0) Destroy(gameObject);
    }

    void UpdateWatchers () {
        for (int i = 0; i < watchers.Count; i++) {
            watchers[i].UpdateWatcher();
        }
    }
}
