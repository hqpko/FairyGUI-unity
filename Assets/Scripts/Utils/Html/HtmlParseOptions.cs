using System;
using System.Collections.Generic;
using UnityEngine;

namespace FairyGUI.Utils
{
    public class HtmlParseOptions
    {
        public bool linkUnderline;

        public Color linkColor;

        public Color linkBgColor;

        public Color linkHoverBgColor;

        public bool ignoreWhiteSpace;

        public static bool DefaultLinkUnderline = true;

        public static Color DefaultLinkColor = new Color32(0x3A, 0x67, 0xCC, 0xFF);

        public static Color DefaultLinkBgColor = Color.clear;

        public static Color DefaultLinkHoverBgColor = Color.clear;

        public HtmlParseOptions()
        {
            linkUnderline = DefaultLinkUnderline;
            linkColor = DefaultLinkColor;
            linkBgColor = DefaultLinkBgColor;
            linkHoverBgColor = DefaultLinkHoverBgColor;
        }
    }
}