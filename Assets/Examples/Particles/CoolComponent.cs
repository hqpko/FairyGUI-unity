using UnityEngine;
using FairyGUI;

public class CoolComponent : GComponent
{
    public override void ConstructFromXML(FairyGUI.Utils.XML cxml)
    {
        base.ConstructFromXML(cxml);

        var graph = GetChild("effect").asGraph;

        var prefab = Resources.Load("Flame");
        var go = (GameObject) Object.Instantiate(prefab);
        graph.SetNativeObject(new GoWrapper(go));
    }
}