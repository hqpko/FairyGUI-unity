using System;
using System.Collections.Generic;
using System.Text;

namespace FairyGUI.Utils
{
    public class HtmlImage : IHtmlObject
    {
        public GLoader loader { get; private set; }

        private RichTextField _owner;
        private HtmlElement _element;
        private bool _externalTexture;

        public HtmlImage()
        {
            loader = (GLoader) UIObjectFactory.NewObject(ObjectType.Loader);
            loader.gameObjectName = "HtmlImage";
            loader.fill = FillType.ScaleFree;
            loader.touchable = false;
        }

        public DisplayObject displayObject => loader.displayObject;

        public HtmlElement element => _element;

        public float width => loader.width;

        public float height => loader.height;

        public void Create(RichTextField owner, HtmlElement element)
        {
            _owner = owner;
            _element = element;

            var sourceWidth = 0;
            var sourceHeight = 0;
            var texture = owner.htmlPageContext.GetImageTexture(this);
            if (texture != null)
            {
                sourceWidth = texture.width;
                sourceHeight = texture.height;

                loader.texture = texture;
                _externalTexture = true;
            }
            else
            {
                var src = element.GetString("src");
                if (src != null)
                {
                    var pi = UIPackage.GetItemByURL(src);
                    if (pi != null)
                    {
                        sourceWidth = pi.width;
                        sourceHeight = pi.height;
                    }
                }

                loader.url = src;
                _externalTexture = false;
            }

            var width = element.GetInt("width", sourceWidth);
            var height = element.GetInt("height", sourceHeight);

            if (width == 0)
                width = 5;
            if (height == 0)
                height = 10;
            loader.SetSize(width, height);
        }

        public void SetPosition(float x, float y)
        {
            loader.SetXY(x, y);
        }

        public void Add()
        {
            _owner.AddChild(loader.displayObject);
        }

        public void Remove()
        {
            if (loader.displayObject.parent != null)
                _owner.RemoveChild(loader.displayObject);
        }

        public void Release()
        {
            loader.RemoveEventListeners();
            if (_externalTexture)
            {
                _owner.htmlPageContext.FreeImageTexture(this, loader.texture);
                _externalTexture = false;
            }

            loader.url = null;
            _owner = null;
            _element = null;
        }

        public void Dispose()
        {
            if (_externalTexture)
                _owner.htmlPageContext.FreeImageTexture(this, loader.texture);
            loader.Dispose();
        }
    }
}