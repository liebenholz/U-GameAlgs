using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public enum ONodeIndex : int
{
    TOPUPPERLEFT = 0, // 경계선에 걸치지 않고 완전 포함
    TOPUPPERRIGHT,
    TOPLOWERRIGHT,
    TOPLOWERLEFT,
    BOTTOMUPPERLEFT,
    BOTTOMUPPERRIGHT,
    BOTTOMLOWERRIGHT,
    BOTTOMLOWERLEFT,
    STRADDLING,  // 경계선에 걸친 경우
    OUTOFAREA // 영역 밖을 벗어난 경우. ( 입력 에러 )
};

public class ONode
{
    public ONode(Octree tree, ONode parent, Bounds bounds, int depth)
    {
        _tree = tree;
        _parent = parent;
        _bounds = bounds;
        _depth = depth;
    }

    public List<ONode> _children;
    public List<BoxCollider> _items = new List<BoxCollider>();
    public int _depth = 0;

    private Octree _tree;
    private ONode _parent;
    public Bounds _bounds;

    public void Insert(BoxCollider item)
    {
        ONodeIndex result = TestRegion(item.bounds);
        if(result == ONodeIndex.STRADDLING) // 겹치는 경우 현재 노드에 추가
        {
            _items.Add(item);
        }
        else if (result != ONodeIndex.OUTOFAREA) // 안겹치는 경우 스플릿을 수행하고 자식 노드에 추가
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

    public void Query(BoxCollider item, List<ONode> possibleNodes)
    {
        possibleNodes.Add(this);
        
        if(IsSplitted)
        {
            List<ONodeIndex> octants = GetOctants(item.bounds);
            foreach (var index in octants)
            {
                _children[(int)index].Query(item, possibleNodes);
            }
        }
    }

    private List<ONodeIndex> GetOctants(Bounds bounds)
    {
        List<ONodeIndex> octants = new List<ONodeIndex>();

        bool negX = bounds.min.x <= _bounds.center.x;
        bool negY = bounds.min.y <= _bounds.center.y;
        bool negZ = bounds.min.z <= _bounds.center.z;
        bool posX = bounds.max.x >= _bounds.center.x;
        bool posY = bounds.max.y >= _bounds.center.y;
        bool posZ = bounds.max.z >= _bounds.center.z;

        if (posZ && negX && posY)
        {
            octants.Add(ONodeIndex.TOPUPPERLEFT);
        }

        if (posZ && posX && posY)
        {
            octants.Add(ONodeIndex.TOPUPPERRIGHT);
        }

        if (posZ && posX && negY)
        {
            octants.Add(ONodeIndex.TOPLOWERRIGHT);
        }

        if (posZ && negX && negY)
        {
            octants.Add(ONodeIndex.TOPLOWERLEFT);
        }

        if (negZ && negX && posY)
        {
            octants.Add(ONodeIndex.BOTTOMUPPERLEFT);
        }

        if (negZ && posX && posY)
        {
            octants.Add(ONodeIndex.BOTTOMUPPERRIGHT);
        }

        if (negZ && posX && negY)
        {
            octants.Add(ONodeIndex.BOTTOMLOWERRIGHT);
        }

        if (negZ && negX && negY)
        {
            octants.Add(ONodeIndex.BOTTOMLOWERLEFT);
        }

        return octants;
    }

    private ONodeIndex TestRegion(Bounds bounds)
    {
        List<ONodeIndex> octants = GetOctants(bounds);

        if(octants.Count == 0)
        {
            return ONodeIndex.OUTOFAREA;
        }
        else if(octants.Count == 1)
        {
            return octants[0];
        }
        else
        {
            return ONodeIndex.STRADDLING;
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

            _children = new List<ONode>(8);
            var centerOffset = newBoundsSize * 0.5f;

            //[-x, +y, +z] // TOPUPPERLEFT
            centerOffset.x *= -1.0f;
            _children.Add(new ONode(_tree, this, new Bounds(_bounds.center + centerOffset, newBoundsSize), _depth + 1));

            //[+x, +y, +z] // TOPUPPERRIGHT
            centerOffset.x *= -1.0f;
            _children.Add(new ONode(_tree, this, new Bounds(_bounds.center + centerOffset, newBoundsSize), _depth + 1));

            //[-x, -y, +z] // TOPLOWERRIGHT
            centerOffset.y *= -1.0f;
            _children.Add(new ONode(_tree, this, new Bounds(_bounds.center + centerOffset, newBoundsSize), _depth + 1));

            //[+x, -y, +z] // TOPLOWERLEFT
            centerOffset.x *= -1.0f;
            _children.Add(new ONode(_tree, this, new Bounds(_bounds.center + centerOffset, newBoundsSize), _depth + 1));

            //[-x, +y, -z] // BOTTOMUPPERLEFT
            centerOffset.z *= -1.0f;
            centerOffset.y *= -1.0f;
            _children.Add(new ONode(_tree, this, new Bounds(_bounds.center + centerOffset, newBoundsSize), _depth + 1));

            //[+x, +y, -z] // BOTTOMUPPERRIGHT
            centerOffset.x *= -1.0f;
            _children.Add(new ONode(_tree, this, new Bounds(_bounds.center + centerOffset, newBoundsSize), _depth + 1));

            //[-x, -y, -z] // BOTTOMLOWERRIGHT
            centerOffset.y *= -1.0f;
            _children.Add(new ONode(_tree, this, new Bounds(_bounds.center + centerOffset, newBoundsSize), _depth + 1));

            //[+x, -y, -z] // BOTTOMLOWERLEFT
            centerOffset.x *= -1.0f;
            _children.Add(new ONode(_tree, this, new Bounds(_bounds.center + centerOffset, newBoundsSize), _depth + 1));
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

    public void CollectAllNodes(Dictionary<int, List<ONode>> allNodes)
    {
        if(!allNodes.ContainsKey(_depth))
        {
            allNodes.Add(_depth, new List<ONode>());
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
                var zExtent = _bounds.extents;
                xExtent.y = 0;
                xExtent.z = 0;
                yExtent.x = 0;
                yExtent.z = 0;
                zExtent.x = 0;
                zExtent.y = 0;
                Gizmos.DrawLine(_bounds.center - xExtent, _bounds.center + xExtent);
                Gizmos.DrawLine(_bounds.center - yExtent, _bounds.center + yExtent);
                Gizmos.DrawLine(_bounds.center - zExtent, _bounds.center + zExtent);
            }
        }
    }

    private bool IsSplitted
    {
        get { return (_children != null) && (_children.Count > 0); }
    }
}
