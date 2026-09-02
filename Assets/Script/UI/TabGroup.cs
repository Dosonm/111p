using System;
using UnityEngine;
using UnityEngine.UI;

public class TabGroup : MonoBehaviour
{
    [Serializable]
    private class Tab
    {
        public GameObject content;

        public Image buttonImage;
    }

    [SerializeField] private Tab[] tabs;

    [SerializeField] private Color selectedColor = Color.white;

    [SerializeField] private Color unselectedColor = new(0.7f, 0.7f, 0.7f, 1f);

    [Min(0)]
    [SerializeField] private int initialTabIndex;

    private void Start()
    {
        Select(Mathf.Clamp(initialTabIndex, 0, tabs.Length - 1));
    }

    public void Select(int index)
    {
        for (int i = 0; i < tabs.Length; i++)
        {
            bool isSelected = i == index;
            tabs[i].content.SetActive(isSelected);
            tabs[i].buttonImage.color = isSelected ? selectedColor : unselectedColor;
        }
    }
}
