using UnityEngine;


public class FPD_OverridableFloatAttribute : PropertyAttribute
{
    public string BoolVarName;
    public string TargetVarName;
    public int LabelWidth;

    public FPD_OverridableFloatAttribute(string boolVariableName, string targetVariableName, int labelWidth = 90)
    {
        BoolVarName = boolVariableName;
        TargetVarName = targetVariableName;
        LabelWidth = labelWidth;
    }
}


// -------------------------- Next F Property Drawer -------------------------- \\


public class BackgroundColorAttribute : PropertyAttribute
{
    public float r;
    public float g;
    public float b;
    public float a;

    public BackgroundColorAttribute()
    {
        r = g = b = a = 1f;
    }

    public BackgroundColorAttribute(float aR, float aG, float aB, float aA)
    {
        r = aR;
        g = aG;
        b = aB;
        a = aA;
    }

    public Color Color { get { return new Color(r, g, b, a); } }
}


// -------------------------- Next F Property Drawer -------------------------- \\

public class FPD_WidthAttribute : PropertyAttribute
{
    public float LabelWidth;

    public FPD_WidthAttribute(int labelWidth)
    {
        LabelWidth = labelWidth;
    }
}

// -------------------------- Next F Property Drawer -------------------------- \\

public class FPD_IndentAttribute : PropertyAttribute
{
    public int IndentCount = 1;
    public int LabelsWidth = 0;
    public int SpaceAfter = 0;

    public FPD_IndentAttribute(int indent = 1, int labelsWidth = 0, int spaceAfter = 0)
    {
        IndentCount = indent;
        LabelsWidth = labelsWidth;
        SpaceAfter = spaceAfter;
    }
}

// -------------------------- Next F Property Drawer -------------------------- \\

public class FPD_HorizontalLineAttribute : PropertyAttribute
{
    public Color color;

    public FPD_HorizontalLineAttribute(float r = 0.55f, float g = 0.55f, float b = 0.55f, float a = 0.7f)
    {
        color = new Color(r, g, b, a);
    }
}