using FairyGUI;
using UnityEngine;

public class BagWindow : Window
{
    private GList _list;

    public BagWindow()
    {
    }

    protected override void OnInit()
    {
        contentPane = UIPackage.CreateObject("Bag", "BagWin").asCom;
        Center();
        modal = true;

        _list = contentPane.GetChild("list").asList;
        _list.onClickItem.Add(__clickItem);
        _list.itemRenderer = RenderListItem;
        _list.numItems = 45;
    }

    private void RenderListItem(int index, GObject obj)
    {
        var button = (GButton) obj;
        button.icon = "i" + Random.Range(0, 10);
        button.title = "" + Random.Range(0, 100);
    }

    protected override void DoShowAnimation()
    {
        SetScale(0.1f, 0.1f);
        SetPivot(0.5f, 0.5f);
        TweenScale(new Vector2(1, 1), 0.3f).OnComplete(OnShown);
    }

    protected override void DoHideAnimation()
    {
        TweenScale(new Vector2(0.1f, 0.1f), 0.3f).OnComplete(HideImmediately);
    }

    private void __clickItem(EventContext context)
    {
        var item = (GButton) context.data;
        contentPane.GetChild("n11").asLoader.url = item.icon;
        contentPane.GetChild("n13").text = item.icon;
    }
}