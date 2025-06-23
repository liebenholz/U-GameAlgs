using System.Collections.Generic;
using UnityEngine;

public enum KDrawMode : int
{
    INSERT, 
    QUERY
};

public class KdtreeManager : MonoBehaviour
{
    public int _dimension = 2;
    public KDrawMode _drawMode = KDrawMode.INSERT;
    public Bounds _drawArea = new Bounds(Vector3.zero, Vector3.one * 64.0f);
    public Kdtree _tree = null;

    private int _iIndex = 0;
    private GameObject[] _insertObjects;

    private int _qIndex = 0;
    private GameObject[] _queryObjects;
    private GameObject _nearestObject;
    private List<KNode> _searchedNodes; 

    private void Awake()
    {
        if (_tree == null)
        {
            _tree = new Kdtree(_dimension);
        }
        else
        {
            _tree.Clear();
        }

        _insertObjects = GameObject.FindGameObjectsWithTag("Insert");
        _queryObjects = GameObject.FindGameObjectsWithTag("Query");

        if(_drawMode == KDrawMode.QUERY)
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

        _tree.Add(_insertObjects[_iIndex++].GetComponent<Transform>());
    }

    public void NearestQuery()
    {
        if (_tree == null || _tree.Root == null)
        {
            return;
        }

        if (_qIndex >= _queryObjects.Length)
        {
            return;
        }

        _searchedNodes = new List<KNode>();
        _nearestObject = _tree.NearestQuery(_queryObjects[_qIndex++].GetComponent<Transform>(), _searchedNodes).gameObject;
        Debug.Log(string.Format("Total Searched Nodes : {0}", _searchedNodes.Count));
    }

    [ExecuteInEditMode]
    protected void OnDrawGizmos()
    {
        Gizmos.color = Color.gray;

        // KD트리의 모든 노드 영역 그리기 ( 회색 )
        Gizmos.DrawWireCube(_drawArea.center, _drawArea.size);

        if(_tree != null && _tree.Root != null)
        {
            _tree.Root.DrawSplitPlane(_drawArea);
        }

        if (_drawMode == KDrawMode.INSERT)
        {
            // 삽입할 영역 그리기 ( 빨간색 )
            if (HasInsertCandidate)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawCube(_insertObjects[_iIndex].transform.position, Vector3.one);
            }
        }
        else if(_drawMode == KDrawMode.QUERY)
        {
            // 질의할 물체 그리기 ( 빨간색 )
            if (HasQueryCandidate)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawCube(_queryObjects[_qIndex].transform.position, Vector3.one);
            }

            // 질의한 물체 그리기 ( 노란색 )
            if (_qIndex > 0)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawCube(_queryObjects[_qIndex - 1].transform.position, Vector3.one);
            }

            // 질의한 물체와 가장 가까운 물체 그리기 ( 보라색 )
            if (_nearestObject != null)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawCube(_nearestObject.transform.position, Vector3.one);

                // 질의한 물체와 가장 가까운 물체와의 선 그리기 ( 보라색 )
                Gizmos.DrawLine(_queryObjects[_qIndex - 1].transform.position, _nearestObject.transform.position);
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
