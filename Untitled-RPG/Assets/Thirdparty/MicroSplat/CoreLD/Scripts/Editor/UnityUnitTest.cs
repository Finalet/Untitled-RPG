using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JBooth.MicroSplat;
using UnityEditor;

public class UnityUnitTest
{

    static void Generate(string id, params string[] keywords)
    {
        var path = "Assets/MicroSplatUnitTest/" + id;
        System.IO.Directory.CreateDirectory(path);

        MicroSplatShaderGUI.NewShaderAndMaterial(path, id, keywords);


    }
    // generates a minimum set of shaders to test SRP problem points.
    // Usually changes in the include files, such as changing the shadow vertex definition,
    // would be caught by this. However, changes in how things are rendered, not so much.

    //[MenuItem("MicroSplat/UnitTestURPShaders")]
    public static void GenerateShaders()
    {
#if __MICROSPLAT__ && __MICROSPLAT_TESSELLATION__ && __MICROSPLAT_DIGGER__ && __MICROSPLAT_URP__

        Generate("Basic");
        Generate("Specular", MicroSplatBaseFeatures.DefineFeature._USESPECULARWORKFLOW.ToString());
        Generate("Tess", MicroSplatTessellation.DefineFeature._TESSDISTANCE.ToString());
        Generate("Dig", MicroSplatDiggerModule.DefineFeature._OUTPUTDIGGER.ToString());

        return;
#else

        Debug.LogError("MicroSplat must be installed with URP, tessellation, and digger integration modules installed.");
#endif
    }
}
