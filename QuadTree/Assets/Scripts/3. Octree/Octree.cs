using System.Collections.Generic;
using UnityEngine;

public class Octree
{
    public ONode _rootNode;
    public int _maxDepth = 5;

    /// <summary>
    /// ��Ʈ ��� ����
    /// </summary>
    /// <param name="center">�ٿ������ �߽�</param>
    /// <param name="size">�ٿ������ ũ��</param>
    public Octree(Vector3 size)
    {
        _rootNode = new ONode(this, null, new Bounds(Vector3.zero, size), 0);
    }

    public void Insert(BoxCollider insertItem)
    {
        _rootNode.Insert(insertItem);
    }

    public List<BoxCollider> Query(BoxCollider queryItem, List<ONode> possibleNodes)
    {
        _rootNode.Query(queryItem, possibleNodes);

        List<BoxCollider> intersects = new List<BoxCollider>();
        foreach (var node in possibleNodes)
        {
            foreach(var item in node._items)
            {
                if(item.bounds.Intersects(queryItem.bounds))
                {
                    intersects.Add(item);
                }
            }
        }

        return intersects;
    }

    /// <summary>
    /// �Ҵ�� ��� ��带 ����
    /// </summary>
    public void Clear()
    {
        _rootNode.Clear();
    }

    public Dictionary<int, List<ONode>> GetAllNodes()
    {
        Dictionary<int, List<ONode>> allNodes = new Dictionary<int, List<ONode>>();
        _rootNode.CollectAllNodes(allNodes);
        return allNodes;
    }

    public void DrawBounds()
    {
        _rootNode.DrawBounds();
    }
}
