using System;
using System.Collections.Generic;
using UnityEngine;

namespace FairyGUI
{
    /// <summary>
    /// Window class.
    /// 窗口使用前首先要设置窗口中需要显示的内容，这通常是在编辑器里制作好的，可以直接使用Window.contentPane进行设置。
    /// 建议把设置contentPane等初始化操作放置到Window.onInit方法中。
    /// 另外，FairyGUI还提供了一套机制用于窗口动态创建。动态创建是指初始时仅指定窗口需要使用的资源，等窗口需要显示时才实际开始构建窗口的内容。
    /// 首先需要在窗口的构造函数中调用Window.addUISource。这个方法需要一个IUISource类型的参数，而IUISource是一个接口，
    /// 用户需要自行实现载入相关UI包的逻辑。当窗口第一次显示之前，IUISource的加载方法将会被调用，并等待载入完成后才返回执行Window.OnInit，然后窗口才会显示。
    /// 
    /// 如果你需要窗口显示时播放动画效果，那么覆盖doShowAnimation编写你的动画代码，并且在动画结束后调用onShown。覆盖onShown编写其他需要在窗口显示时处理的业务逻辑。
    /// 如果你需要窗口隐藏时播放动画效果，那么覆盖doHideAnimation编写你的动画代码，并且在动画结束时调用Window.hideImmediately（注意不是直接调用onHide！）。覆盖onHide编写其他需要在窗口隐藏时处理的业务逻辑。
    /// </summary>
    public class Window : GComponent
    {
        public bool bringToFontOnClick;

        private GComponent _frame;
        private GComponent _contentPane;
        private GObject _modalWaitPane;
        private GObject _closeButton;
        private GObject _dragArea;
        private GObject _contentArea;
        private bool _modal;

        private List<IUISource> _uiSources;
        private bool _inited;
        private bool _loading;

        protected int _requestingCmd;

#if FAIRYGUI_PUERTS
        public Action __onInit;
        public Action __onShown;
        public Action __onHide;
        public Action __doShowAnimation;
        public Action __doHideAnimation;
#endif

        public Window()
            : base()
        {
            _uiSources = new List<IUISource>();
            tabStopChildren = true;
            bringToFontOnClick = UIConfig.bringWindowToFrontOnClick;

            displayObject.onAddedToStage.Add(__addedToStage);
            displayObject.onRemovedFromStage.Add(__removeFromStage);
            displayObject.onTouchBegin.AddCapture(__touchBegin);

            gameObjectName = "Window";
            SetHome(GRoot.inst);
        }

        /// <summary>
        /// Set a UISource to this window. It must call before the window is shown. When the window is first time to show,
        /// UISource.Load is called. Only after all UISource is loaded, the window will continue to init.
        /// 为窗口添加一个源。这个方法建议在构造函数调用。当窗口第一次显示前，UISource的Load方法将被调用，然后只有所有的UISource
        /// 都ready后，窗口才会继续初始化和显示。
        /// </summary>
        /// <param name="source"></param>
        public void AddUISource(IUISource source)
        {
            _uiSources.Add(source);
        }

        public GComponent contentPane
        {
            set
            {
                if (_contentPane != value)
                {
                    if (_contentPane != null)
                        RemoveChild(_contentPane);
                    _contentPane = value;
                    if (_contentPane != null)
                    {
                        gameObjectName = "Window - " + _contentPane.gameObjectName;
                        _contentPane.gameObjectName = "ContentPane";

                        AddChild(_contentPane);
                        SetSize(_contentPane.width, _contentPane.height);
                        _contentPane.AddRelation(this, RelationType.Size);
                        _contentPane.fairyBatching = true;
                        _frame = _contentPane.GetChild("frame") as GComponent;
                        if (_frame != null)
                        {
                            closeButton = _frame.GetChild("closeButton");
                            dragArea = _frame.GetChild("dragArea");
                            contentArea = _frame.GetChild("contentArea");
                        }
                    }
                    else
                    {
                        _frame = null;
                        gameObjectName = "Window";
                    }
                }
            }
            get => _contentPane;
        }

        public GComponent frame => _frame;

        public GObject closeButton
        {
            get => _closeButton;
            set
            {
                if (_closeButton != null)
                    _closeButton.onClick.Remove(closeEventHandler);
                _closeButton = value;
                if (_closeButton != null)
                    _closeButton.onClick.Add(closeEventHandler);
            }
        }

        public GObject dragArea
        {
            get => _dragArea;
            set
            {
                if (_dragArea != value)
                {
                    if (_dragArea != null)
                    {
                        _dragArea.draggable = false;
                        _dragArea.onDragStart.Remove(__dragStart);
                    }

                    _dragArea = value;
                    if (_dragArea != null)
                    {
                        var graph = _dragArea as GGraph;
                        if (graph != null && graph.shape.isEmpty)
                            graph.DrawRect(_dragArea.width, _dragArea.height, 0, Color.clear, Color.clear);
                        _dragArea.draggable = true;
                        _dragArea.onDragStart.Add(__dragStart);
                    }
                }
            }
        }

        public GObject contentArea
        {
            get => _contentArea;
            set => _contentArea = value;
        }

        public GObject modalWaitingPane => _modalWaitPane;

        public void Show()
        {
            GRoot.inst.ShowWindow(this);
        }

        /// <param name="r"></param>
        public void ShowOn(GRoot r)
        {
            r.ShowWindow(this);
        }

        public void Hide()
        {
            if (isShowing)
                DoHideAnimation();
        }

        /// <summary>
        /// Hide window immediately, no OnHide will be called.
        /// </summary>
        public void HideImmediately()
        {
            root.HideWindowImmediately(this);
        }

        /// <summary>
        /// Make the window be center of the screen.
        /// </summary>
        /// <param name="r"></param>
        /// <param name="restraint">Add relations to ensure keeping center on screen size changed.</param>
        public void CenterOn(GRoot r, bool restraint)
        {
            SetXY((int) ((r.width - width) / 2), (int) ((r.height - height) / 2));
            if (restraint)
            {
                AddRelation(r, RelationType.Center_Center);
                AddRelation(r, RelationType.Middle_Middle);
            }
        }

        /// <summary>
        /// Switch show and hide status.
        /// </summary>
        public void ToggleStatus()
        {
            if (isTop)
                Hide();
            else
                Show();
        }

        public bool isShowing => parent != null;

        public bool isTop => parent != null && parent.GetChildIndex(this) == parent.numChildren - 1;

        public bool modal
        {
            get => _modal;
            set => _modal = value;
        }

        public void BringToFront()
        {
            root.BringToFront(this);
        }

        public void ShowModalWait()
        {
            ShowModalWait(0);
        }

        /// <summary>
        /// Display a modal waiting sign in the front.
        /// 显示一个等待标志在最前面。等待标志的资源可以通过UIConfig.windowModalWaiting。等待标志组件会设置为屏幕大小，请内部做好关联。
        /// 还可以设定一个requestingCmd作为等待的命令字，在CloseModalWait里传入相同的命令字ModalWait将结束，否则CloseModalWait无效。
        /// </summary>
        /// <param name="requestingCmd"></param>
        public void ShowModalWait(int requestingCmd)
        {
            if (requestingCmd != 0)
                _requestingCmd = requestingCmd;

            if (UIConfig.windowModalWaiting != null)
            {
                if (_modalWaitPane == null)
                {
                    _modalWaitPane = UIPackage.CreateObjectFromURL(UIConfig.windowModalWaiting);
                    _modalWaitPane.SetHome(this);
                }

                LayoutModalWaitPane();

                AddChild(_modalWaitPane);
            }
        }

        protected virtual void LayoutModalWaitPane()
        {
            if (_contentArea != null)
            {
                var pt = _frame.LocalToGlobal(Vector2.zero);
                pt = GlobalToLocal(pt);
                _modalWaitPane.SetXY((int) pt.x + _contentArea.x, (int) pt.y + _contentArea.y);
                _modalWaitPane.SetSize(_contentArea.width, _contentArea.height);
            }
            else
            {
                _modalWaitPane.SetSize(width, height);
            }
        }

        /// <returns></returns>
        public bool CloseModalWait()
        {
            return CloseModalWait(0);
        }

        /// <summary>
        /// Close modal waiting. If rquestingCmd is equal to the value you transfer in ShowModalWait, mowal wait will be closed.
        /// Otherwise, this function has no effect.
        /// 关闭模式等待。如果requestingCmd和ShowModalWait传入的不相同，则这个函数没有任何动作，立即返回。
        /// </summary>
        /// <param name="requestingCmd"></param>
        /// <returns></returns>
        public bool CloseModalWait(int requestingCmd)
        {
            if (requestingCmd != 0)
                if (_requestingCmd != requestingCmd)
                    return false;
            _requestingCmd = 0;

            if (_modalWaitPane != null && _modalWaitPane.parent != null)
                RemoveChild(_modalWaitPane);

            return true;
        }

        public bool modalWaiting => _modalWaitPane != null && _modalWaitPane.inContainer;

        public void Init()
        {
            if (_inited || _loading)
                return;

            if (_uiSources.Count > 0)
            {
                _loading = false;
                var cnt = _uiSources.Count;
                for (var i = 0; i < cnt; i++)
                {
                    var lib = _uiSources[i];
                    if (!lib.loaded)
                    {
                        lib.Load(__uiLoadComplete);
                        _loading = true;
                    }
                }

                if (!_loading)
                    _init();
            }
            else
            {
                _init();
            }
        }

        protected virtual void OnInit()
        {
#if FAIRYGUI_TOLUA
            CallLua("OnInit");
#endif
#if FAIRYGUI_PUERTS
            if (__onInit != null)
                __onInit();
#endif
        }

        protected virtual void OnShown()
        {
#if FAIRYGUI_TOLUA
            CallLua("OnShown");
#endif
#if FAIRYGUI_PUERTS
            if (__onShown != null)
                __onShown();
#endif
        }

        protected virtual void OnHide()
        {
#if FAIRYGUI_TOLUA
            CallLua("OnHide");
#endif
#if FAIRYGUI_PUERTS
            if (__onHide != null)
                __onHide();
#endif
        }

        protected virtual void DoShowAnimation()
        {
#if FAIRYGUI_TOLUA
            if (!CallLua("DoShowAnimation"))
                OnShown();
#elif FAIRYGUI_PUERTS
            if (__doShowAnimation != null)
                __doShowAnimation();
            else
                OnShown();
#else
            OnShown();
#endif
        }

        protected virtual void DoHideAnimation()
        {
#if FAIRYGUI_TOLUA
            if (!CallLua("DoHideAnimation"))
                HideImmediately();
#elif FAIRYGUI_PUERTS
            if (__doHideAnimation != null)
                __doHideAnimation();
            else
                HideImmediately();
#else
            HideImmediately();
#endif
        }

        private void __uiLoadComplete()
        {
            var cnt = _uiSources.Count;
            for (var i = 0; i < cnt; i++)
            {
                var lib = _uiSources[i];
                if (!lib.loaded)
                    return;
            }

            _loading = false;
            _init();
        }

        private void _init()
        {
            _inited = true;
            OnInit();

            if (isShowing)
                DoShowAnimation();
        }

        public override void Dispose()
        {
            if (_modalWaitPane != null && _modalWaitPane.parent == null)
                _modalWaitPane.Dispose();

#if FAIRYGUI_PUERTS
            __onInit = null;
            __onShown = null;
            __onHide = null;
            __doShowAnimation = null;
            __doHideAnimation = null;
#endif

            base.Dispose();
        }

        protected virtual void closeEventHandler(EventContext context)
        {
            Hide();
        }

        private void __addedToStage()
        {
            if (!_inited)
                Init();
            else
                DoShowAnimation();
        }

        private void __removeFromStage()
        {
            CloseModalWait();
            OnHide();
        }

        private void __touchBegin(EventContext context)
        {
            if (isShowing && bringToFontOnClick) BringToFront();
        }

        private void __dragStart(EventContext context)
        {
            context.PreventDefault();

            StartDrag((int) context.data);
        }
    }
}