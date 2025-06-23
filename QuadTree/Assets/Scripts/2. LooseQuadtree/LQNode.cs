using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public enum LQNodeIndex : int
{
    UPPERLEFT = 0, // 경계선에 걸치지 않고 완전 포함
    UPPERRIGHT,
    LOWERRIGHT,
    LOWERLEFT,
    STRADDLING,  // 경계선에 걸친 경우
    OUTOFAREA // 영역 밖을 벗어난 경우. ( 입력 에러 )
};

public class LQNode
{
    public LQNode(LQuadtree tree, LQNode parent, Bounds bounds, int depth)
    {
        _tree = tree;
        _parent = parent;
        _bounds = bounds;
        _depth = depth;
        _qbounds = bounds;
        _qbounds.size *= _tree._constantK;
    }

    public List<LQNode> _children;
    public List<BoxCollider> _items = new List<BoxCollider>();
    public int _depth = 0;

    private LQuadtree _tree;
    private LQNode _parent;
    public Bounds _bounds;
    public Bounds _qbounds;

    public void InsertAtDepth(BoxCollider item, int targetDepth)
    {
        if(_depth < targetDepth) // 현재 깊이가 목표 깊이보다 얕으면 다음 깊이로 넘김.
        {
            if(Split())
            {
                _children[(int)TestRegion(item.bounds)].InsertAtDepth(item, targetDepth);
            }
        }
        else if(_depth == targetDepth) // 현재 깊이와 목표 깊이가 같으면 목록에 추가
        {
            _items.Add(item);
        }
    }

    public void Query(BoxCollider item, List<LQNode> possibleNodes)
    {
        possibleNodes.Add(this);

        if (IsSplitted)
        {
            List<LQNode> quads = GetQuads(item.bounds);
            foreach (var quad in quads)
            {
                quad.Query(item, possibleNodes);
            }
        }
    }

    private List<LQNode> GetQuads(Bounds bounds)
    {
        List<LQNode> quads = new List<LQNode>();

        foreach (var child in _children)
        {
            if(child._qbounds.Intersects(bounds))
            {
                quads.Add(child);
            }
        }

        return quads;
    }

    private LQNodeIndex TestRegion(Bounds bounds)
    {
        bool negX = bounds.center.x <= _bounds.center.x;
        bool negY = bounds.center.y <= _bounds.center.y;
        bool posX = bounds.center.x > _bounds.center.x;
        bool posY = bounds.center.y > _bounds.center.y;

        if (negX && posY)
        {
            return LQNodeIndex.UPPERLEFT;
        }

        if (posX && posY)
        {
            return LQNodeIndex.UPPERRIGHT;
        }

        if (posX && negY)
        {
            return LQNodeIndex.LOWERRIGHT;
        }

        if (negX && negY)
        {
            return LQNodeIndex.LOWERLEFT;
        }

        return LQNodeIndex.OUTOFAREA;
    }

    private bool Split()
    {
        if(_depth == _tree._maxDepth) // 지정한 최대 깊이에 도달
        {
            return false;
        }

        if(!IsSplitted)
        {
            var newBoundsSize = _bounds.size * 0.5f;

            _children = new List<LQNode>(4);
            var centerOffset = newBoundsSize * 0.5f;

            //[-x, +y] // UPPERLEFT
            centerOffset.x *= -1.0f;
            _children.Add(new LQNode(_tree, this, new Bounds(_bounds.center + centerOffset, newBoundsSize), _depth + 1));

            //[+x, +y] // UPPERRIGHT
            centerOffset.x *= -1.0f;
            _children.Add(new LQNode(_tree, this, new Bounds(_bounds.center + centerOffset, newBoundsSize), _depth + 1));

            //[-x, -y] // LOWERRIGHT
            centerOffset.y *= -1.0f;
            _children.Add(new LQNode(_tree, this, new Bounds(_bounds.center + centerOffset, newBoundsSize), _depth + 1));

            //[+x, -y] // LOWERLEFT
            centerOffset.x *= -1.0f;
            _children.Add(new LQNode(_tree, this, new Bounds(_bounds.center + centerOffset, newBoundsSize), _depth + 1));
        }

        return true;
    }

    public void Clear()
    {
        if(_children != null)
        {
            foreach(var node in _children)
            {
                node.Clear();
            }
        }

        _items.Clear();
        _children.Clear();
    }

    public void CollectAllNodes(Dictionary<int, List<LQNode>> allNodes)
    {
        if(!allNodes.ContainsKey(_depth))
        {
            allNodes.Add(_depth, new List<LQNode>());
        }
        allNodes[_depth].Add(this);

        if (IsSplitted)
        {
            foreach(var node in _children)
            {
                node.CollectAllNodes(allNodes);
            }
        }
    }

    public void DrawBounds()
    {
        Gizmos.DrawWireCube(_bounds.center, _bounds.size);
        Handles.Label(_bounds.center, _items.Count.ToString());

        if (IsSplitted)
        {
            foreach (var node in _children)
            {
                node.DrawBounds();
            }
        }
        else
        {
            if(_items.Count > 0)
            {
                var xExtent = _bounds.extents;
                var yExtent = _bounds.extents;
                xExtent.y = 0;
                yExtent.x = 0;
                Gizmos.DrawLine(_bounds.center - xExtent, _bounds.center + xExtent);
                Gizmos.DrawLine(_bounds.center - yExtent, _bounds.center + yExtent);
            }
        }
    }

    private bool IsSplitted
    {
        get { return (_children != null) && (_children.Count > 0); }
    }
}
