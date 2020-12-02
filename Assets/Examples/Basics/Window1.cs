using System;
using System.Collections.Generic;
using FairyGUI;

public class Window1 : Window
{
    public Window1()
    {
    }

    protected override void OnInit()
    {
        contentPane = UIPackage.CreateObject("Basics", "WindowA").asCom;
        Center();
    }

    protected override void OnShown()
    {
        var list = contentPane.GetChild("n6").asList;
        list.RemoveChildrenToPool();

        for (var i = 0; i < 6; i++)
        {
            var item = list.AddItemFromPool().asButton;
            item.title = "" + i;
            item.icon = UIPackage.GetItemURL("Basics", "r4");
        }
    }
}