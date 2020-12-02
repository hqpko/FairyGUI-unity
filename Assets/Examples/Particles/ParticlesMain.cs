using UnityEngine;
using FairyGUI;

public class ParticlesMain : MonoBehaviour
{
    private GComponent _mainView;

    private void Awake()
    {
        UIPackage.AddPackage("UI/Particles");

        UIObjectFactory.SetPackageItemExtension("ui://Particles/CoolComponent", typeof(CoolComponent));
    }

    private void Start()
    {
        Application.targetFrameRate = 60;
        Stage.inst.onKeyDown.Add(OnKeyDown);

        _mainView = GetComponent<UIPanel>().ui;

        var prefab = Resources.Load("Flame");
        var go = (GameObject) Instantiate(prefab);
        _mainView.GetChild("holder").asGraph.SetNativeObject(new GoWrapper(go));

        _mainView.GetChild("c0").draggable = true;
        _mainView.GetChild("c1").draggable = true;
    }

    private void OnKeyDown(EventContext context)
    {
        if (context.inputEvent.keyCode == KeyCode.Escape) Application.Quit();
    }
}