using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ES3Internal
{
    internal static class ES3Debug
    {
        private const string disableWarningMsg = "\n<i>To disable warnings from Easy Save, go to Window > Easy Save 3 > Settings, and uncheck 'Log Warnings'</i>";
        public static void Log(string msg, Object context = null)
        {
            if (!ES3Settings.defaultSettingsScriptableObject.logInfo)
                return;
            else if (context != null)
                Debug.LogFormat(context, msg);
            else
                Debug.LogFormat(context, msg);
        }

        public static void LogWarning(string msg, Object context=null)
        {
            if (!ES3Settings.defaultSettingsScriptableObject.logWarnings)
                return;
            else if (context != null)
                Debug.LogWarningFormat(context, msg + disableWarningMsg);
            else
                Debug.LogWarningFormat(context, msg + disableWarningMsg);
        }

        public static void LogError(string msg, Object context = null)
        {
            if (!ES3Settings.defaultSettingsScriptableObject.logErrors)
                return;
            else if (context != null)
                Debug.LogErrorFormat(context, msg);
            else
                Debug.LogErrorFormat(context, msg);
        }
    }
}
