using System.Collections;
using UnityEngine;
using FairyGUI;

public class EmitNumbersMain : MonoBehaviour
{
    private Transform _npc1;
    private Transform _npc2;
    private bool _finished;

    private void Start()
    {
        Application.targetFrameRate = 60;
        Stage.inst.onKeyDown.Add(OnKeyDown);

        _npc1 = GameObject.Find("npc1").transform;
        _npc2 = GameObject.Find("npc2").transform;

        StartCoroutine(RunTest());
    }

    private void OnDisable()
    {
        _finished = true;
    }

    private IEnumerator RunTest()
    {
        while (!_finished)
        {
            EmitManager.inst.Emit(_npc1, 0, Random.Range(100, 100000), Random.Range(0, 10) == 5);
            EmitManager.inst.Emit(_npc2, 1, Random.Range(100, 100000), Random.Range(0, 10) == 5);
            yield return new WaitForSeconds(0.3f);
        }
    }

    private void OnKeyDown(EventContext context)
    {
        if (context.inputEvent.keyCode == KeyCode.Escape) Application.Quit();
    }
}