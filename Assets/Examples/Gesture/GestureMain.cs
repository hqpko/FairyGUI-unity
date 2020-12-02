using UnityEngine;
using FairyGUI;

public class GestureMain : MonoBehaviour
{
    private GComponent _mainView;
    private Transform _ball;

    private void Awake()
    {
        Application.targetFrameRate = 60;
        Stage.inst.onKeyDown.Add(OnKeyDown);

        UIPackage.AddPackage("UI/Gesture");
    }

    private void Start()
    {
        _mainView = GetComponent<UIPanel>().ui;
        var holder = _mainView.GetChild("holder");

        _ball = GameObject.Find("Globe").transform;

        var gesture1 = new SwipeGesture(holder);
        gesture1.onMove.Add(OnSwipeMove);
        gesture1.onEnd.Add(OnSwipeEnd);

        var gesture2 = new LongPressGesture(holder);
        gesture2.once = false;
        gesture2.onAction.Add(OnHold);

        var gesture3 = new PinchGesture(holder);
        gesture3.onAction.Add(OnPinch);

        var gesture4 = new RotationGesture(holder);
        gesture4.onAction.Add(OnRotate);
    }

    private void OnSwipeMove(EventContext context)
    {
        var gesture = (SwipeGesture) context.sender;
        var v = new Vector3();
        if (Mathf.Abs(gesture.delta.x) > Mathf.Abs(gesture.delta.y))
        {
            v.y = -Mathf.Round(gesture.delta.x);
            if (Mathf.Abs(v.y) < 2) //消除手抖的影响
                return;
        }
        else
        {
            v.x = -Mathf.Round(gesture.delta.y);
            if (Mathf.Abs(v.x) < 2)
                return;
        }

        _ball.Rotate(v, Space.World);
    }

    private void OnSwipeEnd(EventContext context)
    {
        var gesture = (SwipeGesture) context.sender;
        var v = new Vector3();
        if (Mathf.Abs(gesture.velocity.x) > Mathf.Abs(gesture.velocity.y))
        {
            v.y = -Mathf.Round(Mathf.Sign(gesture.velocity.x) * Mathf.Sqrt(Mathf.Abs(gesture.velocity.x)));
            if (Mathf.Abs(v.y) < 2)
                return;
        }
        else
        {
            v.x = -Mathf.Round(Mathf.Sign(gesture.velocity.y) * Mathf.Sqrt(Mathf.Abs(gesture.velocity.y)));
            if (Mathf.Abs(v.x) < 2)
                return;
        }

        GTween.To(v, Vector3.zero, 0.3f).SetTarget(_ball).OnUpdate(
            (GTweener tweener) => { _ball.Rotate(tweener.deltaValue.vec3, Space.World); });
    }

    private void OnHold(EventContext context)
    {
        GTween.Shake(_ball.transform.localPosition, 0.05f, 0.5f).SetTarget(_ball).OnUpdate(
            (GTweener tweener) =>
            {
                _ball.transform.localPosition =
                    new Vector3(tweener.value.x, tweener.value.y, _ball.transform.localPosition.z);
            });
    }

    private void OnPinch(EventContext context)
    {
        GTween.Kill(_ball);

        var gesture = (PinchGesture) context.sender;
        var newValue = Mathf.Clamp(_ball.localScale.x + gesture.delta, 0.3f, 2);
        _ball.localScale = new Vector3(newValue, newValue, newValue);
    }

    private void OnRotate(EventContext context)
    {
        GTween.Kill(_ball);

        var gesture = (RotationGesture) context.sender;
        _ball.Rotate(Vector3.forward, -gesture.delta, Space.World);
    }

    private void OnKeyDown(EventContext context)
    {
        if (context.inputEvent.keyCode == KeyCode.Escape) Application.Quit();
    }
}