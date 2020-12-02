using UnityEngine;
using FairyGUI;

public class TreeViewMain : MonoBehaviour
{
    private GComponent _mainView;
    private GTree _tree1;
    private GTree _tree2;
    private string _fileURL;

    private void Awake()
    {
        Application.targetFrameRate = 60;
        Stage.inst.onKeyDown.Add(OnKeyDown);
    }

    private void Start()
    {
        _mainView = GetComponent<UIPanel>().ui;

        _fileURL = "ui://TreeView/file";

        _tree1 = _mainView.GetChild("tree").asTree;
        _tree1.onClickItem.Add(__clickNode);
        _tree2 = _mainView.GetChild("tree2").asTree;
        _tree2.onClickItem.Add(__clickNode);
        _tree2.treeNodeRender = RenderTreeNode;

        var topNode = new GTreeNode(true);
        topNode.data = "I'm a top node";
        _tree2.rootNode.AddChild(topNode);
        for (var i = 0; i < 5; i++)
        {
            var node = new GTreeNode(false);
            node.data = "Hello " + i;
            topNode.AddChild(node);
        }

        var aFolderNode = new GTreeNode(true);
        aFolderNode.data = "A folder node";
        topNode.AddChild(aFolderNode);
        for (var i = 0; i < 5; i++)
        {
            var node = new GTreeNode(false);
            node.data = "Good " + i;
            aFolderNode.AddChild(node);
        }

        for (var i = 0; i < 3; i++)
        {
            var node = new GTreeNode(false);
            node.data = "World " + i;
            topNode.AddChild(node);
        }

        var anotherTopNode = new GTreeNode(false);
        anotherTopNode.data = new string[] {"I'm a top node too", "ui://TreeView/heart"};
        _tree2.rootNode.AddChild(anotherTopNode);
    }

    private void RenderTreeNode(GTreeNode node, GComponent obj)
    {
        if (node.isFolder)
        {
            obj.text = (string) node.data;
        }
        else if (node.data is string[])
        {
            obj.icon = ((string[]) node.data)[1];
            obj.text = ((string[]) node.data)[0];
        }
        else
        {
            obj.icon = _fileURL;
            obj.text = (string) node.data;
        }
    }

    private void __clickNode(EventContext context)
    {
        var node = ((GObject) context.data).treeNode;
        Debug.Log(node.text);
    }

    private void OnKeyDown(EventContext context)
    {
        if (context.inputEvent.keyCode == KeyCode.Escape) Application.Quit();
    }
}