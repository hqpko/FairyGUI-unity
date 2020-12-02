using UnityEngine;

namespace FairyGUI
{
    public class InputEvent
    {
        /// <summary>
        /// x position in stage coordinates.
        /// </summary>
        public float x { get; internal set; }

        /// <summary>
        /// y position in stage coordinates.
        /// </summary>
        public float y { get; internal set; }

        public KeyCode keyCode { get; internal set; }

        public char character { get; internal set; }

        public EventModifiers modifiers { get; internal set; }

        public float mouseWheelDelta { get; internal set; }

        public int touchId { get; internal set; }

        /// <summary>
        /// -1-none,0-left,1-right,2-middle
        /// </summary>
        public int button { get; internal set; }

        /// <value></value>
        public int clickCount { get; internal set; }

        /// <summary>
        /// Duraion of holding the button. You can read this in touchEnd or click event.
        /// </summary>
        /// <value></value>
        public float holdTime { get; internal set; }

        public InputEvent()
        {
            touchId = -1;
            x = 0;
            y = 0;
            clickCount = 0;
            keyCode = KeyCode.None;
            character = '\0';
            modifiers = 0;
            mouseWheelDelta = 0;
        }

        public Vector2 position => new Vector2(x, y);

        public bool isDoubleClick => clickCount > 1 && button == 0;

        public bool ctrlOrCmd => ctrl || command;

        public bool ctrl => Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

        public bool shift => Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        public bool alt => Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);

        public bool command
        {
            get
            {
                //In win, as long as the win key and other keys are pressed at the same time, the getKey will continue to return true. So it can only be shielded.
                if (Application.platform == RuntimePlatform.OSXPlayer ||
                    Application.platform == RuntimePlatform.OSXEditor)
                    return Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightCommand);
                else
                    return false;
            }
        }
    }
}