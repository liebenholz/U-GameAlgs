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

        // KDƮ���� ��� ��� ���� �׸��� ( ȸ�� )
        Gizmos.DrawWireCube(_drawArea.center, _drawArea.size);

        if(_tree != null && _tree.Root != null)
        {
            _tree.Root.DrawSplitPlane(_drawArea);
        }

        if (_drawMode == KDrawMode.INSERT)
        {
            // ������ ���� �׸��� ( ������ )
            if (HasInsertCandidate)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawCube(_insertObjects[_iIndex].transform.position, Vector3.one);
            }
        }
        else if(_drawMode == KDrawMode.QUERY)
        {
            // ������ ��ü �׸��� ( ������ )
            if (HasQueryCandidate)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawCube(_queryObjects[_qIndex].transform.position, Vector3.one);
            }

            // ������ ��ü �׸��� ( ����� )
            if (_qIndex > 0)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawCube(_queryObjects[_qIndex - 1].transform.position, Vector3.one);
            }

            // ������ ��ü�� ���� ����� ��ü �׸��� ( ����� )
            if (_nearestObject != null)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawCube(_nearestObject.transform.position, Vector3.one);

                // ������ ��ü�� ���� ����� ��ü���� �� �׸��� ( ����� )
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
