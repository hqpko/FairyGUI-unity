using System;
using System.Collections.Generic;
using FairyGUI;
using UnityEngine;

public class Window2 : Window
{
    public Window2()
    {
    }

    protected override void OnInit()
    {
        contentPane = UIPackage.CreateObject("Basics", "WindowB").asCom;
        Center();
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

    protected override void OnShown()
    {
        contentPane.GetTransition("t1").Play();
    }

    protected override void OnHide()
    {
        contentPane.GetTransition("t1").Stop();
    }
}