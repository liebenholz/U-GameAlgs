using JetBrains.Annotations;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum LQDrawMode : int
{
    INSERT, 
    QUERY
};

public class LQuadtreeManager : MonoBehaviour
{
    public LQDrawMode _drawMode = LQDrawMode.INSERT;
    public Vector2 _totalArea = new Vector3(64f, 64f);
    public LQuadtree _tree = null;

    private int _iIndex = 0;
    private GameObject[] _insertObjects;

    private int _qIndex = 0;
    private GameObject[] _queryObjects;
    private List<LQNode> _possibleNodes; 
    private List<BoxCollider> _intersectObjects; 

    private void Awake()
    {
        if (_tree == null)
        {
            _tree = new LQuadtree(_totalArea);
        }
        else
        {
            _tree.Clear();
        }

        _insertObjects = GameObject.FindGameObjectsWithTag("Insert");
        _queryObjects = GameObject.FindGameObjectsWithTag("Query");

        if(_drawMode == LQDrawMode.QUERY)
        {
            InsertAll();
        }
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void InsertAll()
    {
        for (int ix = 0; ix < _insertObjects.Length; ++ix)
        {
            Insert();
        }
    }

    public void Insert()
    {
        if (_iIndex >= _insertObjects.Length)
        {
            return;
        }

        _tree.Insert(_insertObjects[_iIndex++].GetComponent<BoxCollider>());
    }

    public void Query()
    {
        if (_qIndex >= _queryObjects.Length)
        {
            return;
        }

        _possibleNodes = new List<LQNode>();
        _intersectObjects = _tree.Query(_queryObjects[_qIndex++].GetComponent<BoxCollider>(), _possibleNodes);
    }

    public void Stat()
    {
        Dictionary<int, List<LQNode>> allNodes = _tree.GetAllNodes();

        string debugString = string.Format("Total Nodes: {0} \n", allNodes.Count);
        foreach (int depth in allNodes.Keys)
        {
            int nodeCount = allNodes[depth].Count;
            int itemCount = 0;
            foreach(var node in allNodes[depth])
            {
                itemCount += node._items.Count;
            }

            debugString = string.Concat(debugString, string.Format("Depth: {0}, Nodes: {1}, Objects: {2} \n", depth, nodeCount, itemCount));
        }

        Debug.Log(debugString);
    }

    [ExecuteInEditMode]
    protected void OnDrawGizmos()
    {
        Gizmos.color = Color.gray;

        // 쿼드트리의 모든 노드 영역 그리기 ( 회색 )
        Gizmos.DrawWireCube(Vector3.zero, _totalArea);
        if (_tree != null)
        {
            _tree.DrawBounds();
        }

        Bounds testBounds;
        Gizmos.color = Color.cyan;

        // 삽입한 영역 그리기 ( 하늘색 )
        for (int ix = 0; ix < _iIndex; ++ix)
        {
            testBounds = _insertObjects[ix].GetComponent<BoxCollider>().bounds;
            Gizmos.DrawCube(testBounds.center, testBounds.size);
        }

        if (_drawMode == LQDrawMode.INSERT)
        {
            // 삽입할 영역 그리기 ( 빨간색 )
            if (HasInsertCandidate)
            {
                Gizmos.color = Color.red;
                testBounds = _insertObjects[_iIndex].GetComponent<BoxCollider>().bounds;
                Gizmos.DrawCube(testBounds.center, testBounds.size);
            }
        }
        else if(_drawMode == LQDrawMode.QUERY)
        {
            // 질의할 영역 그리기 ( 빨간색 )
            if (HasQueryCandidate)
            {
                Gizmos.color = Color.red;
                testBounds = _queryObjects[_qIndex].GetComponent<BoxCollider>().bounds;
                Gizmos.DrawWireCube(testBounds.center, testBounds.size);
            }

            // 이전에 질의했던 영역 그리기 ( 노란색 )
            if (_qIndex > 0)
            {
                Gizmos.color = Color.yellow;
                testBounds = _queryObjects[_qIndex - 1].GetComponent<BoxCollider>().bounds;
                Gizmos.DrawWireCube(testBounds.center, testBounds.size);
            }

            // 질의한 영역과 겹치는 노드 영역 그리기 ( 보라색 )
            Gizmos.color = Color.magenta;
            if (_possibleNodes != null)
            {
                foreach (var node in _possibleNodes)
                {
                    Gizmos.DrawWireCube(node._bounds.center, node._bounds.size);
                }
            }

            // 질의한 영역과 겹침 판정을 받은 영역 그리기 ( 보라색 )
            if (_intersectObjects != null)
            {
                foreach (var col in _intersectObjects)
                {
                    Gizmos.DrawCube(col.bounds.center, col.bounds.size);
                }
            }
        }
    }

    public bool HasInsertCandidate
    {
        get { return _insertObjects != null && _iIndex < _insertObjects.Length; }
    }

    public bool HasQueryCandidate
    {
        get { return _queryObjects != null && _qIndex < _queryObjects.Length; }
    }

}
