using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FairyGUI
{
    public interface EMRenderTarget
    {
        int EM_sortingOrder { get; }

        void EM_BeforeUpdate();
        void EM_Update(UpdateContext context);
        void EM_Reload();
    }

    /// <summary>
    /// 这是一个在编辑状态下渲染UI的功能类。EM=Edit Mode
    /// </summary>
    public class EMRenderSupport
    {
        public static bool orderChanged;

        private static UpdateContext _updateContext;
        private static List<EMRenderTarget> _targets = new List<EMRenderTarget>();

        public static bool packageListReady { get; private set; }

        public static bool hasTarget => _targets.Count > 0;

        /// <param name="value"></param>
        public static void Add(EMRenderTarget value)
        {
            if (!_targets.Contains(value))
                _targets.Add(value);
            orderChanged = true;
        }

        /// <param name="value"></param>
        public static void Remove(EMRenderTarget value)
        {
            _targets.Remove(value);
        }

        /// <summary>
        /// 由StageCamera调用
        /// </summary>
        public static void Update()
        {
            if (Application.isPlaying)
                return;

            if (_updateContext == null)
                _updateContext = new UpdateContext();

            if (orderChanged)
            {
                _targets.Sort(CompareDepth);
                orderChanged = false;
            }

            var cnt = _targets.Count;
            for (var i = 0; i < cnt; i++)
            {
                var panel = _targets[i];
                panel.EM_BeforeUpdate();
            }

            if (packageListReady)
            {
                _updateContext.Begin();
                for (var i = 0; i < cnt; i++)
                {
                    var panel = _targets[i];
                    panel.EM_Update(_updateContext);
                }

                _updateContext.End();
            }
        }

        /// <summary>
        /// 当发生二进制重载时，或用户点击刷新菜单
        /// </summary>
        public static void Reload()
        {
            if (Application.isPlaying)
                return;

            UIConfig.ClearResourceRefs();
            var configs = Object.FindObjectsOfType<UIConfig>();
            foreach (var config in configs)
                config.Load();

            packageListReady = true;

            var cnt = _targets.Count;
            for (var i = 0; i < cnt; i++)
            {
                var panel = _targets[i];
                panel.EM_Reload();
            }
        }

        private static int CompareDepth(EMRenderTarget c1, EMRenderTarget c2)
        {
            return c1.EM_sortingOrder - c2.EM_sortingOrder;
        }
    }
}