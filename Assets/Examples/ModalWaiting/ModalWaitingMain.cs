using System.Collections;
using UnityEngine;
using FairyGUI;

public class ModalWaitingMain : MonoBehaviour
{
    private GComponent _mainView;
    private Window4 _testWin;

    private void Awake()
    {
        UIPackage.AddPackage("UI/ModalWaiting");
        UIConfig.globalModalWaiting = "ui://ModalWaiting/GlobalModalWaiting";
        UIConfig.windowModalWaiting = "ui://ModalWaiting/WindowModalWaiting";
    }

    private void Start()
    {
        Application.targetFrameRate = 60;
        Stage.inst.onKeyDown.Add(OnKeyDown);

        _mainView = GetComponent<UIPanel>().ui;

        _testWin = new Window4();

        _mainView.GetChild("n0").onClick.Add(() => { _testWin.Show(); });

        StartCoroutine(WaitSomeTime());
    }

    private IEnumerator WaitSomeTime()
    {
        GRoot.inst.ShowModalWait();

        yield return new WaitForSeconds(3);

        GRoot.inst.CloseModalWait();
    }

    private void OnKeyDown(EventContext context)
    {
        if (context.inputEvent.keyCode == KeyCode.Escape) Application.Quit();
    }
}