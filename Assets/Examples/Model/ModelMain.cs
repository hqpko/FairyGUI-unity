using UnityEngine;
using FairyGUI;

public class ModelMain : MonoBehaviour
{
    private GComponent _mainView;

    private void Start()
    {
        Application.targetFrameRate = 60;
        Stage.inst.onKeyDown.Add(OnKeyDown);

        _mainView = GetComponent<UIPanel>().ui;

        var prefab = Resources.Load("Role/npc");
        var go = (GameObject) Instantiate(prefab);
        go.transform.localPosition = new Vector3(61, -89, 1000); //set z to far from UICamera is important!
        go.transform.localScale = new Vector3(180, 180, 180);
        go.transform.localEulerAngles = new Vector3(0, 100, 0);
        _mainView.GetChild("holder").asGraph.SetNativeObject(new GoWrapper(go));
    }

    private void OnKeyDown(EventContext context)
    {
        if (context.inputEvent.keyCode == KeyCode.Escape) Application.Quit();
    }
}