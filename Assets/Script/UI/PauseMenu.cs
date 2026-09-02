using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;

    private void Start()
    {
        GameManager.Instance.OnBackPressed += HandleBackPressed;
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnBackPressed -= HandleBackPressed;
    }

    private void HandleBackPressed()
    {
        GameManager.Instance.Pause();
        panelRoot.SetActive(true);
    }

    public void HandleCloseClicked()
    {
        panelRoot.SetActive(false);
        GameManager.Instance.Resume();
    }

    public void HandleGoToMainClicked()
    {
        panelRoot.SetActive(false);
        GameManager.Instance.AbandonRun();
    }
}
