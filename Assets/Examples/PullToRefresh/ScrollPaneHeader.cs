using FairyGUI;
using UnityEngine;

public class ScrollPaneHeader : GComponent
{
    private Controller _c1;

    public override void ConstructFromXML(FairyGUI.Utils.XML xml)
    {
        base.ConstructFromXML(xml);

        _c1 = GetController("c1");

        onSizeChanged.Add(OnSizeChanged);
    }

    private void OnSizeChanged()
    {
        if (_c1.selectedIndex == 2 || _c1.selectedIndex == 3)
            return;

        if (height > sourceHeight)
            _c1.selectedIndex = 1;
        else
            _c1.selectedIndex = 0;
    }

    public bool ReadyToRefresh => _c1.selectedIndex == 1;

    public void SetRefreshStatus(int value)
    {
        _c1.selectedIndex = value;
    }
}