using UnityEngine;

namespace FairyGUI
{
    public interface IColorGear
    {
        Color color { get; set; }
    }

    public interface ITextColorGear : IColorGear
    {
        Color strokeColor { get; set; }
    }
}