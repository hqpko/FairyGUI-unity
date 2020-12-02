using System;
using System.Collections.Generic;
using FairyGUI.Utils;

namespace FairyGUI
{
    public class GTree : GList
    {
        public delegate void TreeNodeRenderDelegate(GTreeNode node, GComponent obj);

        public delegate void TreeNodeWillExpandDelegate(GTreeNode node, bool expand);

        /// <summary>
        /// 当TreeNode需要更新时回调
        /// </summary>
        public TreeNodeRenderDelegate treeNodeRender;

        /// <summary>
        /// 当TreeNode即将展开或者收缩时回调。可以在回调中动态增加子节点。
        /// </summary>
        public TreeNodeWillExpandDelegate treeNodeWillExpand;

        private int _indent;
        private GTreeNode _rootNode;
        private int _clickToExpand;
        private bool _expandedStatusInEvt;

        private static List<int> helperIntList = new List<int>();

        public GTree()
        {
            _indent = 30;

            _rootNode = new GTreeNode(true);
            _rootNode._SetTree(this);
            _rootNode.expanded = true;
        }

        /// <summary>
        /// TreeView的顶层节点，这是个虚拟节点，也就是他不会显示出来。
        /// </summary>
        public GTreeNode rootNode => _rootNode;

        /// <summary>
        /// TreeView每级的缩进，单位像素。
        /// </summary>
        public int indent
        {
            get => _indent;
            set => _indent = value;
        }

        public int clickToExpand
        {
            get => _clickToExpand;
            set => _clickToExpand = value;
        }

        /// <returns></returns>
        public GTreeNode GetSelectedNode()
        {
            var i = selectedIndex;
            if (i != -1)
                return (GTreeNode) GetChildAt(i)._treeNode;
            else
                return null;
        }

        /// <returns></returns>
        public List<GTreeNode> GetSelectedNodes()
        {
            return GetSelectedNodes(null);
        }

        /// <param name="result"></param>
        /// <returns></returns>
        public List<GTreeNode> GetSelectedNodes(List<GTreeNode> result)
        {
            if (result == null)
                result = new List<GTreeNode>();
            helperIntList.Clear();
            var sels = GetSelection(helperIntList);
            var cnt = sels.Count;
            for (var i = 0; i < cnt; i++)
            {
                var node = GetChildAt(sels[i])._treeNode;
                result.Add(node);
            }

            return result;
        }

        /// <param name="node"></param>
        public void SelectNode(GTreeNode node)
        {
            SelectNode(node, false);
        }

        /// <param name="node"></param>
        /// <param name="scrollItToView"></param>
        public void SelectNode(GTreeNode node, bool scrollItToView)
        {
            var parentNode = node.parent;
            while (parentNode != null && parentNode != _rootNode)
            {
                parentNode.expanded = true;
                parentNode = parentNode.parent;
            }

            AddSelection(GetChildIndex(node.cell), scrollItToView);
        }

        /// <param name="node"></param>
        public void UnselectNode(GTreeNode node)
        {
            RemoveSelection(GetChildIndex(node.cell));
        }

        public void ExpandAll()
        {
            ExpandAll(_rootNode);
        }

        /// <param name="folderNode"></param>
        public void ExpandAll(GTreeNode folderNode)
        {
            folderNode.expanded = true;
            var cnt = folderNode.numChildren;
            for (var i = 0; i < cnt; i++)
            {
                var node = folderNode.GetChildAt(i);
                if (node.isFolder)
                    ExpandAll(node);
            }
        }

        /// <param name="folderNode"></param>
        public void CollapseAll()
        {
            CollapseAll(_rootNode);
        }

        /// <param name="folderNode"></param>
        public void CollapseAll(GTreeNode folderNode)
        {
            if (folderNode != _rootNode)
                folderNode.expanded = false;
            var cnt = folderNode.numChildren;
            for (var i = 0; i < cnt; i++)
            {
                var node = folderNode.GetChildAt(i);
                if (node.isFolder)
                    CollapseAll(node);
            }
        }

        /// <param name="node"></param>
        private void CreateCell(GTreeNode node)
        {
            var child =
                itemPool.GetObject(string.IsNullOrEmpty(node._resURL) ? defaultItem : node._resURL) as GComponent;
            if (child == null)
                throw new Exception("FairyGUI: cannot create tree node object.");
            child.displayObject.home = displayObject.cachedTransform;
            child._treeNode = node;
            node._cell = child;

            var indentObj = node.cell.GetChild("indent");
            if (indentObj != null)
                indentObj.width = (node.level - 1) * indent;

            Controller cc;

            cc = child.GetController("expanded");
            if (cc != null)
            {
                cc.onChanged.Add(__expandedStateChanged);
                cc.selectedIndex = node.expanded ? 1 : 0;
            }

            cc = child.GetController("leaf");
            if (cc != null)
                cc.selectedIndex = node.isFolder ? 0 : 1;

            if (node.isFolder)
                child.onTouchBegin.Add(__cellTouchBegin);

            if (treeNodeRender != null)
                treeNodeRender(node, node._cell);
        }

        /// <param name="node"></param>
        internal void _AfterInserted(GTreeNode node)
        {
            if (node._cell == null)
                CreateCell(node);

            var index = GetInsertIndexForNode(node);
            AddChildAt(node.cell, index);
            if (treeNodeRender != null)
                treeNodeRender(node, node._cell);

            if (node.isFolder && node.expanded)
                CheckChildren(node, index);
        }

        /// <param name="node"></param>
        /// <returns></returns>
        private int GetInsertIndexForNode(GTreeNode node)
        {
            var prevNode = node.GetPrevSibling();
            if (prevNode == null)
                prevNode = node.parent;
            var insertIndex = GetChildIndex(prevNode.cell) + 1;
            var myLevel = node.level;
            var cnt = numChildren;
            for (var i = insertIndex; i < cnt; i++)
            {
                var testNode = GetChildAt(i)._treeNode;
                if (testNode.level <= myLevel)
                    break;

                insertIndex++;
            }

            return insertIndex;
        }

        /// <param name="node"></param>
        internal void _AfterRemoved(GTreeNode node)
        {
            RemoveNode(node);
        }

        /// <param name="node"></param>
        internal void _AfterExpanded(GTreeNode node)
        {
            if (node == _rootNode)
            {
                CheckChildren(_rootNode, 0);
                return;
            }

            if (treeNodeWillExpand != null)
                treeNodeWillExpand(node, true);

            if (node._cell == null)
                return;

            if (treeNodeRender != null)
                treeNodeRender(node, node._cell);

            var cc = node._cell.GetController("expanded");
            if (cc != null)
                cc.selectedIndex = 1;

            if (node._cell.parent != null)
                CheckChildren(node, GetChildIndex(node._cell));
        }

        /// <param name="node"></param>
        internal void _AfterCollapsed(GTreeNode node)
        {
            if (node == _rootNode)
            {
                CheckChildren(_rootNode, 0);
                return;
            }

            if (treeNodeWillExpand != null)
                treeNodeWillExpand(node, false);

            if (node._cell == null)
                return;

            if (treeNodeRender != null)
                treeNodeRender(node, node._cell);

            var cc = node._cell.GetController("expanded");
            if (cc != null)
                cc.selectedIndex = 0;

            if (node._cell.parent != null)
                HideFolderNode(node);
        }

        /// <param name="node"></param>
        internal void _AfterMoved(GTreeNode node)
        {
            var startIndex = GetChildIndex(node._cell);
            int endIndex;
            if (node.isFolder)
                endIndex = GetFolderEndIndex(startIndex, node.level);
            else
                endIndex = startIndex + 1;
            var insertIndex = GetInsertIndexForNode(node);
            var cnt = endIndex - startIndex;

            if (insertIndex < startIndex)
                for (var i = 0; i < cnt; i++)
                {
                    var obj = GetChildAt(startIndex + i);
                    SetChildIndex(obj, insertIndex + i);
                }
            else
                for (var i = 0; i < cnt; i++)
                {
                    var obj = GetChildAt(startIndex);
                    SetChildIndex(obj, insertIndex);
                }
        }

        private int GetFolderEndIndex(int startIndex, int level)
        {
            var cnt = numChildren;
            for (var i = startIndex + 1; i < cnt; i++)
            {
                var node = GetChildAt(i)._treeNode;
                if (node.level <= level)
                    return i;
            }

            return cnt;
        }

        /// <param name="folderNode"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        private int CheckChildren(GTreeNode folderNode, int index)
        {
            var cnt = folderNode.numChildren;
            for (var i = 0; i < cnt; i++)
            {
                index++;
                var node = folderNode.GetChildAt(i);
                if (node.cell == null)
                    CreateCell(node);

                if (node.cell.parent == null)
                    AddChildAt(node.cell, index);

                if (node.isFolder && node.expanded)
                    index = CheckChildren(node, index);
            }

            return index;
        }

        /// <param name="folderNode"></param>
        private void HideFolderNode(GTreeNode folderNode)
        {
            var cnt = folderNode.numChildren;
            for (var i = 0; i < cnt; i++)
            {
                var node = folderNode.GetChildAt(i);
                if (node.cell != null && node.cell.parent != null)
                    RemoveChild(node.cell);

                if (node.isFolder && node.expanded)
                    HideFolderNode(node);
            }
        }

        /// <param name="node"></param>
        private void RemoveNode(GTreeNode node)
        {
            if (node.cell != null)
            {
                if (node.cell.parent != null)
                    RemoveChild(node.cell);
                itemPool.ReturnObject(node.cell);
                node._cell._treeNode = null;
                node._cell = null;
            }

            if (node.isFolder)
            {
                var cnt = node.numChildren;
                for (var i = 0; i < cnt; i++)
                {
                    var node2 = node.GetChildAt(i);
                    RemoveNode(node2);
                }
            }
        }

        private void __cellTouchBegin(EventContext context)
        {
            var node = ((GObject) context.sender)._treeNode;
            _expandedStatusInEvt = node.expanded;
        }

        private void __expandedStateChanged(EventContext context)
        {
            var cc = (Controller) context.sender;
            var node = cc.parent._treeNode;
            node.expanded = cc.selectedIndex == 1;
        }

        protected override void DispatchItemEvent(GObject item, EventContext context)
        {
            if (_clickToExpand != 0)
            {
                var node = item._treeNode;
                if (node != null && _expandedStatusInEvt == node.expanded)
                {
                    if (_clickToExpand == 2)
                    {
                        if (context.inputEvent.isDoubleClick)
                            node.expanded = !node.expanded;
                    }
                    else
                    {
                        node.expanded = !node.expanded;
                    }
                }
            }

            base.DispatchItemEvent(item, context);
        }

        public override void Setup_BeforeAdd(ByteBuffer buffer, int beginPos)
        {
            base.Setup_BeforeAdd(buffer, beginPos);

            buffer.Seek(beginPos, 9);

            _indent = buffer.ReadInt();
            _clickToExpand = buffer.ReadByte();
        }

        protected override void ReadItems(ByteBuffer buffer)
        {
            int nextPos;
            string str;
            bool isFolder;
            GTreeNode lastNode = null;
            int level;
            var prevLevel = 0;

            int cnt = buffer.ReadShort();
            for (var i = 0; i < cnt; i++)
            {
                nextPos = buffer.ReadShort();
                nextPos += buffer.position;

                str = buffer.ReadS();
                if (str == null)
                {
                    str = defaultItem;
                    if (str == null)
                    {
                        buffer.position = nextPos;
                        continue;
                    }
                }

                isFolder = buffer.ReadBool();
                level = buffer.ReadByte();

                var node = new GTreeNode(isFolder, str);
                node.expanded = true;
                if (i == 0)
                {
                    _rootNode.AddChild(node);
                }
                else
                {
                    if (level > prevLevel)
                    {
                        lastNode.AddChild(node);
                    }
                    else if (level < prevLevel)
                    {
                        for (var j = level; j <= prevLevel; j++)
                            lastNode = lastNode.parent;
                        lastNode.AddChild(node);
                    }
                    else
                    {
                        lastNode.parent.AddChild(node);
                    }
                }

                lastNode = node;
                prevLevel = level;

                SetupItem(buffer, node.cell);

                buffer.position = nextPos;
            }
        }
    }
}