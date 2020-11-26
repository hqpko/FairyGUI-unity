using UnityEngine;

namespace FairyGUI
{

    public class GTween
    {

        public static bool catchCallbackExceptions = false;


        /// <param name="startValue"></param>
        /// <param name="endValue"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        public static GTweener To(float startValue, float endValue, float duration)
        {
            return TweenManager.CreateTween()._To(startValue, endValue, duration);
        }


        /// <param name="startValue"></param>
        /// <param name="endValue"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        public static GTweener To(Vector2 startValue, Vector2 endValue, float duration)
        {
            return TweenManager.CreateTween()._To(startValue, endValue, duration);
        }


        /// <param name="startValue"></param>
        /// <param name="endValue"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        public static GTweener To(Vector3 startValue, Vector3 endValue, float duration)
        {
            return TweenManager.CreateTween()._To(startValue, endValue, duration);
        }


        /// <param name="startValue"></param>
        /// <param name="endValue"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        public static GTweener To(Vector4 startValue, Vector4 endValue, float duration)
        {
            return TweenManager.CreateTween()._To(startValue, endValue, duration);
        }


        /// <param name="startValue"></param>
        /// <param name="endValue"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        public static GTweener To(Color startValue, Color endValue, float duration)
        {
            return TweenManager.CreateTween()._To(startValue, endValue, duration);
        }


        /// <param name="startValue"></param>
        /// <param name="endValue"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        public static GTweener ToDouble(double startValue, double endValue, float duration)
        {
            return TweenManager.CreateTween()._To(startValue, endValue, duration);
        }


        /// <param name="delay"></param>
        /// <returns></returns>
        public static GTweener DelayedCall(float delay)
        {
            return TweenManager.CreateTween().SetDelay(delay);
        }


        /// <param name="startValue"></param>
        /// <param name="amplitude"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        public static GTweener Shake(Vector3 startValue, float amplitude, float duration)
        {
            return TweenManager.CreateTween()._Shake(startValue, amplitude, duration);
        }


        /// <param name="target"></param>
        /// <returns></returns>
        public static bool IsTweening(object target)
        {
            return TweenManager.IsTweening(target, TweenPropType.None);
        }


        /// <param name="target"></param>
        /// <param name="propType"></param>
        /// <returns></returns>
        public static bool IsTweening(object target, TweenPropType propType)
        {
            return TweenManager.IsTweening(target, propType);
        }


        /// <param name="target"></param>
        public static void Kill(object target)
        {
            TweenManager.KillTweens(target, TweenPropType.None, false);
        }


        /// <param name="target"></param>
        /// <param name="complete"></param>
        public static void Kill(object target, bool complete)
        {
            TweenManager.KillTweens(target, TweenPropType.None, complete);
        }


        /// <param name="target"></param>
        /// <param name="propType"></param>
        /// <param name="complete"></param>
        public static void Kill(object target, TweenPropType propType, bool complete)
        {
            TweenManager.KillTweens(target, propType, complete);
        }


        /// <param name="target"></param>
        /// <returns></returns>
        public static GTweener GetTween(object target)
        {
            return TweenManager.GetTween(target, TweenPropType.None);
        }


        /// <param name="target"></param>
        /// <param name="propType"></param>
        /// <returns></returns>
        public static GTweener GetTween(object target, TweenPropType propType)
        {
            return TweenManager.GetTween(target, propType);
        }


        public static void Clean()
        {
            TweenManager.Clean();
        }
    }
}
