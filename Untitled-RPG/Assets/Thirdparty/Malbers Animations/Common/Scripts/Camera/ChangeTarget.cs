using UnityEngine;

namespace MalbersAnimations
{
  public class ChangeTarget : MonoBehaviour
    {
        public Transform[] targets;
        public KeyCode key = KeyCode.T;
        int current;

        [Tooltip("Deactivate the Inputs of the other targets to keep them from moving")]
        public bool NoInputs = false;
        private MFreeLookCamera m;

        // Update is called once per frame

        void Start()
        {
            if (NoInputs)
            {
                IInputSource input = null;

                for (int i = 0; i < targets.Length; i++)
                {
                    if (targets[i])
                    {
                        input = targets[i].GetComponent<IInputSource>();
                        if (input != null)input.Enable(false);
                    }
                }

                m = GetComponent<MFreeLookCamera>();
                if (m && m.Target)
                {
                    input = m.Target.GetComponent<IInputSource>();
                    if (input != null) input.Enable(true);

                    for (int i = 0; i < targets.Length; i++)
                    {
                        if (targets[i] == m.Target)
                        {
                            current = i;
                            break;
                        }
                    }
                }
            } 
        }

        void Update()
        {
            if (targets.Length == 0) return;
            if (targets.Length > current && targets[current] == null) return;

            if (Input.GetKeyDown(key))
            {
                if (NoInputs)
                {
                    IInputSource input = targets[current].GetComponent<IInputSource>();
                    if (input != null) input.Enable(false);
                }

                current++;
                current = current % targets.Length;
                SendMessage("SetTarget", targets[current]);

                if (NoInputs)
                {
                    IInputSource input = targets[current].GetComponent<IInputSource>();
                    if (input != null) input.Enable(true);
                }
            }
        }
    }
}
