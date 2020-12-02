using UnityEngine;

namespace FairyGUI
{
    public class ShapeHitTest : IHitTest
    {
        public DisplayObject shape;

        public ShapeHitTest(DisplayObject obj)
        {
            shape = obj;
        }

        /// <param name="contentRect"></param>
        /// <param name="localPoint"></param>
        /// <returns></returns>
        public bool HitTest(Rect contentRect, Vector2 localPoint)
        {
            if (shape.graphics == null)
                return false;

            if (shape.parent != null)
                localPoint = shape.parent.TransformPoint(localPoint, shape);

            var ht = shape.graphics.meshFactory as IHitTest;
            if (ht == null)
                return false;

            return ht.HitTest(contentRect, localPoint);
        }
    }
}