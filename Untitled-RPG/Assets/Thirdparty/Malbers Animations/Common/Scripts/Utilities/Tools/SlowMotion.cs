using UnityEngine;
using System.Collections;

namespace MalbersAnimations
{
    /// <summary> Going slow motion on user input</summary>
    [AddComponentMenu("Malbers/Utilities/Managers/Slow Motion")]

    public class SlowMotion : MonoBehaviour
    {
        [Space]
        [Range(0.05f, 1), SerializeField] float slowMoTimeScale = 0.25f;
        [Range(0.1f, 2), SerializeField] float slowMoSpeed = .2f;
        private bool PauseGame = false;
        private float CurrentTime = 1;

        IEnumerator SlowTime_C;
        private void Reset()
        { CreateInputs(); }
        public void PauseEditor() => Debug.Break();

        public void Slow_Motion()
        {
            if (SlowTime_C != null || !enabled) return; //Means that the Coroutine for slowmotion is still live


            if (Time.timeScale == 1.0F)
            {
                SlowTime_C = SlowTime();
                StartCoroutine(SlowTime_C);
            }
            else
            {
                SlowTime_C = RestartTime();
                StartCoroutine(SlowTime_C);
            }
            Time.fixedDeltaTime = 0.02F * Time.timeScale;
        }

        public virtual void Freeze_Game()
        {
            PauseGame ^= true;

            CurrentTime = Time.timeScale != 0 ? Time.timeScale : CurrentTime;

            Time.timeScale = PauseGame ? 0 : CurrentTime;
        }

        IEnumerator SlowTime()
        {
            while (Time.timeScale > slowMoTimeScale)
            {
                Time.timeScale = Mathf.Clamp(Time.timeScale - (1 / slowMoSpeed * Time.unscaledDeltaTime), 0, 100);
                Time.fixedDeltaTime = 0.02F * Time.timeScale;
                yield return null;
            }

            Time.timeScale =  CurrentTime = slowMoTimeScale;
           
            SlowTime_C = null;
        }

        IEnumerator RestartTime()
        {
            while (Time.timeScale < 1)
            {
                Time.timeScale += 1 / slowMoSpeed * Time.unscaledDeltaTime;
                yield return null;
            }
            Time.timeScale = CurrentTime = 1;
            SlowTime_C = null;
        }

        [ContextMenu("Create Inputs")]
        protected void CreateInputs()
        {
#if UNITY_EDITOR
            MInput input = GetComponent<MInput>();

            if (input == null)
                input = gameObject.AddComponent<MInput>();

            input.IgnoreOnPause.Value = false;

            #region Open Close Input
            var OpenCloseInput = input.FindInput("Freeze");
            if (OpenCloseInput == null)
            {
                OpenCloseInput = new InputRow("Freeze", "Freeze", KeyCode.Escape, InputButton.Down, InputType.Key);
                input.inputs.Add(OpenCloseInput);
                UnityEditor.Events.UnityEventTools.AddPersistentListener(OpenCloseInput.OnInputDown, Freeze_Game);
            }
            #endregion

            #region Submit Input
            var Submit = input.FindInput("Pause Editor");
            if (Submit == null)
            {
                Submit = new InputRow("Pause Editor", "Pause Editor", KeyCode.P, InputButton.Down, InputType.Key);
                input.inputs.Add(Submit);
                UnityEditor.Events.UnityEventTools.AddPersistentListener(Submit.OnInputDown, PauseEditor);
            }
            #endregion

            #region ChangeLeft Input
            var ChangeLeft = input.FindInput("SlowMo");
            if (ChangeLeft == null)
            {
                ChangeLeft = new InputRow("SlowMo", "SlowMo", KeyCode.Mouse2, InputButton.Down, InputType.Key);
                input.inputs.Add(ChangeLeft);
                UnityEditor.Events.UnityEventTools.AddPersistentListener(ChangeLeft.OnInputDown, Slow_Motion);
            }
            #endregion
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.EditorUtility.SetDirty(input);
#endif
        }
    }
}
