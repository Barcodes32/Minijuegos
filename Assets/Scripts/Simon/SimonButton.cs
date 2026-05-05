using UnityEngine;
using UnityEngine.UI;

public class SimonButton : MonoBehaviour
{
    public int buttonIndex; // 0-3
    public Color normalColor;
    public Color highlightColor;
    public Image buttonImage;
    public Button button;

    private SimonController _controller;

    void Start()
    {
        _controller = FindFirstObjectByType<SimonController>();
        button.onClick.AddListener(OnClick);
        buttonImage.color = normalColor;
    }

    void OnClick()
    {
        _controller.OnButtonPressed(buttonIndex);
    }

    public void Highlight()
    {
        buttonImage.color = highlightColor;
    }

    public void ResetColor()
    {
        buttonImage.color = normalColor;
    }
}