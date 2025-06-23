using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;

public class KNode
{
    public KNode(int dimension, KNode parent, Transform transform, int depth)
    {
        _dimension = dimension;
        _depth = depth;
        _transform = transform;
        _parent = parent;
    }

    private Transform _transform;
    private int _depth = 0;
    private int _dimension = 2;
    private KNode _parent;
    private KNode _left = null;
    private KNode _right = null;

    public void DrawSplitPlane(Bounds drawArea)
    {
        Bounds[] splittedBounds = new Bounds[2];

        Vector3 planeCenter = drawArea.center;
        planeCenter[_depth % _dimension] = KdValue;
        Vector3 planeSize = drawArea.size;
        planeSize[_depth % _dimension] = 0.0f;

        // 왼쪽 노드의 AABB 영역
        Vector3 leftMin = drawArea.min;
        Vector3 leftMax = drawArea.max;
        leftMax[_depth % _dimension] = KdValue;
        splittedBounds[0].min = leftMin;
        splittedBounds[0].max = leftMax;

        // 오른쪽 노드의 AABB 영역
        Vector3 rightMin = drawArea.min;
        rightMin[_depth % _dimension] = KdValue;
        Vector3 rightMax = drawArea.max;
        splittedBounds[1].min = rightMin;
        splittedBounds[1].max = rightMax;

        Gizmos.color = Color.gray;
        Gizmos.DrawWireCube(planeCenter, planeSize);
        Gizmos.color = Color.white;
        Gizmos.DrawCube(Transform.position, Vector3.one);

        if (!IsLeftOpen)
        {
            _left.DrawSplitPlane(splittedBounds[0]);
        }

        if (!IsRightOpen)
        {
            _right.DrawSplitPlane(splittedBounds[1]);
        }
    }

    public KNode CheckNearestNode(Transform testTransform, KNode nearestNode, List<KNode> searchedNode)
    {
        searchedNode.Add(this); // 방문한 노드 정보

        KNode nearestNodeSoFar = nearestNode;
        float nearestSqrDistance = nearestNodeSoFar.SquaredDistanceTo(testTransform.position);
        float sqrDistanceToNode = SquaredDistanceTo(testTransform.position);
        if (sqrDistanceToNode < nearestSqrDistance)
        {
            nearestSqrDistance = sqrDistanceToNode;

            if (IsLeaf)
            {
                return this;
            }
            else
            {
                nearestNodeSoFar = this;
            }
        }

        float distanceToSplitAxis = KdValue - testTransform.GetKdValue(_dimension, _depth);
        float sqrDistanceToSplitAxis = distanceToSplitAxis * distanceToSplitAxis;

        if (testTransform.GetKdValue(_dimension, _depth) < KdValue)
        {
            if(!IsLeftOpen)
            {
                nearestNodeSoFar = _left.CheckNearestNode(testTransform, nearestNodeSoFar, searchedNode);
                nearestSqrDistance = nearestNodeSoFar.SquaredDistanceTo(testTransform.position);
            }

            if (!IsRightOpen && (sqrDistanceToSplitAxis < nearestSqrDistance))
            {
                nearestNodeSoFar = _right.CheckNearestNode(testTransform, nearestNodeSoFar, searchedNode);
            }
        }
        else
        {
            if (!IsRightOpen)
            {
                nearestNodeSoFar = _right.CheckNearestNode(testTransform, nearestNodeSoFar, searchedNode);
                nearestSqrDistance = nearestNodeSoFar.SquaredDistanceTo(testTransform.position);
            }

            if (!IsLeftOpen && (sqrDistanceToSplitAxis < nearestSqrDistance))
            {
                nearestNodeSoFar = _left.CheckNearestNode(testTransform, nearestNodeSoFar, searchedNode);
            }
        }

        return nearestNodeSoFar;
    }

    public bool IsLeaf
    {
        get { return (_left == null && _right == null) ? true : false; }
    }
    
    public KNode Left
    {
        get { return _left; }
        set { _left = value; }
    }

    public KNode Right
    {
        get { return _right; }
        set { _right = value; }
    }

    public bool IsLeftOpen
    {
        get { return _left == null; }
    }

    public bool IsRightOpen
    {
        get { return _right == null; }
    }

    public Transform Transform
    {
        get { return _transform; }
    }

    public float KdValue
    {
        get { return _transform.GetKdValue(_dimension, _depth); }
    }

    // 계산 성능을 위해 제곱근을 씌우지 않음
    public float SquaredDistanceTo(Vector3 targetPosition)
    {        
        return (targetPosition - Transform.position).sqrMagnitude;
    }

    public float DistanceTo(Vector3 targetPosition)
    {
        return Vector3.Distance(targetPosition, Transform.position);
    }

}
