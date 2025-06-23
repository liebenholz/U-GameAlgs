using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;

public static class TransformKdExtension
{
    public static float GetKdValue(this Transform self, int dimension, int depth)
    {
        return self.position[depth % dimension];
    }
}

public class Kdtree
{
    public List<KNode> _nodes;
    public int _dimension = 2;
    private KNode _root = null;

    public Kdtree(int dimension)
    {
        _nodes = new List<KNode>();
        _dimension = dimension;
    }

    public void Add(Transform newTransform)
    {
        int depth = 0;

        if (_root == null)
        {
            _root = new KNode(_dimension, null, newTransform, depth);
            _nodes.Add(_root);
            return;
        }

        KNode target = _root;
        while(target != null)
        {
            if (newTransform.position == target.Transform.position) // 같으면 추가하지 않고 스킵
            {
                Debug.LogError("같은 값을 가진 노드를 추가할 수 없습니다!");
                break;
            }

            if (newTransform.GetKdValue(_dimension, depth) < target.KdValue)
            {
                if (target.IsLeftOpen)
                {
                    target.Left = new KNode(_dimension, null, newTransform, depth + 1);
                    _nodes.Add(target.Left);
                    break;
                }
                else
                {
                    target = target.Left;
                }
            }
            else
            {
                if (target.IsRightOpen)
                {
                    target.Right = new KNode(_dimension, null, newTransform, depth + 1);
                    _nodes.Add(target.Left);
                    break;
                }
                else
                {
                    target = target.Right;
                }
            }

            depth++;
        }
    }

    public Transform NearestQuery(Transform queryTransform, List<KNode> searchedNode)
    {
        return _root.CheckNearestNode(queryTransform, _root, searchedNode).Transform;
    }

    public void Clear()
    {
        _nodes.Clear();
    }

    public int Count
    {
        get
        {
            return _nodes.Count;
        }
    }

    public KNode Root
    {
        get { return _root; }
    }

}
