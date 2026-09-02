using Unity.Cinemachine;
using UnityEngine;

public class GameRoot : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;

    [SerializeField] private CinemachineCamera virtualCamera;

    [SerializeField] private ChunkStreamer chunkStreamer;

    [SerializeField] private ScrollController scrollController;

    [SerializeField] private Player player;

    [SerializeField] private float cellSize = 0.625f;

    private MonsterRegistry registry;

    private void Awake()
    {
        registry = new MonsterRegistry();

        float orthoSize = GridMath.OrthoSizeForColumns(GridMath.Columns, cellSize, targetCamera.aspect);
        LensSettings lens = virtualCamera.Lens;
        lens.OrthographicSize = orthoSize;
        virtualCamera.Lens = lens;

        chunkStreamer.Init(registry, cellSize);
        scrollController.Init(cellSize);
        player.Init(registry, scrollController, cellSize);

        chunkStreamer.Warmup(scrollController.CurrentPlayerRow);
    }

    private void Start()
    {
        GameManager.Instance.OnPlayStarted += HandleRunRestarted;
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnPlayStarted -= HandleRunRestarted;
    }

    private void HandleRunRestarted()
    {
        scrollController.ResetRun();

        registry.Clear();
        chunkStreamer.ResetRun(scrollController.CurrentPlayerRow);

        player.ResetRun();
    }
}
