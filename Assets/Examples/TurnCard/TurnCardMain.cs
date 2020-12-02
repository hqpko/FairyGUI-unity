using UnityEngine;
using FairyGUI;

public class TurnCardMain : MonoBehaviour
{
    private GComponent _mainView;
    private Card _c0;
    private Card _c1;

    private void Awake()
    {
        Application.targetFrameRate = 60;
        Stage.inst.onKeyDown.Add(OnKeyDown);

        UIPackage.AddPackage("UI/TurnCard");
        UIObjectFactory.SetPackageItemExtension("ui://TurnCard/CardComponent", typeof(Card));
    }

    private void Start()
    {
        _mainView = GetComponent<UIPanel>().ui;

        _c0 = (Card) _mainView.GetChild("c0");

        _c1 = (Card) _mainView.GetChild("c1");
        _c1.SetPerspective();

        _c0.onClick.Add(__clickCard);
        _c1.onClick.Add(__clickCard);
    }

    private void __clickCard(EventContext context)
    {
        var card = (Card) context.sender;
        card.Turn();
    }

    private void OnKeyDown(EventContext context)
    {
        if (context.inputEvent.keyCode == KeyCode.Escape) Application.Quit();
    }
}