using UnityEngine;

namespace FairyGUI
{
    public class ColliderHitTest : IHitTest
    {
        public Collider collider;

        /// <param name="contentRect"></param>
        /// <param name="localPoint"></param>
        /// <returns></returns>
        public virtual bool HitTest(Rect contentRect, Vector2 localPoint)
        {
            RaycastHit hit;
            if (!HitTestContext.GetRaycastHitFromCache(HitTestContext.camera, out hit))
                return false;

            if (hit.collider != collider)
                return false;

            return true;
        }
    }
}