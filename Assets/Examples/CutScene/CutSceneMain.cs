using System.Collections;
using UnityEngine;
using FairyGUI;

/// <summary>
/// Demonstrated the simple flow of a game.
/// </summary>
public class CutSceneMain : MonoBehaviour
{
    private void Start()
    {
        Application.targetFrameRate = 60;
        Stage.inst.onKeyDown.Add(OnKeyDown);

        UIPackage.AddPackage("UI/CutScene");

        LevelManager.inst.Init();
        LevelManager.inst.LoadLevel("scene1");
    }

    private void OnKeyDown(EventContext context)
    {
        if (context.inputEvent.keyCode == KeyCode.Escape) Application.Quit();
    }
}