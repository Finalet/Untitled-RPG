﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.TerrainAPI;

[Serializable]
public class TerrainVisualizationSettings : ScriptableObject
{
    // Heatmap 
    public List<Color> ColorSelection = new List<Color> { Color.blue, Color.cyan, Color.green, Color.yellow, Color.red };
    public List<float> DistanceSelection = new List<float> { 100, 200, 300, 400, 500 };
    public enum REFERENCESPACE { LocalSpace, WorldSpace};
    public REFERENCESPACE ReferenceSpace;
    public enum MEASUREMENTS { Meters, Feet };
    public MEASUREMENTS CurrentMeasure;
    public const float CONVERSIONNUM = 3.280f;
    public float MinDistance = 100;
    public float MaxDistance = 500;
	public int HeatLevels = 5;
    public float SeaLevel;
    public bool WorldSpace = false;
}
