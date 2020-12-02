using UnityEngine;
using FairyGUI;

public class HitTestMain : MonoBehaviour
{
    private Transform cube;

    private void Start()
    {
        Application.targetFrameRate = 60;

        cube = GameObject.Find("Cube").transform;

        Stage.inst.onTouchBegin.Add(OnTouchBegin);
    }

    private void OnTouchBegin()
    {
        if (!Stage.isTouchOnUI)
        {
            RaycastHit hit;
            var ray = Camera.main.ScreenPointToRay(new Vector2(Stage.inst.touchPosition.x,
                Screen.height - Stage.inst.touchPosition.y));
            if (Physics.Raycast(ray, out hit))
                if (hit.transform == cube)
                    Debug.Log("Hit the cube");
        }
    }

    private void OnKeyDown(EventContext context)
    {
        if (context.inputEvent.keyCode == KeyCode.Escape) Application.Quit();
    }
}