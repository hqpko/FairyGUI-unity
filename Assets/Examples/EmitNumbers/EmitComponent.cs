using UnityEngine;
using FairyGUI;

public class EmitComponent : GComponent
{
    private GLoader _symbolLoader;
    private GTextField _numberText;
    private Transform _owner;

    private const float OFFSET_ADDITION = 2.2f;
    private static Vector2 JITTER_FACTOR = new Vector2(80, 80);

    public EmitComponent()
    {
        touchable = false;

        _symbolLoader = new GLoader();
        _symbolLoader.autoSize = true;
        AddChild(_symbolLoader);

        _numberText = new GTextField();
        _numberText.autoSize = AutoSizeType.Both;

        AddChild(_numberText);
    }

    public void SetHurt(Transform owner, int type, long hurt, bool critical)
    {
        _owner = owner;

        var tf = _numberText.textFormat;
        if (type == 0)
            tf.font = EmitManager.inst.hurtFont1;
        else
            tf.font = EmitManager.inst.hurtFont2;
        _numberText.textFormat = tf;
        _numberText.text = "-" + hurt;

        if (critical)
            _symbolLoader.url = EmitManager.inst.criticalSign;
        else
            _symbolLoader.url = "";

        UpdateLayout();

        alpha = 1;
        UpdatePosition(Vector2.zero);
        var rnd = Vector2.Scale(Random.insideUnitCircle, JITTER_FACTOR);
        var toX = (int) rnd.x * 2;
        var toY = (int) rnd.y * 2;

        EmitManager.inst.view.AddChild(this);
        GTween.To(Vector2.zero, new Vector2(toX, toY), 1f).SetTarget(this)
            .OnUpdate((GTweener tweener) => { UpdatePosition(tweener.value.vec2); }).OnComplete(OnCompleted);
        TweenFade(0, 0.5f).SetDelay(0.5f);
    }

    private void UpdateLayout()
    {
        SetSize(_symbolLoader.width + _numberText.width, Mathf.Max(_symbolLoader.height, _numberText.height));
        _numberText.SetXY(_symbolLoader.width > 0 ? _symbolLoader.width + 2 : 0,
            (height - _numberText.height) / 2);
        _symbolLoader.y = (height - _symbolLoader.height) / 2;
    }

    private void UpdatePosition(Vector2 pos)
    {
        var ownerPos = _owner.position;
        ownerPos.y += OFFSET_ADDITION;
        var screenPos = Camera.main.WorldToScreenPoint(ownerPos);
        screenPos.y = Screen.height - screenPos.y; //convert to Stage coordinates system

        Vector3 pt = GRoot.inst.GlobalToLocal(screenPos);
        SetXY(Mathf.RoundToInt(pt.x + pos.x - actualWidth / 2), Mathf.RoundToInt(pt.y + pos.y - height));
    }

    private void OnCompleted()
    {
        _owner = null;
        EmitManager.inst.view.RemoveChild(this);
        EmitManager.inst.ReturnComponent(this);
    }

    public void Cancel()
    {
        _owner = null;
        if (parent != null)
        {
            GTween.Kill(this);
            EmitManager.inst.view.RemoveChild(this);
        }

        EmitManager.inst.ReturnComponent(this);
    }
}