using UnityEngine;
using FairyGUI;

public class PullToRefreshMain : MonoBehaviour
{
    private GComponent _mainView;
    private GList _list1;
    private GList _list2;

    private void Awake()
    {
        Application.targetFrameRate = 60;
        Stage.inst.onKeyDown.Add(OnKeyDown);

        UIObjectFactory.SetPackageItemExtension("ui://PullToRefresh/Header", typeof(ScrollPaneHeader));
    }

    private void Start()
    {
        _mainView = GetComponent<UIPanel>().ui;

        _list1 = _mainView.GetChild("list1").asList;
        _list1.itemRenderer = RenderListItem1;
        _list1.SetVirtual();
        _list1.numItems = 1;
        _list1.scrollPane.onPullDownRelease.Add(OnPullDownToRefresh);

        _list2 = _mainView.GetChild("list2").asList;
        _list2.itemRenderer = RenderListItem2;
        _list2.SetVirtual();
        _list2.numItems = 1;
        _list2.scrollPane.onPullUpRelease.Add(OnPullUpToRefresh);
    }

    private void RenderListItem1(int index, GObject obj)
    {
        var item = obj.asButton;
        item.title = "Item " + (_list1.numItems - index - 1);
    }

    private void RenderListItem2(int index, GObject obj)
    {
        var item = obj.asButton;
        item.title = "Item " + index;
    }

    private void OnPullDownToRefresh()
    {
        var header = (ScrollPaneHeader) _list1.scrollPane.header;
        if (header.ReadyToRefresh)
        {
            header.SetRefreshStatus(2);
            _list1.scrollPane.LockHeader(header.sourceHeight);

            //Simulate a async resquest
            Timers.inst.Add(2, 1, (object param) =>
            {
                _list1.numItems += 5;

                //Refresh completed
                header.SetRefreshStatus(3);
                _list1.scrollPane.LockHeader(35);

                Timers.inst.Add(2, 1, (object param2) =>
                {
                    header.SetRefreshStatus(0);
                    _list1.scrollPane.LockHeader(0);
                });
            });
        }
    }

    private void OnPullUpToRefresh()
    {
        var footer = (GComponent) _list2.scrollPane.footer;

        footer.GetController("c1").selectedIndex = 1;
        _list2.scrollPane.LockFooter(footer.sourceHeight);

        //Simulate a async resquest
        Timers.inst.Add(2, 1, (object param) =>
        {
            _list2.numItems += 5;

            //Refresh completed
            footer.GetController("c1").selectedIndex = 0;
            _list2.scrollPane.LockFooter(0);
        });
    }

    private void OnKeyDown(EventContext context)
    {
        if (context.inputEvent.keyCode == KeyCode.Escape) Application.Quit();
    }
}