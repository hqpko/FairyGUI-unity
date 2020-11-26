using System.Collections.Generic;
using FairyGUI.Utils;

namespace FairyGUI
{

    public class RichTextField : Container
    {

        public IHtmlPageContext htmlPageContext { get; set; }


        public HtmlParseOptions htmlParseOptions { get; private set; }


        public Dictionary<uint, Emoji> emojies { get; set; }


        public TextField textField { get; private set; }

        public RichTextField()
        {
            gameObject.name = "RichTextField";
            this.opaque = true;

            htmlPageContext = HtmlPageContext.inst;
            htmlParseOptions = new HtmlParseOptions();

            this.textField = new TextField();
            textField.EnableRichSupport(this);
            AddChild(textField);
        }


        virtual public string text
        {
            get { return textField.text; }
            set { textField.text = value; }
        }


        virtual public string htmlText
        {
            get { return textField.htmlText; }
            set { textField.htmlText = value; }
        }


        virtual public TextFormat textFormat
        {
            get { return textField.textFormat; }
            set { textField.textFormat = value; }
        }


        /// <param name="name"></param>
        /// <returns></returns>
        public HtmlElement GetHtmlElement(string name)
        {
            List<HtmlElement> elements = textField.htmlElements;
            int count = elements.Count;
            for (int i = 0; i < count; i++)
            {
                HtmlElement element = elements[i];
                if (name.Equals(element.name, System.StringComparison.OrdinalIgnoreCase))
                    return element;
            }

            return null;
        }


        /// <param name="index"></param>
        /// <returns></returns>
        public HtmlElement GetHtmlElementAt(int index)
        {
            return textField.htmlElements[index];
        }


        public int htmlElementCount
        {
            get { return textField.htmlElements.Count; }
        }


        /// <param name="index"></param>
        /// <param name="show"></param>
        public void ShowHtmlObject(int index, bool show)
        {
            HtmlElement element = textField.htmlElements[index];
            if (element.htmlObject != null && element.type != HtmlElementType.Link)
            {
                //set hidden flag
                if (show)
                    element.status &= 253; //~(1<<1)
                else
                    element.status |= 2;

                if ((element.status & 3) == 0) //not (hidden and clipped)
                {
                    if ((element.status & 4) == 0) //not added
                    {
                        element.status |= 4;
                        element.htmlObject.Add();
                    }
                }
                else
                {
                    if ((element.status & 4) != 0) //added
                    {
                        element.status &= 251;
                        element.htmlObject.Remove();
                    }
                }
            }
        }

        public override void EnsureSizeCorrect()
        {
            textField.EnsureSizeCorrect();
        }

        override protected void OnSizeChanged()
        {
            textField.size = _contentRect.size; //千万不可以调用this.size,后者会触发EnsureSizeCorrect

            base.OnSizeChanged();
        }

        public override void Update(UpdateContext context)
        {
            textField.Redraw();

            base.Update(context);
        }

        public override void Dispose()
        {
            if ((_flags & Flags.Disposed) != 0)
                return;

            CleanupObjects();

            base.Dispose();
        }

        internal void CleanupObjects()
        {
            List<HtmlElement> elements = textField.htmlElements;
            int count = elements.Count;
            for (int i = 0; i < count; i++)
            {
                HtmlElement element = elements[i];
                if (element.htmlObject != null)
                {
                    element.htmlObject.Remove();
                    htmlPageContext.FreeObject(element.htmlObject);
                }
            }
        }

        virtual internal void RefreshObjects()
        {
            List<HtmlElement> elements = textField.htmlElements;
            int count = elements.Count;
            for (int i = 0; i < count; i++)
            {
                HtmlElement element = elements[i];
                if (element.htmlObject != null)
                {
                    if ((element.status & 3) == 0) //not (hidden and clipped)
                    {
                        if ((element.status & 4) == 0) //not added
                        {
                            element.status |= 4;
                            element.htmlObject.Add();
                        }
                    }
                    else
                    {
                        if ((element.status & 4) != 0) //added
                        {
                            element.status &= 251;
                            element.htmlObject.Remove();
                        }
                    }
                }
            }
        }
    }
}
