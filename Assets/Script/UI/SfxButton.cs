using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SfxButton : Button
{
    [SerializeField] private bool playClickSfx = true;

    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);

        if (!playClickSfx || !IsActive() || !IsInteractable())
        {
            return;
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySfx(SfxId.UiClick);
        }
    }
}
