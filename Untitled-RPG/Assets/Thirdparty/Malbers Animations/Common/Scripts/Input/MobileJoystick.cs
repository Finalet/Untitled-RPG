using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using MalbersAnimations.Events;
using MalbersAnimations.Scriptables;

namespace MalbersAnimations
{
    public class MobileJoystick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
    {
        //[Tooltip("The Character will follow the camera forward axis")]
        //public bool CameraInput = false;

        /// <summary>Invert the X Axis</summary>
        public bool invertX;
        public float deathpoint = 0.1f;
        /// <summary>Invert the X Axis</summary>
        public bool invertY;                     // Bollean to define whether or not the Y axis is inverted.

     //   [Header("Sensitivity")]
        /// <summary>sensitivity for the X Axis</summary>
        public float sensitivityX = 1;
        /// <summary>sensitivity for the Y Axis</summary>
        public float sensitivityY = 1;


    //    [Header("References")]
        /// <summary> Is the Joystick is being pressed.</summary>
        public BoolReference pressed;
        /// <summary>Variable to Store the XAxis and Y Axis of the JoyStick</summary>
        public Vector2Reference axisValue;


     //   [Header("Events")]
        public UnityEvent OnJoystickDown = new UnityEvent();
        public UnityEvent OnJoystickUp = new UnityEvent();
        public Vector2Event OnAxisChange = new Vector2Event();
        public FloatEvent OnXAxisChange = new FloatEvent();
        public FloatEvent OnYAxisChange = new FloatEvent();
        public BoolEvent OnJoystickPressed = new BoolEvent();

        private float BgXSize;
        private float BgYSize;


        public bool AxisEditor = true;
        public bool EventsEditor = true;
        public bool ReferencesEditor = true;


        /// <summary>JoyStick Background</summary>
        private Graphic bg;
        /// <summary>JoyStick Button</summary>
        private Graphic Jbutton;

        /// <summary>Mutliplier to </summary>
        private const float mult = 3;

       // private Transform m_Cam;

        public bool Pressed
        {
            get { return pressed; }
            set { OnJoystickPressed.Invoke(pressed.Value = value); }
        }

        public Vector2 AxisValue
        {
            get { return axisValue; }
            set
            {
                if (invertX) value.x *= -1;
                if (invertY) value.y *= -1;

                axisValue.Value = value;
            }
        }

        public float XAxis { get { return AxisValue.x; } }
        public float YAxis { get { return AxisValue.y; } }



        void Start()
        {
            bg = GetComponent<Graphic>();
            Jbutton = transform.GetChild(0).GetComponent<Graphic>();
            BgXSize = bg.rectTransform.sizeDelta.x;
            BgYSize = bg.rectTransform.sizeDelta.y;

            //m_Cam = Camera.main.transform;
        }

        void Update()
        {
            OnAxisChange.Invoke(axisValue);
            OnXAxisChange.Invoke(axisValue.Value.x);
            OnYAxisChange.Invoke(axisValue.Value.y);
        }



        // When draging is occuring this will be called every time the cursor is moved.
        public virtual void OnDrag(PointerEventData Point)
        {
            Vector2 pos;
            Vector2 TargetAxis = Vector2.zero; ;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(bg.rectTransform, Point.position, Point.pressEventCamera, out pos))
            {

                pos.x = (pos.x / BgXSize);              // Get the Joystick position on the 2 axes based on the Bg position.
                pos.y = (pos.y / BgYSize);              // Get the Joystick position on the 2 axes based on the Bg position.

                TargetAxis = new Vector3(pos.x * mult * sensitivityX, pos.y * mult * sensitivityY);        // Position is relative to the  Bg.

                TargetAxis = (TargetAxis.magnitude > 1.0f ? TargetAxis.normalized : TargetAxis);

                Vector2 JButtonPos = new Vector2(TargetAxis.x * (BgXSize / mult), TargetAxis.y * (BgYSize / mult));

                Jbutton.rectTransform.anchoredPosition = JButtonPos;
            }

            //if (CameraInput && m_Cam)
            //{

            //    var Input = (TargetAxis.y * m_Cam.forward) + (TargetAxis.x * m_Cam.right);
            //    TargetAxis = new Vector2(Input.x, Input.z);
            //}

            if (TargetAxis.magnitude <= deathpoint)
            {
                AxisValue = Vector2.zero;
            }
            else
            {
                AxisValue = TargetAxis;
            }
        }

        // When the virtual analog's press occured this will be called.
        public virtual void OnPointerDown(PointerEventData enventData)
        {
            Pressed = true;
            OnDrag(enventData);
            OnJoystickDown.Invoke();
        }

        // When the virtual analog's release occured this will be called.
        public virtual void OnPointerUp(PointerEventData ped)
        {
            Pressed = false;
            AxisValue = Vector2.zero;
            Jbutton.rectTransform.anchoredPosition = Vector3.zero;
            OnJoystickUp.Invoke();
        }
    }
}