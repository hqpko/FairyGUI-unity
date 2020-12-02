using UnityEngine;

namespace FairyGUI
{
    public enum HitTestMode
    {
        Default,
        Raycast
    }

    public interface IHitTest
    {
        /// <param name="contentRect"></param>
        /// <param name="localPoint"></param>
        /// <returns></returns>
        bool HitTest(Rect contentRect, Vector2 localPoint);
    }
}