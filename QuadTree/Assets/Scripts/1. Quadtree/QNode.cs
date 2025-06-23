using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public enum QNodeIndex : int
{
    UPPERLEFT = 0, // 경계선에 걸치지 않고 완전 포함
    UPPERRIGHT,
    LOWERRIGHT,
    LOWERLEFT,
    STRADDLING,  // 경계선에 걸친 경우
    OUTOFAREA // 영역 밖을 벗어난 경우. ( 입력 에러 )
};

public class QNode
{
    public QNode(Quadtree tree, QNode parent, Bounds bounds, int depth)
    {
        _tree = tree;
        _parent = parent;
        _bounds = bounds;
        _depth = depth;
    }

    public List<QNode> _children;
    public List<BoxCollider> _items = new List<BoxCollider>();
    public int _depth = 0;

    private Quadtree _tree;
    private QNode _parent;
    public Bounds _bounds;

    public void Insert(BoxCollider item)
    {
        QNodeIndex result = TestRegion(item.bounds);
        if(result == QNodeIndex.STRADDLING) // 겹치는 경우 현재 노드에 추가
        {
            _items.Add(item);
        }
        else if(result != QNodeIndex.OUTOFAREA) // 안겹치는 경우 스플릿을 수행하고 자식 노드에 추가
        {
            if(Split())
            {
                _children[(int)result].Insert(item);
            }
            else // 더 이상 쪼갤 수 없으면 현재 노드에 추가
            {
                _items.Add(item);
            }
        }
    }

    public void Query(BoxCollider item, List<QNode> possibleNodes)
    {
        possibleNodes.Add(this);
        
        if(IsSplitted)
        {
            List<QNodeIndex> quads = GetQuads(item.bounds);
            foreach (var index in quads)
            {
                _children[(int)index].Query(item, possibleNodes);
            }
        }
    }

    private List<QNodeIndex> GetQuads(Bounds bounds)
    {
        List<QNodeIndex> quads = new List<QNodeIndex>();

        bool negX = bounds.min.x <= _bounds.center.x;
        bool negY = bounds.min.y <= _bounds.center.y;
        bool posX = bounds.max.x >= _bounds.center.x;
        bool posY = bounds.max.y >= _bounds.center.y;

        if (negX && posY)
        {
            quads.Add(QNodeIndex.UPPERLEFT);
        }

        if (posX && posY)
        {
            quads.Add(QNodeIndex.UPPERRIGHT);
        }

        if (posX && negY)
        {
            quads.Add(QNodeIndex.LOWERRIGHT);
        }

        if (negX && negY)
        {
            quads.Add(QNodeIndex.LOWERLEFT);
        }

        return quads;
    }

    private QNodeIndex TestRegion(Bounds bounds)
    {
        List<QNodeIndex> quads = GetQuads(bounds);

        if(quads.Count == 0)
        {
            return QNodeIndex.OUTOFAREA;
        }
        else if(quads.Count == 1)
        {
            return quads[0];
        }
        else
        {
            return QNodeIndex.STRADDLING;
        }
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

            _children = new List<QNode>(4);
            var centerOffset = newBoundsSize * 0.5f;

            //[-x, +y] // UPPERLEFT
            centerOffset.x *= -1.0f;
            _children.Add(new QNode(_tree, this, new Bounds(_bounds.center + centerOffset, newBoundsSize), _depth + 1));

            //[+x, +y] // UPPERRIGHT
            centerOffset.x *= -1.0f;
            _children.Add(new QNode(_tree, this, new Bounds(_bounds.center + centerOffset, newBoundsSize), _depth + 1));

            //[-x, -y] // LOWERRIGHT
            centerOffset.y *= -1.0f;
            _children.Add(new QNode(_tree, this, new Bounds(_bounds.center + centerOffset, newBoundsSize), _depth + 1));

            //[+x, -y] // LOWERLEFT
            centerOffset.x *= -1.0f;
            _children.Add(new QNode(_tree, this, new Bounds(_bounds.center + centerOffset, newBoundsSize), _depth + 1));
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

    public void CollectAllNodes(Dictionary<int, List<QNode>> allNodes)
    {
        if(!allNodes.ContainsKey(_depth))
        {
            allNodes.Add(_depth, new List<QNode>());
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
