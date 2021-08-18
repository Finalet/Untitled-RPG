using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using MalbersAnimations.Events;
using MalbersAnimations.Scriptables;

namespace MalbersAnimations
{
    [HelpURL("https://malbersanimations.gitbook.io/animal-controller/mobile/mobile-joystick")]
    [AddComponentMenu("Malbers/Input/Mobile Joystick")]

    public class MobileJoystick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
    {
        [Tooltip("Inverts the Horizontal value of the joystick")]
        public bool invertX;
        [Tooltip("Inverts the Vertical value of the joystick")]
        public bool invertY;                     // Bollean to define whether or not the Y axis is inverted.

        [Tooltip("If the Axis Magnitude is lower than this value then the Axis will zero out")]
        public float deathpoint = 0.1f;
        /// <summary>sensitivity for the X Axis</summary>
        public float sensitivityX = 1;
        /// <summary>sensitivity for the Y Axis</summary>
        public float sensitivityY = 1;


        [Tooltip("The Joystick Start position will be First click on the Area")]
        public bool Dynamic = false;

        //    [Header("References")]
        /// <summary> Is the Joystick is being pressed.</summary>
        public BoolReference pressed;
        /// <summary>Variable to Store the XAxis and Y Axis of the JoyStick</summary>
        public Vector2Reference axisValue;
        private Vector2 DeltaDrag;

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
        [Tooltip("If true, then the joystick will not use the starting position as guide for calculating the movement axis")]
        public bool m_Drag = false;


        /// <summary>JoyStick Background</summary>
        public Graphic bg;

        /// <summary>Drag Area Background</summary>
        public Graphic DragRect;

        /// <summary>JoyStick Button</summary>
        public Graphic Jbutton;

        /// <summary>Mutliplier to </summary>
        private const float mult = 3;

       // private Transform m_Cam;

        public bool Pressed
        {
            get => pressed; 
            set { OnJoystickPressed.Invoke(pressed.Value = value); }
        }

        public Vector2 AxisValue
        {
            get => axisValue;  
            set
            {
                if (invertX) value.x *= -1;
                if (invertY) value.y *= -1;

                axisValue.Value = value;
            }
        }

        public float XAxis => AxisValue.x;
        public float YAxis => AxisValue.y;

        void Start()
        {
            if (bg == null)   bg = GetComponent<Graphic>();
            if (Jbutton == null) Jbutton = transform.GetChild(0).GetComponent<Graphic>();
            if (DragRect == null) DragRect = GetComponent<Graphic>(); 

            BgXSize = bg.rectTransform.sizeDelta.x;
            BgYSize = bg.rectTransform.sizeDelta.y;
        }

        void Update()
        {
            if (Pressed)
            {
                OnAxisChange.Invoke(axisValue);
                OnXAxisChange.Invoke(axisValue.Value.x);
                OnYAxisChange.Invoke(axisValue.Value.y);
            }
        }



        // When draging is occuring this will be called every time the cursor is moved.
        public virtual void OnDrag(PointerEventData Point)
        {
            Vector2 TargetAxis = Vector2.zero; ;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(bg.rectTransform, Point.position, Point.pressEventCamera, out Vector2 pos))
            {
                if (!m_Drag || Dynamic)
                {
                    pos.x /= BgXSize;              // Get the Joystick position on the 2 axes based on the Bg position.
                    pos.y /= BgYSize;              // Get the Joystick position on the 2 axes based on the Bg position.

                    TargetAxis = new Vector3(pos.x * mult * sensitivityX, pos.y * mult * sensitivityY);        // Position is relative to the  Bg.

                    TargetAxis = (TargetAxis.magnitude > 1.0f ? TargetAxis.normalized : TargetAxis);

                    Vector2 JButtonPos = new Vector2(TargetAxis.x * (BgXSize / mult), TargetAxis.y * (BgYSize / mult));

                    Jbutton.rectTransform.anchoredPosition = JButtonPos;
                }
                else
                {
                    Jbutton.rectTransform.anchoredPosition = pos;
                    var relative = pos - DeltaDrag;

                    TargetAxis =
                        new Vector3(relative.x * sensitivityX * Screen.width * 0.001f, relative.y * sensitivityY * 0.001f * Screen.height);      // Position is relative to the  Bg.
                    DeltaDrag = pos;
                }
            }
             

            if (TargetAxis.magnitude <= deathpoint)
            {
                AxisValue = Vector2.zero;
            }
            else
            {
                AxisValue = TargetAxis;
            }


            //OnAxisChange.Invoke(axisValue);
            //OnXAxisChange.Invoke(axisValue.Value.x);
            //OnYAxisChange.Invoke(axisValue.Value.y);
        }

        // When the virtual analog's press occured this will be called.
        public virtual void OnPointerDown(PointerEventData Point)
        {
            OnJoystickDown.Invoke();
            Pressed = true;

            DeltaDrag = Vector2.zero;

            if (Dynamic && !m_Drag)
            {
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(DragRect.rectTransform, Point.position, Point.pressEventCamera, out Vector2 DeltaDrag))
                {
                    DeltaDrag.x -= DragRect.rectTransform.sizeDelta.x;              // Get the Joystick Correct X Position
                    DeltaDrag.y -= DragRect.rectTransform.sizeDelta.y;              // Get the Joystick Correct X Position
                    bg.rectTransform.anchoredPosition = DeltaDrag;
                }
            }
            else
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(bg.rectTransform, Point.position, Point.pressEventCamera, out DeltaDrag);
            }
            OnDrag(Point);
        }

        // When the virtual analog's release occured this will be called.
        public virtual void OnPointerUp(PointerEventData ped)
        {
            OnJoystickUp.Invoke();
            Pressed = false;
            AxisValue = Vector2.zero;
            Jbutton.rectTransform.anchoredPosition = Vector3.zero;
            DeltaDrag = Vector2.zero;
            OnAxisChange.Invoke(axisValue);
            OnXAxisChange.Invoke(axisValue.Value.x);
            OnYAxisChange.Invoke(axisValue.Value.y);
        }
    }
}