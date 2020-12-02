using UnityEngine;

namespace FairyGUI
{
    public class RectHitTest : IHitTest
    {
        public Rect rect;

        /// <param name="contentRect"></param>
        /// <param name="localPoint"></param>
        /// <returns></returns>
        public bool HitTest(Rect contentRect, Vector2 localPoint)
        {
            return rect.Contains(localPoint);
        }
    }
}