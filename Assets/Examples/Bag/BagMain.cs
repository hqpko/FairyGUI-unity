using UnityEngine;
using FairyGUI;

/// <summary>
/// A game bag demo, demonstrated how to customize loader to load icons not in the UI package.
/// </summary>
public class BagMain : MonoBehaviour
{
    private GComponent _mainView;
    private BagWindow _bagWindow;

    private void Awake()
    {
        //Register custom loader class
        UIObjectFactory.SetLoaderExtension(typeof(MyGLoader));
    }

    private void Start()
    {
        Application.targetFrameRate = 60;
        Stage.inst.onKeyDown.Add(OnKeyDown);
        GRoot.inst.SetContentScaleFactor(1136, 640);
        _mainView = GetComponent<UIPanel>().ui;

        _bagWindow = new BagWindow();
        _mainView.GetChild("bagBtn").onClick.Add(() => { _bagWindow.Show(); });
    }

    private void OnKeyDown(EventContext context)
    {
        if (context.inputEvent.keyCode == KeyCode.Escape) Application.Quit();
    }
}