﻿using UnityEngine;

namespace FairyGUI
{
    public class TouchScreenKeyboard : IKeyboard
    {
        private UnityEngine.TouchScreenKeyboard _keyboard;

        public bool done
        {
#if UNITY_2017_2_OR_NEWER
            get
            {
                return _keyboard == null
                       || _keyboard.status == UnityEngine.TouchScreenKeyboard.Status.Done
                       || _keyboard.status == UnityEngine.TouchScreenKeyboard.Status.Canceled
                       || _keyboard.status == UnityEngine.TouchScreenKeyboard.Status.LostFocus;
            }
#else
            get { return _keyboard == null || _keyboard.done || _keyboard.wasCanceled; }
#endif
        }

        public bool supportsCaret => false;

        public string GetInput()
        {
            if (_keyboard != null)
            {
                var s = _keyboard.text;

                if (done)
                    _keyboard = null;

                return s;
            }
            else
            {
                return null;
            }
        }

        public void Open(string text, bool autocorrection, bool multiline, bool secure, bool alert,
            string textPlaceholder, int keyboardType, bool hideInput)
        {
            if (_keyboard != null)
                return;

            UnityEngine.TouchScreenKeyboard.hideInput = hideInput;
            _keyboard = UnityEngine.TouchScreenKeyboard.Open(text, (TouchScreenKeyboardType) keyboardType,
                autocorrection, multiline, secure, alert, textPlaceholder);
        }

        public void Close()
        {
            if (_keyboard != null)
            {
                _keyboard.active = false;
                _keyboard = null;
            }
        }
    }
}