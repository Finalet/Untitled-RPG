using System.Collections;
using Funly.SkyStudio;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace Funly.SkyStudio
{
  [InitializeOnLoad]
  public class EditorVersions
  {
    private static VersionLogger _versionLogger;
    
    static EditorVersions()
    {
      _versionLogger = new VersionLogger();
    }

    private class VersionLogger
    {
      public VersionLogger()
      {
        EditorCoroutine.start(StartRequest());
      }
      
      IEnumerator StartRequest()
      {
        string idKey = "sky_studio_uid";
        string uid = EditorPrefs.GetString(idKey,  GUID.Generate().ToString());
        string editorVersion = Application.unityVersion;
        
        EditorPrefs.SetString(idKey, uid);
        
        string baseUrl = "https://www.google-analytics.com/collect";
        string urlRequest = baseUrl 
                            + "?v=1&tid=UA-71359136-8&cid=" 
                            + uid + "&t=pageview&dp=Unity-Editor-" 
                            + UnityWebRequest.EscapeURL(Application.unityVersion )
                            + "&z=" 
                            + Random.Range(1, 10000).ToString();
        
        using (UnityWebRequest request = UnityWebRequest.Get(urlRequest))
        {
          request.SetRequestHeader("User-Agent", Application.platform.ToString());
          
          yield return request.SendWebRequest();
          
          if (request.responseCode != 200)
          {
            yield break;
          }
        }
        
      }
    }

  }
  
}