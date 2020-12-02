using UnityEngine;
using FairyGUI;

public class CooldownMain : MonoBehaviour
{
    private GComponent _mainView;

    private GButton _btn0;
    private GImage _mask0;
    private float _time1;

    private GButton _btn1;
    private GImage _mask1;
    private float _time2;

    private void Start()
    {
        Application.targetFrameRate = 60;

        Stage.inst.onKeyDown.Add(OnKeyDown);

        _mainView = gameObject.GetComponent<UIPanel>().ui;

        _btn0 = _mainView.GetChild("b0").asButton;
        _btn0.icon = "Cooldown/k0";
        _time1 = 5;
        _mask0 = _btn0.GetChild("mask").asImage;

        _btn1 = _mainView.GetChild("b1").asButton;
        _btn1.icon = "Cooldown/k1";
        _time2 = 10;
        _mask1 = _btn1.GetChild("mask").asImage;
    }

    private void Update()
    {
        _time1 -= Time.deltaTime;
        if (_time1 < 0)
            _time1 = 5;
        _mask0.fillAmount = 1 - (5 - _time1) / 5f;

        _time2 -= Time.deltaTime;
        if (_time2 < 0)
            _time2 = 10;
        _btn1.text = string.Empty + Mathf.RoundToInt(_time2);
        _mask1.fillAmount = 1 - (10 - _time2) / 10f;
    }

    private void OnKeyDown(EventContext context)
    {
        if (context.inputEvent.keyCode == KeyCode.Escape) Application.Quit();
    }
}