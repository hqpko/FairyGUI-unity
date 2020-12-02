using System;
using System.Collections.Generic;

namespace FairyGUI
{
    [Obsolete("Use GTree and GTreeNode instead")]
    public class TreeNode
    {
        public object data;

        public TreeNode parent { get; private set; }

        public TreeView tree { get; private set; }

        public GComponent cell { get; internal set; }

        public int level { get; private set; }

        private List<TreeNode> _children;
        private bool _expanded;

        /// <param name="hasChild"></param>
        public TreeNode(bool hasChild)
        {
            if (hasChild)
                _children = new List<TreeNode>();
        }

        public bool expanded
        {
            get => _expanded;

            set
            {
                if (_children == null)
                    return;

                if (_expanded != value)
                {
                    _expanded = value;
                    if (tree != null)
                    {
                        if (_expanded)
                            tree.AfterExpanded(this);
                        else
                            tree.AfterCollapsed(this);
                    }
                }
            }
        }

        public bool isFolder => _children != null;

        public string text
        {
            get
            {
                if (cell != null)
                    return cell.text;
                else
                    return null;
            }
        }

        /// <param name="child"></param>
        /// <returns></returns>
        public TreeNode AddChild(TreeNode child)
        {
            AddChildAt(child, _children.Count);
            return child;
        }

        /// <param name="child"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public TreeNode AddChildAt(TreeNode child, int index)
        {
            if (child == null)
                throw new Exception("child is null");

            var numChildren = _children.Count;

            if (index >= 0 && index <= numChildren)
            {
                if (child.parent == this)
                {
                    SetChildIndex(child, index);
                }
                else
                {
                    if (child.parent != null)
                        child.parent.RemoveChild(child);

                    var cnt = _children.Count;
                    if (index == cnt)
                        _children.Add(child);
                    else
                        _children.Insert(index, child);

                    child.parent = this;
                    child.level = level + 1;
                    child.SetTree(tree);
                    if (cell != null && cell.parent != null && _expanded)
                        tree.AfterInserted(child);
                }

                return child;
            }
            else
            {
                throw new Exception("Invalid child index");
            }
        }

        /// <param name="child"></param>
        /// <returns></returns>
        public TreeNode RemoveChild(TreeNode child)
        {
            var childIndex = _children.IndexOf(child);
            if (childIndex != -1) RemoveChildAt(childIndex);
            return child;
        }

        /// <param name="index"></param>
        /// <returns></returns>
        public TreeNode RemoveChildAt(int index)
        {
            if (index >= 0 && index < numChildren)
            {
                var child = _children[index];
                _children.RemoveAt(index);

                child.parent = null;
                if (tree != null)
                {
                    child.SetTree(null);
                    tree.AfterRemoved(child);
                }

                return child;
            }
            else
            {
                throw new Exception("Invalid child index");
            }
        }

        /// <param name="beginIndex"></param>
        /// <param name="endIndex"></param>
        public void RemoveChildren(int beginIndex = 0, int endIndex = -1)
        {
            if (endIndex < 0 || endIndex >= numChildren)
                endIndex = numChildren - 1;

            for (var i = beginIndex; i <= endIndex; ++i)
                RemoveChildAt(beginIndex);
        }

        /// <param name="index"></param>
        /// <returns></returns>
        public TreeNode GetChildAt(int index)
        {
            if (index >= 0 && index < numChildren)
                return _children[index];
            else
                throw new Exception("Invalid child index");
        }

        /// <param name="child"></param>
        /// <returns></returns>
        public int GetChildIndex(TreeNode child)
        {
            return _children.IndexOf(child);
        }

        /// <returns></returns>
        public TreeNode GetPrevSibling()
        {
            if (parent == null)
                return null;

            var i = parent._children.IndexOf(this);
            if (i <= 0)
                return null;

            return parent._children[i - 1];
        }

        /// <returns></returns>
        public TreeNode GetNextSibling()
        {
            if (parent == null)
                return null;

            var i = parent._children.IndexOf(this);
            if (i < 0 || i >= parent._children.Count - 1)
                return null;

            return parent._children[i + 1];
        }

        /// <param name="child"></param>
        /// <param name="index"></param>
        public void SetChildIndex(TreeNode child, int index)
        {
            var oldIndex = _children.IndexOf(child);
            if (oldIndex == -1)
                throw new Exception("Not a child of this container");

            var cnt = _children.Count;
            if (index < 0)
                index = 0;
            else if (index > cnt)
                index = cnt;

            if (oldIndex == index)
                return;

            _children.RemoveAt(oldIndex);
            _children.Insert(index, child);
            if (cell != null && cell.parent != null && _expanded)
                tree.AfterMoved(child);
        }

        /// <param name="child1"></param>
        /// <param name="child2"></param>
        public void SwapChildren(TreeNode child1, TreeNode child2)
        {
            var index1 = _children.IndexOf(child1);
            var index2 = _children.IndexOf(child2);
            if (index1 == -1 || index2 == -1)
                throw new Exception("Not a child of this container");
            SwapChildrenAt(index1, index2);
        }

        /// <param name="index1"></param>
        /// <param name="index2"></param>
        public void SwapChildrenAt(int index1, int index2)
        {
            var child1 = _children[index1];
            var child2 = _children[index2];

            SetChildIndex(child1, index2);
            SetChildIndex(child2, index1);
        }

        public int numChildren => null == _children ? 0 : _children.Count;

        internal void SetTree(TreeView value)
        {
            tree = value;
            if (tree != null && tree.treeNodeWillExpand != null && _expanded)
                tree.treeNodeWillExpand(this, true);

            if (_children != null)
            {
                var cnt = _children.Count;
                for (var i = 0; i < cnt; i++)
                {
                    var node = _children[i];
                    node.level = level + 1;
                    node.SetTree(value);
                }
            }
        }
    }
}