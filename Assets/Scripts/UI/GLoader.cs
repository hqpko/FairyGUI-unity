using System;
using UnityEngine;
using FairyGUI.Utils;

namespace FairyGUI
{
    /// <summary>
    /// GLoader class
    /// </summary>
    public class GLoader : GObject, IAnimationGear, IColorGear
    {
        /// <summary>
        /// Display an error sign if the loader fails to load the content.
        /// UIConfig.loaderErrorSign muse be set.
        /// </summary>
        public bool showErrorSign;

        private string _url;
        private AlignType _align;
        private VertAlignType _verticalAlign;
        private bool _autoSize;
        private FillType _fill;
        private bool _shrinkOnly;
        private bool _updatingLayout;
        private PackageItem _contentItem;
        private Action<NTexture> _reloadDelegate;

        private MovieClip _content;
        private GObject _errorSign;
        private GComponent _content2;

        public GLoader()
        {
            _url = string.Empty;
            _align = AlignType.Left;
            _verticalAlign = VertAlignType.Top;
            showErrorSign = true;
            _reloadDelegate = OnExternalReload;
        }

        protected override void CreateDisplayObject()
        {
            displayObject = new Container("GLoader");
            displayObject.gOwner = this;
            _content = new MovieClip();
            ((Container) displayObject).AddChild(_content);
            ((Container) displayObject).opaque = true;
        }

        public override void Dispose()
        {
            if (_content.texture != null)
                if (_contentItem == null)
                {
                    _content.texture.onSizeChanged -= _reloadDelegate;
                    try
                    {
                        FreeExternal(_content.texture);
                    }
                    catch (Exception err)
                    {
                        Debug.LogWarning(err);
                    }
                }

            if (_errorSign != null)
                _errorSign.Dispose();
            if (_content2 != null)
                _content2.Dispose();
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
            get => _content.playing;
            set
            {
                _content.playing = value;
                UpdateGear(5);
            }
        }

        public int frame
        {
            get => _content.frame;
            set
            {
                _content.frame = value;
                UpdateGear(5);
            }
        }

        public float timeScale
        {
            get => _content.timeScale;
            set => _content.timeScale = value;
        }

        public bool ignoreEngineTimeScale
        {
            get => _content.ignoreEngineTimeScale;
            set => _content.ignoreEngineTimeScale = value;
        }

        /// <param name="time"></param>
        public void Advance(float time)
        {
            _content.Advance(time);
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
            get => _content.color;
            set
            {
                if (_content.color != value)
                {
                    _content.color = value;
                    UpdateGear(4);
                }
            }
        }

        public FillMethod fillMethod
        {
            get => _content.fillMethod;
            set => _content.fillMethod = value;
        }

        public int fillOrigin
        {
            get => _content.fillOrigin;
            set => _content.fillOrigin = value;
        }

        public bool fillClockwise
        {
            get => _content.fillClockwise;
            set => _content.fillClockwise = value;
        }

        public float fillAmount
        {
            get => _content.fillAmount;
            set => _content.fillAmount = value;
        }

        public Image image => _content;

        public MovieClip movieClip => _content;

        public GComponent component => _content2;

        public NTexture texture
        {
            get => _content.texture;

            set
            {
                url = null;

                _content.texture = value;
                if (value != null)
                {
                    sourceWidth = value.width;
                    sourceHeight = value.height;
                }
                else
                {
                    sourceWidth = sourceHeight = 0;
                }

                UpdateLayout();
            }
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

            if (_url.StartsWith(UIPackage.URL_PREFIX))
                LoadFromPackage(_url);
            else
                LoadExternal();
        }

        protected void LoadFromPackage(string itemURL)
        {
            _contentItem = UIPackage.GetItemByURL(itemURL);

            if (_contentItem != null)
            {
                _contentItem = _contentItem.getBranch();
                sourceWidth = _contentItem.width;
                sourceHeight = _contentItem.height;
                _contentItem = _contentItem.getHighResolution();
                _contentItem.Load();

                if (_contentItem.type == PackageItemType.Image)
                {
                    _content.texture = _contentItem.texture;
                    _content.textureScale = new Vector2(_contentItem.width / (float) sourceWidth,
                        _contentItem.height / (float) sourceHeight);
                    _content.scale9Grid = _contentItem.scale9Grid;
                    _content.scaleByTile = _contentItem.scaleByTile;
                    _content.tileGridIndice = _contentItem.tileGridIndice;

                    UpdateLayout();
                }
                else if (_contentItem.type == PackageItemType.MovieClip)
                {
                    _content.interval = _contentItem.interval;
                    _content.swing = _contentItem.swing;
                    _content.repeatDelay = _contentItem.repeatDelay;
                    _content.frames = _contentItem.frames;

                    UpdateLayout();
                }
                else if (_contentItem.type == PackageItemType.Component)
                {
                    var obj = UIPackage.CreateObjectFromURL(itemURL);
                    if (obj == null)
                    {
                        SetErrorState();
                    }
                    else if (!(obj is GComponent))
                    {
                        obj.Dispose();
                        SetErrorState();
                    }
                    else
                    {
                        _content2 = (GComponent) obj;
                        ((Container) displayObject).AddChild(_content2.displayObject);
                        UpdateLayout();
                    }
                }
                else
                {
                    if (_autoSize)
                        SetSize(_contentItem.width, _contentItem.height);

                    SetErrorState();

                    Debug.LogWarning("Unsupported type of GLoader: " + _contentItem.type);
                }
            }
            else
            {
                SetErrorState();
            }
        }

        protected virtual void LoadExternal()
        {
            var tex = (Texture2D) Resources.Load(_url, typeof(Texture2D));
            if (tex != null)
                onExternalLoadSuccess(new NTexture(tex));
            else
                onExternalLoadFailed();
        }

        protected virtual void FreeExternal(NTexture texture)
        {
        }

        protected void onExternalLoadSuccess(NTexture texture)
        {
            _content.texture = texture;
            sourceWidth = texture.width;
            sourceHeight = texture.height;
            _content.scale9Grid = null;
            _content.scaleByTile = false;
            texture.onSizeChanged += _reloadDelegate;
            UpdateLayout();
        }

        protected void onExternalLoadFailed()
        {
            SetErrorState();
        }

        private void OnExternalReload(NTexture texture)
        {
            sourceWidth = texture.width;
            sourceHeight = texture.height;
            UpdateLayout();
        }

        private void SetErrorState()
        {
            if (!showErrorSign || !Application.isPlaying)
                return;

            if (_errorSign == null)
            {
                if (UIConfig.loaderErrorSign != null)
                    _errorSign = UIPackage.CreateObjectFromURL(UIConfig.loaderErrorSign);
                else
                    return;
            }

            if (_errorSign != null)
            {
                _errorSign.SetSize(width, height);
                ((Container) displayObject).AddChild(_errorSign.displayObject);
            }
        }

        protected void ClearErrorState()
        {
            if (_errorSign != null && _errorSign.displayObject.parent != null)
                ((Container) displayObject).RemoveChild(_errorSign.displayObject);
        }

        protected void UpdateLayout()
        {
            if (_content2 == null && _content.texture == null && _content.frames == null)
            {
                if (_autoSize)
                {
                    _updatingLayout = true;
                    SetSize(50, 30);
                    _updatingLayout = false;
                }

                return;
            }

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
                    if (_content2 != null)
                    {
                        _content2.SetXY(0, 0);
                        _content2.SetScale(1, 1);
                    }
                    else
                    {
                        _content.SetXY(0, 0);
                        _content.SetSize(contentWidth, contentHeight);
                    }

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

            if (_content2 != null)
                _content2.SetScale(sx, sy);
            else
                _content.size = new Vector2(contentWidth, contentHeight);

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
            if (_content2 != null)
                _content2.SetXY(nx, ny);
            else
                _content.SetXY(nx, ny);

            InvalidateBatchingState();
        }

        private void ClearContent()
        {
            ClearErrorState();

            if (_content.texture != null)
            {
                if (_contentItem == null)
                {
                    _content.texture.onSizeChanged -= _reloadDelegate;
                    FreeExternal(_content.texture);
                }

                _content.texture = null;
            }

            _content.frames = null;

            if (_content2 != null)
            {
                _content2.Dispose();
                _content2 = null;
            }

            _contentItem = null;
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
            showErrorSign = buffer.ReadBool();
            _content.playing = buffer.ReadBool();
            _content.frame = buffer.ReadInt();

            if (buffer.ReadBool())
                _content.color = buffer.ReadColor();
            _content.fillMethod = (FillMethod) buffer.ReadByte();
            if (_content.fillMethod != FillMethod.None)
            {
                _content.fillOrigin = buffer.ReadByte();
                _content.fillClockwise = buffer.ReadBool();
                _content.fillAmount = buffer.ReadFloat();
            }

            if (!string.IsNullOrEmpty(_url))
                LoadContent();
        }
    }
}