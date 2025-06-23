using System.Collections.Generic;
using UnityEngine;

public class Quadtree
{
    public QNode _rootNode;
    public int _maxDepth = 5;

    /// <summary>
    /// 루트 노드 생성
    /// </summary>
    /// <param name="center">바운딩볼륨 중심</param>
    /// <param name="size">바운딩볼륨 크기</param>
    public Quadtree(Vector2 size)
    {
        _rootNode = new QNode(this, null, new Bounds(Vector2.zero, size), 0);
    }

    public void Insert(BoxCollider insertItem)
    {
        _rootNode.Insert(insertItem);
    }

    public List<BoxCollider> Query(BoxCollider queryItem, List<QNode> possibleNodes)
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
    /// 할당된 모든 노드를 제거
    /// </summary>
    public void Clear()
    {
        _rootNode.Clear();
    }

    public Dictionary<int, List<QNode>> GetAllNodes()
    {
        Dictionary<int, List<QNode>> allNodes = new Dictionary<int, List<QNode>>();
        _rootNode.CollectAllNodes(allNodes);
        return allNodes;
    }

    public void DrawBounds()
    {
        _rootNode.DrawBounds();
    }
}
