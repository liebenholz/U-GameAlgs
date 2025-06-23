using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PriorityQueueTest : MonoBehaviour
{
    public List<int> priorityQueueData;

    // Start is called before the first frame update
    void Start()
    {
        PriorityQueue<int> pq = new PriorityQueue<int>();
        pq.Enqueue(23);
        pq.Enqueue(10);
        pq.Enqueue(8);
        pq.Enqueue(9);
        pq.Enqueue(3);
        pq.Enqueue(5);

        pq.Dequeue();
        priorityQueueData = pq.data;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
