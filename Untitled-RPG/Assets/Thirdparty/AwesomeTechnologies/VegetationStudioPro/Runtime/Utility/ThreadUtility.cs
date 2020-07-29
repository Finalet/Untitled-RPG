using System.Threading;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AwesomeTechnologies.Utility
{
#if UNITY_EDITOR
    [InitializeOnLoad]
    public class ThreadUtility
    {
        public static Thread MainThread = null;

        static ThreadUtility()
        {
            if (MainThread == null) MainThread = Thread.CurrentThread;
        }
    }
#endif
}
