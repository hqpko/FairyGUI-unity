using System.Collections.Generic;

namespace FairyGUI
{
    public class EventContext
    {
        public EventDispatcher sender { get; internal set; }

        /// <summary>
        /// /
        /// </summary>
        public object initiator { get; internal set; }

        /// <summary>
        /// /
        /// </summary>
        public InputEvent inputEvent { get; internal set; }

        public string type;

        public object data;

        internal bool _defaultPrevented;
        internal bool _stopsPropagation;
        internal bool _touchCapture;

        internal List<EventBridge> callChain = new List<EventBridge>();

        public void StopPropagation()
        {
            _stopsPropagation = true;
        }

        public void PreventDefault()
        {
            _defaultPrevented = true;
        }

        public void CaptureTouch()
        {
            _touchCapture = true;
        }

        public bool isDefaultPrevented => _defaultPrevented;

        private static Stack<EventContext> pool = new Stack<EventContext>();

        internal static EventContext Get()
        {
            if (pool.Count > 0)
            {
                var context = pool.Pop();
                context._stopsPropagation = false;
                context._defaultPrevented = false;
                context._touchCapture = false;
                return context;
            }
            else
            {
                return new EventContext();
            }
        }

        internal static void Return(EventContext value)
        {
            pool.Push(value);
        }
    }
}