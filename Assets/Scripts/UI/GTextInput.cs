using System.Collections.Generic;
using FairyGUI.Utils;
using UnityEngine;

namespace FairyGUI
{
    public class GTextInput : GTextField
    {
        public InputTextField inputTextField { get; private set; }

        private EventListener _onChanged;
        private EventListener _onSubmit;

        public GTextInput()
        {
            _textField.autoSize = AutoSizeType.None;
            _textField.wordWrap = false;
        }

        public EventListener onChanged => _onChanged ?? (_onChanged = new EventListener(this, "onChanged"));

        public EventListener onSubmit => _onSubmit ?? (_onSubmit = new EventListener(this, "onSubmit"));

        public bool editable
        {
            get => inputTextField.editable;
            set => inputTextField.editable = value;
        }

        public bool hideInput
        {
            get => inputTextField.hideInput;
            set => inputTextField.hideInput = value;
        }

        public int maxLength
        {
            get => inputTextField.maxLength;
            set => inputTextField.maxLength = value;
        }

        public string restrict
        {
            get => inputTextField.restrict;
            set => inputTextField.restrict = value;
        }

        public bool displayAsPassword
        {
            get => inputTextField.displayAsPassword;
            set => inputTextField.displayAsPassword = value;
        }

        public int caretPosition
        {
            get => inputTextField.caretPosition;
            set => inputTextField.caretPosition = value;
        }

        public string promptText
        {
            get => inputTextField.promptText;
            set => inputTextField.promptText = value;
        }

        /// <summary>
        /// 在移动设备上是否使用键盘输入。如果false，则文本在获得焦点后不会弹出键盘。
        /// </summary>
        public bool keyboardInput
        {
            get => inputTextField.keyboardInput;
            set => inputTextField.keyboardInput = value;
        }

        /// <summary>
        /// <see cref="UnityEngine.TouchScreenKeyboardType"/>
        /// </summary>
        public int keyboardType
        {
            get => inputTextField.keyboardType;
            set => inputTextField.keyboardType = value;
        }

        public bool disableIME
        {
            get => inputTextField.disableIME;
            set => inputTextField.disableIME = value;
        }

        public Dictionary<uint, Emoji> emojies
        {
            get => inputTextField.emojies;
            set => inputTextField.emojies = value;
        }

        public int border
        {
            get => inputTextField.border;
            set => inputTextField.border = value;
        }

        public int corner
        {
            get => inputTextField.corner;
            set => inputTextField.corner = value;
        }

        public Color borderColor
        {
            get => inputTextField.borderColor;
            set => inputTextField.borderColor = value;
        }

        public Color backgroundColor
        {
            get => inputTextField.backgroundColor;
            set => inputTextField.backgroundColor = value;
        }

        public bool mouseWheelEnabled
        {
            get => inputTextField.mouseWheelEnabled;
            set => inputTextField.mouseWheelEnabled = value;
        }

        /// <param name="start"></param>
        /// <param name="length"></param>
        public void SetSelection(int start, int length)
        {
            inputTextField.SetSelection(start, length);
        }

        /// <param name="value"></param>
        public void ReplaceSelection(string value)
        {
            inputTextField.ReplaceSelection(value);
        }

        protected override void SetTextFieldText()
        {
            inputTextField.text = _text;
        }

        protected override void CreateDisplayObject()
        {
            inputTextField = new InputTextField();
            inputTextField.gOwner = this;
            displayObject = inputTextField;

            _textField = inputTextField.textField;
        }

        public override void Setup_BeforeAdd(ByteBuffer buffer, int beginPos)
        {
            base.Setup_BeforeAdd(buffer, beginPos);

            buffer.Seek(beginPos, 4);

            var str = buffer.ReadS();
            if (str != null)
                inputTextField.promptText = str;

            str = buffer.ReadS();
            if (str != null)
                inputTextField.restrict = str;

            var iv = buffer.ReadInt();
            if (iv != 0)
                inputTextField.maxLength = iv;
            iv = buffer.ReadInt();
            if (iv != 0)
                inputTextField.keyboardType = iv;
            if (buffer.ReadBool())
                inputTextField.displayAsPassword = true;
        }
    }
}