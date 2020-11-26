using System;
using UnityEngine;

namespace FairyGUI
{

    public class NAudioClip
    {
        public static Action<AudioClip> CustomDestroyMethod;


        public DestroyMethod destroyMethod;


        public AudioClip nativeClip;


        /// <param name="audioClip"></param>
        public NAudioClip(AudioClip audioClip)
        {
            nativeClip = audioClip;
        }


        public void Unload()
        {
            if (nativeClip == null)
                return;

            if (destroyMethod == DestroyMethod.Unload)
                Resources.UnloadAsset(nativeClip);
            else if (destroyMethod == DestroyMethod.Destroy)
                UnityEngine.Object.DestroyImmediate(nativeClip, true);
            else if (destroyMethod == DestroyMethod.Custom)
            {
                if (CustomDestroyMethod == null)
                    Debug.LogWarning("NAudioClip.CustomDestroyMethod must be set to handle DestroyMethod.Custom");
                else
                    CustomDestroyMethod(nativeClip);
            }

            nativeClip = null;
        }


        /// <param name="audioClip"></param>
        public void Reload(AudioClip audioClip)
        {
            if (nativeClip != null && nativeClip != audioClip)
                Unload();

            nativeClip = audioClip;
        }
    }
}
