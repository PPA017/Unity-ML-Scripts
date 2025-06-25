using UnityEngine;
using System.Collections.Generic;

public class RacingLineGyzmo : MonoBehaviour
{
    public int maxPoints = 1000;
    public float pointSpacing = 0.5f;

    private List<Vector3> recordedPositions = new List<Vector3>();
    private Vector3 lastRecordedPosition;

    private void Start()
    {
        lastRecordedPosition = transform.position;
    }

    private void Update()
    {
        float distance = Vector3.Distance(transform.position, lastRecordedPosition);
        if (distance >= pointSpacing)
        {
            recordedPositions.Add(transform.position);
            lastRecordedPosition = transform.position;

            if (recordedPositions.Count > maxPoints)
                recordedPositions.RemoveAt(0);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        float yOffset = 0.3f; 

        for (int i = 1; i < recordedPositions.Count; i++)
        {
            Vector3 prev = recordedPositions[i - 1];
            Vector3 curr = recordedPositions[i];

            prev.y += yOffset;
            curr.y += yOffset;

            Gizmos.DrawLine(prev, curr);
        }
    }

}
