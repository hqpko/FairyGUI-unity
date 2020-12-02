using UnityEngine;
using FairyGUI;

public class RenderTextureMain : MonoBehaviour
{
    private GComponent _mainView;
    private Window3 _testWin;

    private void Start()
    {
        Application.targetFrameRate = 60;
        Stage.inst.onKeyDown.Add(OnKeyDown);

        _mainView = GetComponent<UIPanel>().ui;

        _testWin = new Window3();

        _mainView.GetChild("n2").onClick.Add(() => { _testWin.Show(); });
    }

    private void OnKeyDown(EventContext context)
    {
        if (context.inputEvent.keyCode == KeyCode.Escape) Application.Quit();
    }
}