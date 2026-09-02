using System;
using UnityEngine;

public class ScrollController : MonoBehaviour
{
    [SerializeField] private Transform mapRoot;

    [SerializeField] private int fixedRowOffset = -1;

    [SerializeField] private float slideDuration = 0.12f;

    [SerializeField] private ChunkStreamer chunkStreamer;

    private float cellSize;
    private float initialMapRootY;
    private float targetMapRootY;
    private int rowsAdvanced;

    public int CurrentPlayerRow => rowsAdvanced + fixedRowOffset;

    public int RowsAdvanced => rowsAdvanced;

    public event Action<int> OnDepthChanged;

    public void Init(float appliedCellSize)
    {
        cellSize = appliedCellSize;
        initialMapRootY = mapRoot.localPosition.y;
        targetMapRootY = initialMapRootY;
    }

    public void ResetRun()
    {
        rowsAdvanced = 0;
        targetMapRootY = initialMapRootY;

        Vector3 pos = mapRoot.localPosition;
        mapRoot.localPosition = new Vector3(pos.x, initialMapRootY, pos.z);

        OnDepthChanged?.Invoke(rowsAdvanced);
    }

    public void StepDown()
    {
        rowsAdvanced++;
        targetMapRootY = initialMapRootY + rowsAdvanced * cellSize;
        chunkStreamer.Tick(CurrentPlayerRow);
        OnDepthChanged?.Invoke(rowsAdvanced);
    }

    private void Update()
    {
        Vector3 pos = mapRoot.localPosition;
        if (Mathf.Approximately(pos.y, targetMapRootY))
        {
            return;
        }

        float speed = cellSize / slideDuration;
        float newY = Mathf.MoveTowards(pos.y, targetMapRootY, speed * Time.deltaTime);
        mapRoot.localPosition = new Vector3(pos.x, newY, pos.z);
    }
}
