using System;
using System.Collections.Generic;
using UnityEngine;

namespace FairyGUI.Utils
{
    public class HtmlButton : IHtmlObject
    {
        public GComponent button { get; private set; }

        public const string CLICK_EVENT = "OnHtmlButtonClick";

        public static string resource;

        private RichTextField _owner;
        private HtmlElement _element;
        private EventCallback1 _clickHandler;

        public HtmlButton()
        {
            if (resource != null)
            {
                button = UIPackage.CreateObjectFromURL(resource).asCom;
                _clickHandler = (EventContext context) => { _owner.DispatchEvent(CLICK_EVENT, context.data, this); };
            }
            else
            {
                Debug.LogWarning("FairyGUI: Set HtmlButton.resource first");
            }
        }

        public DisplayObject displayObject => button != null ? button.displayObject : null;

        public HtmlElement element => _element;

        public float width => button != null ? button.width : 0;

        public float height => button != null ? button.height : 0;

        public void Create(RichTextField owner, HtmlElement element)
        {
            _owner = owner;
            _element = element;

            if (button == null)
                return;

            button.onClick.Add(_clickHandler);
            var width = element.GetInt("width", button.sourceWidth);
            var height = element.GetInt("height", button.sourceHeight);
            button.SetSize(width, height);
            button.text = element.GetString("value");
        }

        public void SetPosition(float x, float y)
        {
            if (button != null)
                button.SetXY(x, y);
        }

        public void Add()
        {
            if (button != null)
                _owner.AddChild(button.displayObject);
        }

        public void Remove()
        {
            if (button != null && button.displayObject.parent != null)
                _owner.RemoveChild(button.displayObject);
        }

        public void Release()
        {
            if (button != null)
                button.RemoveEventListeners();

            _owner = null;
            _element = null;
        }

        public void Dispose()
        {
            if (button != null)
                button.Dispose();
        }
    }
}