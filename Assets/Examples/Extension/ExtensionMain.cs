using UnityEngine;
using FairyGUI;

public class ExtensionMain : MonoBehaviour
{
    private GComponent _mainView;
    private GList _list;

    private void Awake()
    {
        UIPackage.AddPackage("UI/Extension");
        UIObjectFactory.SetPackageItemExtension("ui://Extension/mailItem", typeof(MailItem));
    }

    private void Start()
    {
        Application.targetFrameRate = 60;
        Stage.inst.onKeyDown.Add(OnKeyDown);

        _mainView = GetComponent<UIPanel>().ui;

        _list = _mainView.GetChild("mailList").asList;
        for (var i = 0; i < 10; i++)
        {
            var item = (MailItem) _list.AddItemFromPool();
            item.setFetched(i % 3 == 0);
            item.setRead(i % 2 == 0);
            item.setTime("5 Nov 2015 16:24:33");
            item.title = "Mail title here";
        }

        _list.EnsureBoundsCorrect();
        var delay = 0f;
        for (var i = 0; i < 10; i++)
        {
            var item = (MailItem) _list.GetChildAt(i);
            if (_list.IsChildInView(item))
            {
                item.PlayEffect(delay);
                delay += 0.2f;
            }
            else
            {
                break;
            }
        }
    }

    private void OnKeyDown(EventContext context)
    {
        if (context.inputEvent.keyCode == KeyCode.Escape) Application.Quit();
    }
}