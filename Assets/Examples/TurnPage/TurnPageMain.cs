using UnityEngine;
using FairyGUI;

public class TurnPageMain : MonoBehaviour
{
    private GComponent _mainView;
    private FairyBook _book;
    private GSlider _slider;

    private void Awake()
    {
        UIPackage.AddPackage("UI/TurnPage");
        UIObjectFactory.SetPackageItemExtension("ui://TurnPage/Book", typeof(FairyBook));
        UIObjectFactory.SetPackageItemExtension("ui://TurnPage/Page", typeof(BookPage));
    }

    private void Start()
    {
        Application.targetFrameRate = 60;
        Stage.inst.onKeyDown.Add(OnKeyDown);

        _mainView = GetComponent<UIPanel>().ui;

        _book = (FairyBook) _mainView.GetChild("book");
        _book.SetSoftShadowResource("ui://TurnPage/shadow_soft");
        _book.pageRenderer = RenderPage;
        _book.pageCount = 20;
        _book.currentPage = 0;
        _book.ShowCover(FairyBook.CoverType.Front, false);
        _book.onTurnComplete.Add(OnTurnComplete);

        GearBase.disableAllTweenEffect = true;
        _mainView.GetController("bookPos").selectedIndex = 1;
        GearBase.disableAllTweenEffect = false;

        _mainView.GetChild("btnNext").onClick.Add(() => { _book.TurnNext(); });
        _mainView.GetChild("btnPrev").onClick.Add(() => { _book.TurnPrevious(); });

        _slider = _mainView.GetChild("pageSlide").asSlider;
        _slider.max = _book.pageCount - 1;
        _slider.value = 0;
        _slider.onGripTouchEnd.Add(() => { _book.TurnTo((int) _slider.value); });
    }

    private void OnTurnComplete()
    {
        _slider.value = _book.currentPage;

        if (_book.isCoverShowing(FairyBook.CoverType.Front))
            _mainView.GetController("bookPos").selectedIndex = 1;
        else if (_book.isCoverShowing(FairyBook.CoverType.Back))
            _mainView.GetController("bookPos").selectedIndex = 2;
        else
            _mainView.GetController("bookPos").selectedIndex = 0;
    }

    private void RenderPage(int index, GComponent page)
    {
        ((BookPage) page).render(index);
    }

    private void OnKeyDown(EventContext context)
    {
        if (context.inputEvent.keyCode == KeyCode.Escape) Application.Quit();
    }
}