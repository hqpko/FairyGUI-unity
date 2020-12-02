using UnityEngine;
using FairyGUI;

public class TypingEffectMain : MonoBehaviour
{
    private GComponent _mainView;
    private TypingEffect _te1;
    private TypingEffect _te2;

    private void Awake()
    {
        Application.targetFrameRate = 60;
        Stage.inst.onKeyDown.Add(OnKeyDown);
    }

    private void Start()
    {
        _mainView = GetComponent<UIPanel>().ui;

        _te1 = new TypingEffect(_mainView.GetChild("n2").asTextField);
        _te1.Start();
        Timers.inst.StartCoroutine(_te1.Print(0.050f));

        _te2 = new TypingEffect(_mainView.GetChild("n3").asTextField);
        _te2.Start();
        Timers.inst.Add(0.050f, 0, PrintText);
    }

    private void PrintText(object param)
    {
        if (!_te2.Print()) Timers.inst.Remove(PrintText);
    }

    private void OnKeyDown(EventContext context)
    {
        if (context.inputEvent.keyCode == KeyCode.Escape) Application.Quit();
    }
}