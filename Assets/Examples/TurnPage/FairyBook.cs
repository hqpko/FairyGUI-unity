using System;
using FairyGUI;
using FairyGUI.Utils;
using UnityEngine;

/// <summary>
/// Achieving the effect of turning over books. Use virtual mechanism to support unlimited pages. Support covers.
/// </summary>
public class FairyBook : GComponent
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="index"></param>
    /// <param name="item"></param>
    public delegate void PageRenderer(int index, GComponent page);

    /// <summary>
    /// 
    /// </summary>
    public PageRenderer pageRenderer;

    /// <summary>
    /// 
    /// </summary>
    public static float EffectDuration = 0.5f;

    /// <summary>
    /// 
    /// </summary>
    public enum Paper
    {
        Soft,
        Hard
    }

    public enum CoverType
    {
        Front,
        Back
    }

    private enum CoverStatus
    {
        Hidden,
        ShowingFront,
        ShowingBack
    }

    private enum CoverTurningOp
    {
        None,
        ShowFront,
        HideFront,
        ShowBack,
        HideBack
    }

    private enum Corner
    {
        INVALID,
        TL,
        BL,
        TR,
        BR
    }

    private GComponent _pagesContainer;
    private string _pageResource;
    private int _pageWidth;
    private int _pageHeight;

    private int _pageCount;
    private int _currentPage;
    private Paper _paper;

    private int _turningTarget;
    private float _turningAmount;
    private CoverTurningOp _coverTurningOp;
    private GPath _turningPath;

    private GComponent[] _objects;
    private GGraph _mask1;
    private GGraph _mask2;
    private GObject _softShadow;
    private int[] _objectIndice;
    private int[] _objectNewIndice;

    private Corner _draggingCorner;
    private Vector2 _dragPoint;
    private float _touchDownTime;

    private GComponent _frontCover;
    private GComponent _backCover;
    private Vector2 _frontCoverPos;
    private Vector2 _backCoverPos;
    private CoverStatus _coverStatus;

    private EventListener _onTurnComplete;

    public override void ConstructFromXML(XML xml)
    {
        base.ConstructFromXML(xml);

        _pagesContainer = GetChild("pages").asCom;
        if (_pagesContainer == null)
        {
            Debug.LogError("Not a valid book resource");
            return;
        }

        var obj1 = _pagesContainer.GetChild("left").asCom;
        var obj2 = _pagesContainer.GetChild("right").asCom;
        if (obj1 == null || obj2 == null || obj1.resourceURL != obj2.resourceURL
            || obj1.width != obj2.width || obj2.x != obj1.x + obj1.width)
        {
            Debug.LogError("Not a valid book resource");
            return;
        }

        obj1.displayObject.home = displayObject.cachedTransform;
        obj2.displayObject.home = displayObject.cachedTransform;
        _pagesContainer.RemoveChild(obj1);
        _pagesContainer.RemoveChild(obj2);

        _frontCover = GetChild("frontCover") as GComponent;
        if (_frontCover != null)
            _frontCoverPos = _frontCover.position;
        _backCover = GetChild("backCover") as GComponent;
        if (_backCover != null)
            _backCoverPos = _backCover.position;

        _objects = new GComponent[4] {obj1, obj2, null, null};
        _objectIndice = new int[4] {-1, -1, -1, -1};
        _objectNewIndice = new int[4];
        _turningTarget = -1;
        _currentPage = -1;

        _pageWidth = (int) obj1.width;
        _pageHeight = (int) obj1.height;
        _pageResource = obj1.resourceURL;

        _mask1 = new GGraph();
        _mask1.displayObject.home = displayObject.cachedTransform;
        _mask1.SetSize(_pageWidth, _pageHeight);

        _mask2 = new GGraph();
        _mask2.displayObject.home = displayObject.cachedTransform;
        _mask2.SetSize(_pageWidth, _pageHeight);

        SetupHotspot(GetChild("hotspot_tl"), Corner.TL);
        SetupHotspot(GetChild("hotspot_bl"), Corner.BL);
        SetupHotspot(GetChild("hotspot_tr"), Corner.TR);
        SetupHotspot(GetChild("hotspot_br"), Corner.BR);
    }

    public override void Dispose()
    {
        for (var i = 0; i < 4; i++)
            if (_objects[i] != null)
                _objects[i].Dispose();
        _mask1.Dispose();
        _mask2.Dispose();
        if (_softShadow != null)
            _softShadow.Dispose();

        base.Dispose();
    }

    /// <summary>
    /// 
    /// </summary>
    public EventListener onTurnComplete =>
        _onTurnComplete ?? (_onTurnComplete = new EventListener(this, "onTurnComplete"));

    /// <summary>
    /// 
    /// </summary>
    /// <param name="res"></param>
    public void SetSoftShadowResource(string res)
    {
        _softShadow = UIPackage.CreateObjectFromURL(res);
        _softShadow.height = Mathf.Sqrt(Mathf.Pow(_pageWidth, 2) + Mathf.Pow(_pageHeight, 2)) + 60;
        _softShadow.displayObject.home = displayObject.cachedTransform;
        _softShadow.sortingOrder = int.MaxValue;
    }

    /// <summary>
    /// 
    /// </summary>
    public Paper pageSoftness
    {
        get => _paper;
        set => _paper = value;
    }

    /// <summary>
    /// 
    /// </summary>
    public int pageCount
    {
        get => _pageCount;
        set
        {
            if (_pageCount % 2 != 0)
                throw new Exception("Page count must be even!");

            _pageCount = value;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public int currentPage
    {
        get => _currentPage;
        set
        {
            if (value < 0 || value > _pageCount - 1)
                throw new Exception("Page index out of bounds: " + value);

            if (_currentPage != value)
            {
                GTween.Kill(this, true);

                _currentPage = value;
                _coverStatus = CoverStatus.Hidden;

                RenderPages();
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pageIndex"></param>
    public void TurnTo(int pageIndex)
    {
        if (pageIndex < 0 || pageIndex > _pageCount - 1)
            throw new Exception("Page index out of bounds: " + pageIndex);

        GTween.Kill(this, true);

        if (_coverStatus == CoverStatus.ShowingFront)
        {
            _coverTurningOp = CoverTurningOp.HideFront;
            _draggingCorner = Corner.BR;
        }
        else if (_coverStatus == CoverStatus.ShowingBack)
        {
            _coverTurningOp = CoverTurningOp.HideBack;
            _draggingCorner = Corner.BL;
        }

        var tt1 = _currentPage;
        if (_currentPage % 2 == 0)
            tt1--;
        var tt2 = pageIndex;
        if (pageIndex % 2 == 0)
            tt2--;
        if (tt1 == tt2)
        {
            _currentPage = pageIndex;
            _turningTarget = -1;
        }
        else
        {
            _turningTarget = pageIndex;
            if (_turningTarget < _currentPage)
                _draggingCorner = Corner.BL;
            else
                _draggingCorner = Corner.BR;
        }

        if (_draggingCorner == Corner.INVALID)
            return;

        StartTween();
    }

    /// <summary>
    /// 
    /// </summary>
    public void TurnNext()
    {
        GTween.Kill(this, true);

        if (isCoverShowing(CoverType.Front))
            TurnTo(0);
        else if (_currentPage == _pageCount - 1)
            ShowCover(CoverType.Back, true);
        else if (_currentPage % 2 == 0)
            TurnTo(_currentPage + 1);
        else
            TurnTo(_currentPage + 2);
    }

    /// <summary>
    /// 
    /// </summary>
    public void TurnPrevious()
    {
        GTween.Kill(this, true);

        if (isCoverShowing(CoverType.Back))
            TurnTo(_pageCount - 1);
        else if (_currentPage == 0)
            ShowCover(CoverType.Front, true);
        else if (_currentPage % 2 == 0)
            TurnTo(_currentPage - 2);
        else
            TurnTo(_currentPage - 1);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cover"></param>
    /// <param name="turnEffect"></param>
    public void ShowCover(CoverType cover, bool turnEffect)
    {
        GTween.Kill(this, true);

        if (_frontCover == null)
            return;

        if (turnEffect)
        {
            if (cover == CoverType.Front)
            {
                if (_coverStatus == CoverStatus.ShowingFront)
                    return;

                _coverTurningOp = CoverTurningOp.ShowFront;
                _draggingCorner = Corner.BL;
                _currentPage = 0;
            }
            else
            {
                if (_coverStatus == CoverStatus.ShowingBack)
                    return;

                _coverTurningOp = CoverTurningOp.ShowBack;
                _draggingCorner = Corner.BR;
                _currentPage = _pageCount - 1;
            }

            StartTween();
        }
        else
        {
            if (cover == CoverType.Front)
            {
                _currentPage = 0;
                _coverStatus = CoverStatus.ShowingFront;
            }
            else
            {
                _currentPage = _pageCount - 1;
                _coverStatus = CoverStatus.ShowingBack;
            }

            RenderPages();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cover"></param>
    /// <returns></returns>
    public bool isCoverShowing(CoverType cover)
    {
        return cover == CoverType.Front
            ? _coverStatus == CoverStatus.ShowingFront
            : _coverStatus == CoverStatus.ShowingBack;
    }

    private void StartTween()
    {
        _turningAmount = 0;
        RenderPages();
        var source = GetCornerPosition(_draggingCorner, _coverTurningOp != CoverTurningOp.None);
        Vector2 target;
        if (_draggingCorner == Corner.TL || _draggingCorner == Corner.BL)
            target = GetCornerPosition(_draggingCorner + 2, _coverTurningOp != CoverTurningOp.None);
        else
            target = GetCornerPosition(_draggingCorner - 2, _coverTurningOp != CoverTurningOp.None);

        if (_turningPath == null)
            _turningPath = new GPath();
        var mid = new Vector2(source.x + (target.x - source.x) / 2, target.y - 50);
        _turningPath.Create(new GPathPoint(source), new GPathPoint(mid), new GPathPoint(target));
        GTween.To(source, target, EffectDuration).SetUserData(true).SetTarget(this)
            .SetPath(_turningPath)
            .OnUpdate(OnTurnUpdate).OnComplete(OnTurnComplete);
    }

    private void OnTurnUpdate(GTweener tweener)
    {
        _dragPoint = tweener.value.vec2;
        _turningAmount =
            _dragPoint.x / (_coverTurningOp != CoverTurningOp.None ? _frontCover.width * 2 : _pageWidth * 2);
        if (_draggingCorner == Corner.TR || _draggingCorner == Corner.BR)
            _turningAmount = 1 - _turningAmount;
        PlayTurnEffect();
    }

    private void OnTurnComplete(GTweener tweener)
    {
        var suc = (bool) tweener.userData;
        _draggingCorner = Corner.INVALID;
        if (suc && _turningTarget != -1)
            _currentPage = _turningTarget;
        if (suc && _coverTurningOp != CoverTurningOp.None)
        {
            if (_coverTurningOp == CoverTurningOp.ShowFront)
                _coverStatus = CoverStatus.ShowingFront;
            else if (_coverTurningOp == CoverTurningOp.ShowBack)
                _coverStatus = CoverStatus.ShowingBack;
            else
                _coverStatus = CoverStatus.Hidden;
        }

        _coverTurningOp = CoverTurningOp.None;
        _turningTarget = -1;

        RenderPages();

        DispatchEvent("onTurnComplete");
    }

    private void PlayTurnEffect()
    {
        if (_coverTurningOp != CoverTurningOp.None)
            PlayCoverEffect();

        if (_turningTarget != -1)
        {
            if (_paper == Paper.Hard)
                PlayHardEffect();
            else
                PlaySoftEffect();
        }
    }

    private void PlayCoverEffect()
    {
        var amount = Mathf.Clamp01(_turningAmount);
        float ratio;
        bool isLeft;
        var turningObj = _coverTurningOp == CoverTurningOp.ShowFront || _coverTurningOp == CoverTurningOp.HideFront
            ? _frontCover
            : _backCover;
        var mesh = GetHardMesh(turningObj);

        if (amount < 0.5f)
        {
            ratio = 1 - amount * 2;
            isLeft = _coverTurningOp == CoverTurningOp.ShowFront || _coverTurningOp == CoverTurningOp.HideBack;
        }
        else
        {
            ratio = (amount - 0.5f) * 2;
            isLeft = _coverTurningOp == CoverTurningOp.HideFront || _coverTurningOp == CoverTurningOp.ShowBack;
        }

        if (turningObj == _frontCover)
            SetCoverStatus(turningObj, CoverType.Front, !isLeft);
        else
            SetCoverStatus(turningObj, CoverType.Back, isLeft);

        mesh.points.Clear();
        mesh.texcoords.Clear();
        if (isLeft)
        {
            var topOffset = 1f / 8 * (1 - ratio);
            var xOffset = 1 - ratio;
            mesh.Add(new Vector2(xOffset, 1 + topOffset));
            mesh.Add(new Vector2(xOffset, -topOffset));
            mesh.Add(new Vector2(1, 0));
            mesh.Add(new Vector2(1, 1));
        }
        else
        {
            var topOffset = 1f / 8 * (1 - ratio);
            mesh.Add(new Vector2(0, 1));
            mesh.Add(new Vector2(0, 0));
            mesh.Add(new Vector2(ratio, -topOffset));
            mesh.Add(new Vector2(ratio, 1 + topOffset));
        }

        mesh.texcoords.AddRange(VertexBuffer.NormalizedUV);
    }

    private void PlayHardEffect()
    {
        var amount = Mathf.Clamp01(_turningAmount);
        float ratio;
        bool isLeft;
        GComponent turningObj;
        PolygonMesh mesh;
        if (amount < 0.5f)
        {
            ratio = 1 - amount * 2;
            isLeft = _turningTarget < _currentPage;

            turningObj = _objects[2];
            mesh = GetHardMesh(turningObj);
            GetHardMesh(_objects[3]).points.Clear();
        }
        else
        {
            ratio = (amount - 0.5f) * 2;
            isLeft = _turningTarget > _currentPage;

            turningObj = _objects[3];
            mesh = GetHardMesh(turningObj);
            GetHardMesh(_objects[2]).points.Clear();
        }

        mesh.points.Clear();
        mesh.texcoords.Clear();
        if (isLeft)
        {
            turningObj.x = 0;

            var topOffset = 1f / 8 * (1 - ratio);
            var xOffset = 1 - ratio;
            mesh.Add(new Vector2(xOffset, 1 + topOffset));
            mesh.Add(new Vector2(xOffset, -topOffset));
            mesh.Add(new Vector2(1, 0));
            mesh.Add(new Vector2(1, 1));
        }
        else
        {
            turningObj.x = _pageWidth;

            var topOffset = 1f / 8 * (1 - ratio);
            mesh.Add(new Vector2(0, 1));
            mesh.Add(new Vector2(0, 0));
            mesh.Add(new Vector2(ratio, -topOffset));
            mesh.Add(new Vector2(ratio, 1 + topOffset));
        }

        mesh.texcoords.AddRange(VertexBuffer.NormalizedUV);
    }

    private void FlipPoint(ref Vector2 pt, float w, float h)
    {
        switch (_draggingCorner)
        {
            case Corner.TL:
                pt.x = w - pt.x;
                pt.y = h - pt.y;
                break;
            case Corner.BL:
                pt.x = w - pt.x;
                break;
            case Corner.TR:
                pt.y = h - pt.y;
                break;
        }
    }

    private void PlaySoftEffect()
    {
        var turningObj1 = _objects[2];
        var turningObj2 = _objects[3];
        var mesh1 = GetSoftMesh(turningObj1);
        var mesh2 = GetSoftMesh(turningObj2);

        /**
        *               a           
        *              /  \         
        * f(0,0)------/    b--g(w,0)
        * |          /     /  |     
        * |         /     /   |     
        * |        c     /    |     
        * |         \   /     |     
        * |          \ /      |     
        * e(0,h)-----d--------h(w,h)
        */
        Vector2 pa, pb, pc, pd, pe, pf, pg, ph;
        float k, angle;
        var threePoints = false;

        pc = _dragPoint;
        pe = new Vector2(0, _pageHeight);
        pf = Vector2.zero;
        pg = new Vector2(_pageWidth, 0);
        ph = new Vector2(_pageWidth, _pageHeight);

        FlipPoint(ref pc, _pageWidth * 2, _pageHeight);
        pc.x -= _pageWidth;
        if (pc.x >= _pageWidth)
            return;

        k = (ph.y - pc.y) / (ph.x - pc.x);
        var k2 = 1 + Mathf.Pow(k, 2);
        float min;
        min = ph.x - _pageWidth * 2 / k2;
        if (pc.x < min)
        {
            pc.x = min;
            if (pc.x >= _pageWidth)
                return;
            pc.y = ph.y - k * (ph.x - pc.x);
        }

        min = ph.x - (_pageWidth + _pageHeight * k) * 2 / k2;
        if (pc.x < min)
        {
            pc.x = min;
            if (pc.x >= _pageWidth)
                return;
            pc.y = ph.y - k * (ph.x - pc.x);
        }

        angle = Mathf.Atan(k) * Mathf.Rad2Deg;
        pd = new Vector2(_pageWidth - k2 * (ph.x - pc.x) / 2, _pageHeight);
        pb = new Vector2(pd.x + _pageHeight * k, 0);
        pa = new Vector2();

        if (pb.x > _pageWidth)
        {
            pb.x = _pageWidth;
            pa = new Vector2(_pageWidth, _pageHeight - (_pageWidth - pd.x) / k);
            threePoints = true;
        }

        FlipPoint(ref pa, _pageWidth, _pageHeight);
        FlipPoint(ref pb, _pageWidth, _pageHeight);
        FlipPoint(ref pd, _pageWidth, _pageHeight);
        FlipPoint(ref pc, _pageWidth, _pageHeight);
        if (_draggingCorner == Corner.BL || _draggingCorner == Corner.TL)
            angle = -angle;

        switch (_draggingCorner)
        {
            case Corner.BR:
            {
                turningObj1.SetPivot(0, 0, true);
                turningObj1.position = new Vector2(_pageWidth, 0);

                turningObj2.SetPivot(0, 1, true);
                turningObj2.position = new Vector2(_pageWidth + pc.x, pc.y);
                turningObj2.rotation = 2 * angle;

                if (_softShadow != null)
                {
                    _softShadow.SetPivot(1, (_softShadow.height - 30) / _softShadow.height, true);
                    _softShadow.position = new Vector2(Vector2.Distance(pc, pd), _pageHeight);
                    _softShadow.rotation = -angle;
                    if (_softShadow.x > _pageWidth - 20)
                        _softShadow.alpha = (_pageWidth - _softShadow.x) / 20;
                    else
                        _softShadow.alpha = 1;
                }

                mesh1.points.Clear();
                mesh1.Add(pe);
                mesh1.Add(pf);
                mesh1.Add(pb);
                if (threePoints)
                    mesh1.Add(pa);
                mesh1.Add(pd);

                mesh2.points.Clear();
                mesh2.Add(new Vector2(Vector2.Distance(pc, pd), _pageHeight));
                mesh2.Add(new Vector2(0, _pageHeight));
                if (threePoints)
                {
                    mesh2.Add(new Vector2(0, _pageHeight - Vector2.Distance(pc, pa)));
                }
                else
                {
                    mesh2.Add(new Vector2(0, 0));
                    mesh2.Add(new Vector2(Vector2.Distance(pg, pb), 0));
                }

                break;
            }
            case Corner.TR:
            {
                turningObj1.SetPivot(0, 0, true);
                turningObj1.position = new Vector2(_pageWidth, 0);

                turningObj2.SetPivot(0, 0, true);
                turningObj2.position = new Vector2(_pageWidth + pc.x, pc.y);
                turningObj2.rotation = -2 * angle;

                if (_softShadow != null)
                {
                    _softShadow.SetPivot(1, 30 / _softShadow.height, true);
                    _softShadow.position = new Vector2(Vector2.Distance(pc, pd), 0);
                    _softShadow.rotation = angle;
                    if (_softShadow.x > _pageWidth - 20)
                        _softShadow.alpha = (_pageWidth - _softShadow.x) / 20;
                    else
                        _softShadow.alpha = 1;
                }

                mesh1.points.Clear();
                mesh1.Add(pe);
                mesh1.Add(pf);
                mesh1.Add(pd);
                if (threePoints)
                    mesh1.Add(pa);
                mesh1.Add(pb);

                mesh2.points.Clear();
                if (threePoints)
                {
                    mesh2.Add(new Vector2(0, Vector2.Distance(pc, pa)));
                }
                else
                {
                    mesh2.Add(new Vector2(Vector2.Distance(pb, ph), _pageHeight));
                    mesh2.Add(new Vector2(0, _pageHeight));
                }

                mesh2.Add(new Vector2(0, 0));
                mesh2.Add(new Vector2(Vector2.Distance(pc, pd), 0));
                break;
            }
            case Corner.BL:
            {
                turningObj1.SetPivot(0, 0, true);
                turningObj1.position = Vector2.zero;

                turningObj2.SetPivot(1, 1, true);
                turningObj2.position = pc;
                turningObj2.rotation = 2 * angle;

                if (_softShadow != null)
                {
                    _softShadow.SetPivot(1, 30 / _softShadow.height, true);
                    _softShadow.position = new Vector2(_pageWidth - Vector2.Distance(pc, pd), _pageHeight);
                    _softShadow.rotation = 180 - angle;
                    if (_softShadow.x < 20)
                        _softShadow.alpha = (_softShadow.x - 20) / 20;
                    else
                        _softShadow.alpha = 1;
                }

                mesh1.points.Clear();
                mesh1.Add(pb);
                mesh1.Add(pg);
                mesh1.Add(ph);
                mesh1.Add(pd);
                if (threePoints)
                    mesh1.Add(pa);

                mesh2.points.Clear();
                if (!threePoints)
                {
                    mesh2.Add(new Vector2(_pageWidth - Vector2.Distance(pf, pb), 0));
                    mesh2.Add(new Vector2(_pageWidth, 0));
                }
                else
                {
                    mesh2.Add(new Vector2(_pageWidth, _pageHeight - Vector2.Distance(pc, pa)));
                }

                mesh2.Add(new Vector2(_pageWidth, _pageHeight));
                mesh2.Add(new Vector2(_pageWidth - Vector2.Distance(pc, pd), _pageHeight));
                break;
            }
            case Corner.TL:
            {
                turningObj1.SetPivot(0, 0, true);
                turningObj1.position = Vector2.zero;

                turningObj2.SetPivot(1, 0, true);
                turningObj2.position = pc;
                turningObj2.rotation = -2 * angle;

                if (_softShadow != null)
                {
                    _softShadow.SetPivot(1, (_softShadow.height - 30) / _softShadow.height, true);
                    _softShadow.position = new Vector2(_pageWidth - Vector2.Distance(pc, pd), 0);
                    _softShadow.rotation = 180 + angle;
                    if (_softShadow.x < 20)
                        _softShadow.alpha = (_softShadow.x - 20) / 20;
                    else
                        _softShadow.alpha = 1;
                }

                mesh1.points.Clear();
                mesh1.Add(pd);
                mesh1.Add(pg);
                mesh1.Add(ph);
                mesh1.Add(pb);
                if (threePoints)
                    mesh1.Add(pa);

                mesh2.points.Clear();
                mesh2.Add(new Vector2(_pageWidth - Vector2.Distance(pc, pd), 0));
                mesh2.Add(new Vector2(_pageWidth, 0));
                if (threePoints)
                {
                    mesh2.Add(new Vector2(_pageWidth, Vector2.Distance(pc, pa)));
                }
                else
                {
                    mesh2.Add(new Vector2(_pageWidth, _pageHeight));
                    mesh2.Add(new Vector2(_pageWidth - Vector2.Distance(pe, pb), _pageHeight));
                }

                break;
            }
        }
    }

    private void RenderPages()
    {
        RenderCovers();

        if (_softShadow != null)
            _softShadow.RemoveFromParent();

        var curPage = _currentPage;
        if (curPage % 2 == 0)
            curPage--;

        int leftPage, rightPage, turningPageBack, turningPageFront;
        leftPage = curPage;
        rightPage = leftPage < _pageCount - 1 ? leftPage + 1 : -1;

        if (_turningTarget != -1)
        {
            var tt = _turningTarget;
            if (tt % 2 == 0)
                tt = tt - 1;

            if (tt == curPage)
            {
                _currentPage = _turningTarget;
                turningPageBack = turningPageFront = -1;
            }
            else if (tt > leftPage)
            {
                turningPageFront = tt;
                turningPageBack = rightPage;
                rightPage = tt < _pageCount - 1 ? tt + 1 : -1;
            }
            else
            {
                turningPageFront = tt > 0 ? tt + 1 : 0;
                turningPageBack = leftPage;
                leftPage = tt > 0 ? tt : -1;
            }
        }
        else
        {
            turningPageBack = turningPageFront = -1;
        }

        _objectNewIndice[0] = leftPage;
        _objectNewIndice[1] = rightPage;
        _objectNewIndice[2] = turningPageBack;
        _objectNewIndice[3] = turningPageFront;

        for (var i = 0; i < 4; i++)
        {
            var pageIndex = _objectNewIndice[i];
            if (pageIndex != -1)
                for (var j = 0; j < 4; j++)
                {
                    var pageIndex2 = _objectIndice[j];
                    if (pageIndex2 == pageIndex)
                    {
                        if (j != i)
                        {
                            _objectIndice[j] = _objectIndice[i];
                            _objectIndice[i] = pageIndex;

                            var tmp = _objects[j];
                            _objects[j] = _objects[i];
                            _objects[i] = tmp;
                        }

                        break;
                    }
                }
        }

        for (var i = 0; i < 4; i++)
        {
            var obj = _objects[i];
            var oldIndex = _objectIndice[i];
            var index = _objectNewIndice[i];
            _objectIndice[i] = index;
            if (index == -1)
            {
                if (obj != null)
                    obj.RemoveFromParent();
            }
            else if (oldIndex != index)
            {
                if (obj == null)
                {
                    obj = UIPackage.CreateObjectFromURL(_pageResource).asCom;
                    obj.displayObject.home = displayObject.cachedTransform;
                    _objects[i] = obj;
                }

                _pagesContainer.AddChild(obj);
                pageRenderer(index, obj);
            }
            else
            {
                if (obj.parent == null)
                {
                    _pagesContainer.AddChild(obj);
                    pageRenderer(index, obj);
                }
                else
                {
                    _pagesContainer.AddChild(obj);
                }
            }

            if (obj != null && obj.parent != null)
            {
                var c1 = obj.GetController("side");
                if (c1 != null)
                {
                    if (index == 0)
                        c1.selectedPage = "first";
                    else if (index == _pageCount - 1)
                        c1.selectedPage = "last";
                    else
                        c1.selectedPage = index % 2 == 0 ? "right" : "left";
                }

                if (i == 0 || i == 1)
                    SetPageNormal(obj, i == 0);
                else if (_paper == Paper.Soft)
                    SetPageSoft(obj, i == 2);
                else
                    SetPageHard(obj, i == 2);
            }
        }
    }

    private void RenderCovers()
    {
        if (_frontCover != null)
        {
            if (_coverTurningOp == CoverTurningOp.ShowFront || _coverTurningOp == CoverTurningOp.HideFront)
            {
                SetPageHard(_frontCover, true);
                SetCoverStatus(_frontCover, CoverType.Front, _coverTurningOp == CoverTurningOp.HideFront);
            }
            else
            {
                if (_frontCover.displayObject.cacheAsBitmap)
                    SetCoverNormal(_frontCover, CoverType.Front);

                SetCoverStatus(_frontCover, CoverType.Front, _coverStatus == CoverStatus.ShowingFront);
            }
        }

        if (_backCover != null)
        {
            if (_coverTurningOp == CoverTurningOp.ShowBack || _coverTurningOp == CoverTurningOp.HideBack)
            {
                SetPageHard(_backCover, true);
                SetCoverStatus(_backCover, CoverType.Back, _coverTurningOp == CoverTurningOp.HideBack);
            }
            else
            {
                if (_backCover.displayObject.cacheAsBitmap)
                    SetCoverNormal(_backCover, CoverType.Back);

                SetCoverStatus(_backCover, CoverType.Back, _coverStatus == CoverStatus.ShowingBack);
            }
        }
    }

    private void SetupHotspot(GObject obj, Corner corner)
    {
        if (obj == null)
            return;

        obj.data = corner;

        obj.onTouchBegin.Add(__touchBegin);
        obj.onTouchMove.Add(__touchMove);
        obj.onTouchEnd.Add(__touchEnd);
    }

    private void SetPageHard(GComponent obj, bool front)
    {
        obj.touchable = false;
        obj.displayObject.cacheAsBitmap = true;
        if (obj.mask != null)
        {
            obj.mask.RemoveFromParent();
            obj.mask = null;
        }

        var mesh = obj.displayObject.paintingGraphics.GetMeshFactory<PolygonMesh>();
        mesh.usePercentPositions = true;
        mesh.points.Clear();
        mesh.texcoords.Clear();
        obj.displayObject.paintingGraphics.SetMeshDirty();

        if (front)
        {
            mesh.points.AddRange(VertexBuffer.NormalizedPosition);
            mesh.texcoords.AddRange(VertexBuffer.NormalizedUV);
        }
    }

    private void SetPageSoft(GComponent obj, bool front)
    {
        obj.touchable = false;
        obj.displayObject.cacheAsBitmap = false;
        var mask = front ? _mask1.displayObject : _mask2.displayObject;
        obj.mask = mask;

        var mesh = mask.graphics.GetMeshFactory<PolygonMesh>();
        mesh.usePercentPositions = false;
        mesh.points.Clear();
        mesh.texcoords.Clear();
        mask.graphics.SetMeshDirty();

        if (front)
        {
            mesh.Add(new Vector2(0, _pageHeight));
            mesh.Add(Vector2.zero);
            mesh.Add(new Vector2(_pageWidth, 0));
            mesh.Add(new Vector2(_pageWidth, _pageHeight));
        }
        else if (_softShadow != null)
        {
            obj.AddChild(_softShadow);
        }
    }

    private void SetPageNormal(GComponent obj, bool left)
    {
        obj.displayObject.cacheAsBitmap = false;
        obj.touchable = true;
        obj.SetPivot(0, 0, true);
        if (left)
            obj.SetXY(0, 0);
        else
            obj.SetXY(_pageWidth, 0);
        obj.rotation = 0;
        if (obj.mask != null)
        {
            obj.mask.RemoveFromParent();
            obj.mask = null;
        }
    }

    private void SetCoverStatus(GComponent obj, CoverType coverType, bool show)
    {
        var c = obj.GetController("side");
        if (show)
        {
            if (c.selectedIndex != 0)
            {
                obj.position = coverType == CoverType.Front ? _backCoverPos : _frontCoverPos;
                obj.parent.SetChildIndexBefore(obj, obj.parent.GetChildIndex(_pagesContainer) + 1);
                c.selectedIndex = 0; //front

                if (obj.displayObject.cacheAsBitmap)
                    obj.displayObject.cacheAsBitmap = true; //refresh
            }
        }
        else
        {
            if (c.selectedIndex != 1)
            {
                obj.position = coverType == CoverType.Front ? _frontCoverPos : _backCoverPos;
                obj.parent.SetChildIndexBefore(obj, obj.parent.GetChildIndex(_pagesContainer));
                c.selectedIndex = 1; //back

                if (obj.displayObject.cacheAsBitmap)
                    obj.displayObject.cacheAsBitmap = true; //refresh
            }
        }
    }

    private void SetCoverNormal(GComponent obj, CoverType coverType)
    {
        obj.position = coverType == CoverType.Front ? _frontCoverPos : _backCoverPos;
        obj.displayObject.cacheAsBitmap = false;
        obj.touchable = true;
        obj.parent.SetChildIndexBefore(obj, obj.parent.GetChildIndex(_pagesContainer));
        obj.GetController("side").selectedIndex = 1; //back
    }

    private PolygonMesh GetHardMesh(GComponent obj)
    {
        obj.displayObject.paintingGraphics.SetMeshDirty();
        return obj.displayObject.paintingGraphics.GetMeshFactory<PolygonMesh>();
    }

    private PolygonMesh GetSoftMesh(GComponent obj)
    {
        obj.mask.graphics.SetMeshDirty();
        return obj.mask.graphics.GetMeshFactory<PolygonMesh>();
    }

    private void UpdateDragPosition(Vector2 pos)
    {
        if (_coverTurningOp != CoverTurningOp.None)
        {
            _dragPoint = GlobalToLocal(pos) - _frontCoverPos;
            _turningAmount = _dragPoint.x / (2 * _frontCover.width);
        }
        else
        {
            _dragPoint = _pagesContainer.GlobalToLocal(pos);
            _turningAmount = _dragPoint.x / (2 * _pageWidth);
        }

        if (_draggingCorner == Corner.TR || _draggingCorner == Corner.BR)
            _turningAmount = 1 - _turningAmount;
    }

    private Vector2 GetCornerPosition(Corner corner, bool isCover)
    {
        var w = isCover ? _frontCover.width : _pageWidth;
        var h = isCover ? _frontCover.height : _pageHeight;
        Vector2 pt;
        switch (corner)
        {
            case Corner.BL:
                pt = new Vector2(0, h);
                break;

            case Corner.TR:
                pt = new Vector2(w * 2, 0);
                break;

            case Corner.BR:
                pt = new Vector2(w * 2, h);
                break;

            default:
                pt = Vector2.zero;
                break;
        }

        return pt;
    }

    private void __touchBegin(EventContext context)
    {
        GTween.Kill(this, true);

        _draggingCorner = (Corner) ((GObject) context.sender).data;
        if (_draggingCorner == Corner.TL || _draggingCorner == Corner.BL)
        {
            if (_coverStatus == CoverStatus.ShowingBack)
            {
                _coverTurningOp = CoverTurningOp.HideBack;
            }
            else if (_objectNewIndice[0] == -1)
            {
                if (_frontCover != null && _coverStatus != CoverStatus.ShowingFront)
                    _coverTurningOp = CoverTurningOp.ShowFront;
                else
                    _draggingCorner = Corner.INVALID;
            }
            else
            {
                _turningTarget = _objectNewIndice[0] - 2;
                if (_turningTarget < 0)
                    _turningTarget = 0;
            }
        }
        else
        {
            if (_coverStatus == CoverStatus.ShowingFront)
            {
                _coverTurningOp = CoverTurningOp.HideFront;
            }
            else if (_objectNewIndice[1] == -1)
            {
                if (_backCover != null && _coverStatus != CoverStatus.ShowingBack)
                    _coverTurningOp = CoverTurningOp.ShowBack;
                else
                    _draggingCorner = Corner.INVALID;
            }
            else
            {
                _turningTarget = _objectNewIndice[1] + 1;
            }
        }

        if (_draggingCorner != Corner.INVALID)
        {
            _touchDownTime = Time.unscaledTime;
            UpdateDragPosition(context.inputEvent.position);
            RenderPages();
            PlayTurnEffect();

            context.CaptureTouch();
        }
    }

    private void __touchMove(EventContext context)
    {
        if (_draggingCorner != Corner.INVALID)
        {
            UpdateDragPosition(context.inputEvent.position);
            PlayTurnEffect();
        }
    }

    private void __touchEnd(EventContext context)
    {
        if (_draggingCorner != Corner.INVALID)
        {
            var suc = _turningAmount > 0.4f || Time.unscaledTime - _touchDownTime < 0.35f;
            Vector2 target;
            if (suc)
            {
                if (_draggingCorner == Corner.TL || _draggingCorner == Corner.BL)
                    target = GetCornerPosition(_draggingCorner + 2, _coverTurningOp != CoverTurningOp.None);
                else
                    target = GetCornerPosition(_draggingCorner - 2, _coverTurningOp != CoverTurningOp.None);
            }
            else
            {
                target = GetCornerPosition(_draggingCorner, _coverTurningOp != CoverTurningOp.None);
            }

            var duration = Mathf.Max(EffectDuration * 0.5f,
                Mathf.Abs(target.x - _dragPoint.x) / (_pageWidth * 2) * EffectDuration);
            GTween.To(_dragPoint, target, duration).SetTarget(this).SetUserData(suc)
                .OnUpdate(OnTurnUpdate).OnComplete(OnTurnComplete);
        }
    }
}