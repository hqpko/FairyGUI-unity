using UnityEngine;
using FairyGUI;

public class JoystickMain : MonoBehaviour
{
    private GComponent _mainView;
    private GTextField _text;
    private JoystickModule _joystick;

    private void Start()
    {
        Application.targetFrameRate = 60;
        Stage.inst.onKeyDown.Add(OnKeyDown);

        _mainView = GetComponent<UIPanel>().ui;

        _text = _mainView.GetChild("n9").asTextField;

        _joystick = new JoystickModule(_mainView);
        _joystick.onMove.Add(__joystickMove);
        _joystick.onEnd.Add(__joystickEnd);
    }

    private void __joystickMove(EventContext context)
    {
        var degree = (float) context.data;
        _text.text = "" + degree;
    }

    private void __joystickEnd()
    {
        _text.text = "";
    }

    private void OnKeyDown(EventContext context)
    {
        if (context.inputEvent.keyCode == KeyCode.Escape) Application.Quit();
    }
}