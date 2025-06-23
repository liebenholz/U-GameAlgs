using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

public class LQuadtree
{
    public LQNode _rootNode;
    public int _maxDepth = 5;
    public float _constantK = 2.0f;

    /// <summary>
    /// ��Ʈ ��� ����
    /// </summary>
    /// <param name="center">�ٿ������ �߽�</param>
    /// <param name="size">�ٿ������ ũ��</param>
    public LQuadtree(Vector2 size)
    {
        _rootNode = new LQNode(this, null, new Bounds(Vector2.zero, size), 0);
    }

    private int GetTargetDepth(BoxCollider item)
    {
        // ���� ���� ��ü�� Depth�� �̸� ����
        float width = _rootNode._bounds.size.x;
        float maxHalfValue = Mathf.Max(item.bounds.extents.x, item.bounds.extents.y);
        int targetDepth = Mathf.FloorToInt(Mathf.Log(width / maxHalfValue, 2.0f));
        if (targetDepth > _maxDepth)
        {
            targetDepth = _maxDepth;
        }

        return targetDepth;
    }

    public void Insert(BoxCollider insertItem)
    {
        _rootNode.InsertAtDepth(insertItem, GetTargetDepth(insertItem));
    }

    public List<BoxCollider> Query(BoxCollider queryItem, List<LQNode> possibleNodes)
    {
        _rootNode.Query(queryItem, possibleNodes);

        List<BoxCollider> intersects = new List<BoxCollider>();
        foreach (var node in possibleNodes)
        {
            foreach (var item in node._items)
            {
                if (item.bounds.Intersects(queryItem.bounds))
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

    public Dictionary<int, List<LQNode>> GetAllNodes()
    {
        Dictionary<int, List<LQNode>> allNodes = new Dictionary<int, List<LQNode>>();
        _rootNode.CollectAllNodes(allNodes);
        return allNodes;
    }

    public void DrawBounds()
    {
        _rootNode.DrawBounds();
    }
}
