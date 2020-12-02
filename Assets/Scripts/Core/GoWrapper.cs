using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace FairyGUI
{
    /// <summary>
    /// GoWrapper is class for wrapping common gameobject into UI display list.
    /// </summary>
    public class GoWrapper : DisplayObject
    {
        [Obsolete("No need to manually set this flag anymore, coz it will be handled automatically.")]
        public bool supportStencil;

        public event Action<UpdateContext> onUpdate;

        protected GameObject _wrapTarget;
        protected List<RendererInfo> _renderers;
        protected Dictionary<Material, Material> _materialsBackup;
        protected Canvas _canvas;
        protected bool _cloneMaterial;
        protected bool _shouldCloneMaterial;

        protected struct RendererInfo
        {
            public Renderer renderer;
            public Material[] materials;
            public int sortingOrder;
        }

        protected static List<Transform> helperTransformList = new List<Transform>();

        public GoWrapper()
        {
            _flags |= Flags.SkipBatching;

            _renderers = new List<RendererInfo>();
            _materialsBackup = new Dictionary<Material, Material>();

            CreateGameObject("GoWrapper");
        }

        /// <param name="go">包装对象。</param>
        public GoWrapper(GameObject go) : this()
        {
            SetWrapTarget(go, false);
        }

        /// <summary>
        /// 设置包装对象。注意如果原来有包装对象，设置新的包装对象后，原来的包装对象只会被删除引用，但不会被销毁。
        /// 对象包含的所有材质不会被复制，如果材质已经是公用的，这可能影响到其他对象。如果希望自动复制，改为使用SetWrapTarget(target, true)设置。
        /// </summary>
        public GameObject wrapTarget
        {
            get => _wrapTarget;
            set => SetWrapTarget(value, false);
        }

        [Obsolete("setWrapTarget is deprecated. Use SetWrapTarget instead.")]
        public void setWrapTarget(GameObject target, bool cloneMaterial)
        {
            SetWrapTarget(target, cloneMaterial);
        }

        /// <summary>
        ///  设置包装对象。注意如果原来有包装对象，设置新的包装对象后，原来的包装对象只会被删除引用，但不会被销毁。
        /// </summary>
        /// <param name="target"></param>
        /// <param name="cloneMaterial">如果true，则复制材质，否则直接使用sharedMaterial。</param>
        public void SetWrapTarget(GameObject target, bool cloneMaterial)
        {
            RecoverMaterials();

            _cloneMaterial = cloneMaterial;
            if (_wrapTarget != null)
                _wrapTarget.transform.SetParent(null, false);

            _canvas = null;
            _wrapTarget = target;
            _shouldCloneMaterial = false;
            _renderers.Clear();

            if (_wrapTarget != null)
            {
                _wrapTarget.transform.SetParent(cachedTransform, false);
                _canvas = _wrapTarget.GetComponent<Canvas>();
                if (_canvas != null)
                {
                    _canvas.renderMode = RenderMode.WorldSpace;
                    _canvas.worldCamera = StageCamera.main;
                    _canvas.overrideSorting = true;

                    var rt = _canvas.GetComponent<RectTransform>();
                    rt.pivot = new Vector2(0, 1);
                    rt.position = new Vector3(0, 0, 0);
                    SetSize(rt.rect.width, rt.rect.height);
                }
                else
                {
                    CacheRenderers();
                    SetSize(0, 0);
                }

                SetGoLayers(layer);
            }
        }

        /// <summary>
        /// GoWrapper will cache all renderers of your gameobject on constructor. 
        /// If your gameobject change laterly, call this function to update the cache.
        /// GoWrapper会在构造函数里查询你的gameobject所有的Renderer并保存。如果你的gameobject
        /// 后续发生了改变，调用这个函数通知GoWrapper重新查询和保存。
        /// </summary>
        public void CacheRenderers()
        {
            if (_canvas != null)
                return;

            RecoverMaterials();
            _renderers.Clear();

            var items = _wrapTarget.GetComponentsInChildren<Renderer>(true);

            var cnt = items.Length;
            _renderers.Capacity = cnt;
            for (var i = 0; i < cnt; i++)
            {
                var r = items[i];
                var mats = r.sharedMaterials;
                var ri = new RendererInfo()
                {
                    renderer = r,
                    materials = mats,
                    sortingOrder = r.sortingOrder
                };
                _renderers.Add(ri);
            }

            _renderers.Sort((RendererInfo c1, RendererInfo c2) => { return c1.sortingOrder - c2.sortingOrder; });

            _shouldCloneMaterial = true;
        }

        private void CloneMaterials()
        {
            _shouldCloneMaterial = false;

            var cnt = _renderers.Count;
            for (var i = 0; i < cnt; i++)
            {
                var ri = _renderers[i];
                var mats = ri.materials;
                if (mats == null)
                    continue;

                var shouldSetRQ = ri.renderer is SkinnedMeshRenderer || ri.renderer is MeshRenderer;

                var mcnt = mats.Length;
                for (var j = 0; j < mcnt; j++)
                {
                    var mat = mats[j];
                    if (mat == null)
                        continue;

                    if (shouldSetRQ && mat.renderQueue != 3000
                    ) //Set the object rendering in Transparent Queue as UI objects
                        mat.renderQueue = 3000;

                    //确保相同的材质不会复制两次
                    Material newMat;
                    if (!_materialsBackup.TryGetValue(mat, out newMat))
                    {
                        newMat = new Material(mat);
                        _materialsBackup[mat] = newMat;
                    }

                    mats[j] = newMat;
                }

                if (ri.renderer != null)
                    ri.renderer.sharedMaterials = mats;
            }
        }

        private void RecoverMaterials()
        {
            if (_materialsBackup.Count == 0)
                return;

            var cnt = _renderers.Count;
            for (var i = 0; i < cnt; i++)
            {
                var ri = _renderers[i];
                if (ri.renderer == null)
                    continue;

                var mats = ri.materials;
                if (mats == null)
                    continue;

                var mcnt = mats.Length;
                for (var j = 0; j < mcnt; j++)
                {
                    var mat = mats[j];

                    foreach (var kv in _materialsBackup)
                        if (kv.Value == mat)
                            mats[j] = kv.Key;
                }

                ri.renderer.sharedMaterials = mats;
            }

            foreach (var kv in _materialsBackup)
                Object.DestroyImmediate(kv.Value);

            _materialsBackup.Clear();
        }

        public override int renderingOrder
        {
            get => base.renderingOrder;
            set
            {
                base.renderingOrder = value;

                if (_canvas != null)
                {
                    _canvas.sortingOrder = value;
                }
                else
                {
                    var cnt = _renderers.Count;
                    for (var i = 0; i < cnt; i++)
                    {
                        var ri = _renderers[i];
                        if (ri.renderer != null)
                        {
                            if (i != 0 && _renderers[i].sortingOrder != _renderers[i - 1].sortingOrder)
                                value = UpdateContext.current.renderingOrder++;
                            ri.renderer.sortingOrder = value;
                        }
                    }
                }
            }
        }

        protected override bool SetLayer(int value, bool fromParent)
        {
            if (base.SetLayer(value, fromParent))
            {
                SetGoLayers(value);
                return true;
            }
            else
            {
                return false;
            }
        }

        protected void SetGoLayers(int layer)
        {
            if (_wrapTarget == null)
                return;

            _wrapTarget.GetComponentsInChildren<Transform>(true, helperTransformList);
            var cnt = helperTransformList.Count;
            for (var i = 0; i < cnt; i++)
                helperTransformList[i].gameObject.layer = layer;
            helperTransformList.Clear();
        }

        public override void Update(UpdateContext context)
        {
            if (onUpdate != null)
                onUpdate(context);

            if (_shouldCloneMaterial)
                CloneMaterials();

            ApplyClipping(context);

            base.Update(context);
        }

        private List<Material> helperMaterials = new List<Material>();

        protected virtual void ApplyClipping(UpdateContext context)
        {
#if UNITY_2018_2_OR_NEWER
            var cnt = _renderers.Count;
            for (var i = 0; i < cnt; i++)
            {
                var renderer = _renderers[i].renderer;
                if (renderer == null)
                    continue;

                renderer.GetMaterials(helperMaterials);

                var cnt2 = helperMaterials.Count;
                for (var j = 0; j < cnt2; j++)
                {
                    var mat = helperMaterials[j];
                    if (mat != null)
                        context.ApplyClippingProperties(mat, false);
                }

                helperMaterials.Clear();
            }
#else
            int cnt = _renderers.Count;
            for (int i = 0; i < cnt; i++)
            {
                Material[] mats = _renderers[i].materials;
                if (mats == null)
                    continue;
                
                int cnt2 = mats.Length;
                for (int j = 0; j < cnt2; j++)
                {
                    Material mat = mats[j];
                    if (mat != null)
                        context.ApplyClippingProperties(mat, false);
                }
            }
#endif
        }

        public override void Dispose()
        {
            if ((_flags & Flags.Disposed) != 0)
                return;

            if (_wrapTarget != null)
            {
                Object.Destroy(_wrapTarget);
                _wrapTarget = null;

                if (_materialsBackup.Count > 0)
                    //如果有备份，说明材质是复制出来的，应该删除
                    foreach (var kv in _materialsBackup)
                        Object.DestroyImmediate(kv.Value);
            }

            _renderers = null;
            _materialsBackup = null;
            _canvas = null;

            base.Dispose();
        }
    }
}