using UnityEngine;
using FairyGUI.Utils;

namespace FairyGUI
{
    public partial class GLoader3D : GObject, IAnimationGear, IColorGear
    {
        private string _url;
        private AlignType _align;
        private VertAlignType _verticalAlign;
        private bool _autoSize;
        private FillType _fill;
        private bool _shrinkOnly;
        private string _animationName;
        private string _skinName;
        private bool _playing;
        private int _frame;
        private bool _loop;
        private bool _updatingLayout;
        private Color _color;

        protected PackageItem _contentItem;
        protected GoWrapper _content;

        public GLoader3D()
        {
            _url = string.Empty;
            _align = AlignType.Left;
            _verticalAlign = VertAlignType.Top;
            _playing = true;
            _color = Color.white;
        }

        protected override void CreateDisplayObject()
        {
            displayObject = new Container("GLoader3D");
            displayObject.gOwner = this;

            _content = new GoWrapper();
            _content.onUpdate += OnUpdateContent;
            ((Container) displayObject).AddChild(_content);
            ((Container) displayObject).opaque = true;
        }

        public override void Dispose()
        {
            _content.Dispose();
            base.Dispose();
        }

        public string url
        {
            get => _url;
            set
            {
                if (_url == value)
                    return;

                ClearContent();
                _url = value;
                LoadContent();
                UpdateGear(7);
            }
        }

        public override string icon
        {
            get => _url;
            set => url = value;
        }

        public AlignType align
        {
            get => _align;
            set
            {
                if (_align != value)
                {
                    _align = value;
                    UpdateLayout();
                }
            }
        }

        public VertAlignType verticalAlign
        {
            get => _verticalAlign;
            set
            {
                if (_verticalAlign != value)
                {
                    _verticalAlign = value;
                    UpdateLayout();
                }
            }
        }

        public FillType fill
        {
            get => _fill;
            set
            {
                if (_fill != value)
                {
                    _fill = value;
                    UpdateLayout();
                }
            }
        }

        public bool shrinkOnly
        {
            get => _shrinkOnly;
            set
            {
                if (_shrinkOnly != value)
                {
                    _shrinkOnly = value;
                    UpdateLayout();
                }
            }
        }

        public bool autoSize
        {
            get => _autoSize;
            set
            {
                if (_autoSize != value)
                {
                    _autoSize = value;
                    UpdateLayout();
                }
            }
        }

        public bool playing
        {
            get => _playing;
            set
            {
                if (_playing != value)
                {
                    _playing = value;
                    OnChange("playing");
                    UpdateGear(5);
                }
            }
        }

        public int frame
        {
            get => _frame;
            set
            {
                if (_frame != value)
                {
                    _frame = value;
                    OnChange("frame");
                    UpdateGear(5);
                }
            }
        }

        /// <summary>
        /// Not implemented
        /// </summary>
        public float timeScale { get; set; }

        /// <summary>
        /// Not implemented
        /// </summary>
        public bool ignoreEngineTimeScale { get; set; }

        /// <summary>
        /// Not implemented
        /// </summary>
        /// <param name="time"></param>
        public void Advance(float time)
        {
        }

        public bool loop
        {
            get => _loop;
            set
            {
                if (_loop != value)
                {
                    _loop = value;
                    OnChange("loop");
                }
            }
        }

        /// <value></value>
        public string animationName
        {
            get => _animationName;
            set
            {
                _animationName = value;
                OnChange("animationName");
            }
        }

        /// <value></value>
        public string skinName
        {
            get => _skinName;
            set
            {
                _skinName = value;
                OnChange("skinName");
            }
        }

        public Material material
        {
            get => _content.material;
            set => _content.material = value;
        }

        public string shader
        {
            get => _content.shader;
            set => _content.shader = value;
        }

        public Color color
        {
            get => _color;
            set
            {
                if (_color != value)
                {
                    _color = value;
                    UpdateGear(4);

                    OnChange("color");
                }
            }
        }

        public GameObject wrapTarget => _content.wrapTarget;

        /// <param name="gameObject"></param>
        /// <param name="cloneMaterial"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void SetWrapTarget(GameObject gameObject, bool cloneMaterial, int width, int height)
        {
            _content.SetWrapTarget(gameObject, cloneMaterial);
            _content.SetSize(width, height);
            sourceWidth = width;
            sourceHeight = height;

            UpdateLayout();
        }

        public override IFilter filter
        {
            get => _content.filter;
            set => _content.filter = value;
        }

        public override BlendMode blendMode
        {
            get => _content.blendMode;
            set => _content.blendMode = value;
        }

        protected void LoadContent()
        {
            ClearContent();

            if (string.IsNullOrEmpty(_url))
                return;

            _contentItem = UIPackage.GetItemByURL(_url);

            if (_contentItem != null)
            {
                _contentItem = _contentItem.getBranch();
                _contentItem = _contentItem.getHighResolution();
                _contentItem.Load();

                if (_contentItem.type == PackageItemType.Spine)
                {
#if FAIRYGUI_SPINE
                    LoadSpine();
#endif
                }
                else if (_contentItem.type == PackageItemType.DragoneBones)
                {
#if FAIRYGUI_DRAGONBONES
                    LoadDragonBones();
#endif
                }
            }
            else
            {
                LoadExternal();
            }
        }

        protected virtual void OnChange(string propertyName)
        {
            if (_contentItem == null)
                return;

            if (_contentItem.type == PackageItemType.Spine)
            {
#if FAIRYGUI_SPINE
                OnChangeSpine(propertyName);
#endif
            }
            else if (_contentItem.type == PackageItemType.DragoneBones)
            {
#if FAIRYGUI_DRAGONBONES
                OnChangeDragonBones(propertyName);
#endif
            }
        }

        protected virtual void LoadExternal()
        {
        }

        protected virtual void FreeExternal()
        {
            Object.DestroyImmediate(_content.wrapTarget);
        }

        protected void UpdateLayout()
        {
            float contentWidth = sourceWidth;
            float contentHeight = sourceHeight;

            if (_autoSize)
            {
                _updatingLayout = true;
                if (contentWidth == 0)
                    contentWidth = 50;
                if (contentHeight == 0)
                    contentHeight = 30;
                SetSize(contentWidth, contentHeight);

                _updatingLayout = false;

                if (_width == contentWidth && _height == contentHeight)
                {
                    _content.SetXY(0, 0);
                    _content.SetScale(1, 1);

                    InvalidateBatchingState();
                    return;
                }

                //如果不相等，可能是由于大小限制造成的，要后续处理
            }

            float sx = 1, sy = 1;
            if (_fill != FillType.None)
            {
                sx = width / sourceWidth;
                sy = height / sourceHeight;

                if (sx != 1 || sy != 1)
                {
                    if (_fill == FillType.ScaleMatchHeight)
                    {
                        sx = sy;
                    }
                    else if (_fill == FillType.ScaleMatchWidth)
                    {
                        sy = sx;
                    }
                    else if (_fill == FillType.Scale)
                    {
                        if (sx > sy)
                            sx = sy;
                        else
                            sy = sx;
                    }
                    else if (_fill == FillType.ScaleNoBorder)
                    {
                        if (sx > sy)
                            sy = sx;
                        else
                            sx = sy;
                    }

                    if (_shrinkOnly)
                    {
                        if (sx > 1)
                            sx = 1;
                        if (sy > 1)
                            sy = 1;
                    }

                    contentWidth = sourceWidth * sx;
                    contentHeight = sourceHeight * sy;
                }
            }

            _content.SetScale(sx, sy);

            float nx;
            float ny;
            if (_align == AlignType.Center)
                nx = (width - contentWidth) / 2;
            else if (_align == AlignType.Right)
                nx = width - contentWidth;
            else
                nx = 0;
            if (_verticalAlign == VertAlignType.Middle)
                ny = (height - contentHeight) / 2;
            else if (_verticalAlign == VertAlignType.Bottom)
                ny = height - contentHeight;
            else
                ny = 0;
            _content.SetXY(nx, ny);

            InvalidateBatchingState();
        }

        protected void ClearContent()
        {
            if (_content.wrapTarget != null)
            {
                if (_contentItem != null)
                {
                    if (_contentItem.type == PackageItemType.Spine)
                    {
#if FAIRYGUI_SPINE
                        FreeSpine();
#endif
                    }
                    else if (_contentItem.type == PackageItemType.DragoneBones)
                    {
#if FAIRYGUI_DRAGONBONES
                        FreeDragonBones();
#endif
                    }
                }
                else
                {
                    FreeExternal();
                }
            }

            _content.wrapTarget = null;
            _contentItem = null;
        }

        protected void OnUpdateContent(UpdateContext context)
        {
            if (_contentItem == null)
                return;

            if (_contentItem.type == PackageItemType.Spine)
            {
#if FAIRYGUI_SPINE
                OnUpdateSpine(context);
#endif
            }
            else if (_contentItem.type == PackageItemType.DragoneBones)
            {
#if FAIRYGUI_DRAGONBONES
                OnUpdateDragonBones(context);
#endif
            }
        }

        protected override void HandleSizeChanged()
        {
            base.HandleSizeChanged();

            if (!_updatingLayout)
                UpdateLayout();
        }

        public override void Setup_BeforeAdd(ByteBuffer buffer, int beginPos)
        {
            base.Setup_BeforeAdd(buffer, beginPos);

            buffer.Seek(beginPos, 5);

            _url = buffer.ReadS();
            _align = (AlignType) buffer.ReadByte();
            _verticalAlign = (VertAlignType) buffer.ReadByte();
            _fill = (FillType) buffer.ReadByte();
            _shrinkOnly = buffer.ReadBool();
            _autoSize = buffer.ReadBool();
            _animationName = buffer.ReadS();
            _skinName = buffer.ReadS();
            _playing = buffer.ReadBool();
            _frame = buffer.ReadInt();
            _loop = buffer.ReadBool();

            if (buffer.ReadBool())
                color = buffer.ReadColor(); //color

            if (!string.IsNullOrEmpty(_url))
                LoadContent();
        }
    }
}