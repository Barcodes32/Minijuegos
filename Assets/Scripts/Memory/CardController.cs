using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardController : MonoBehaviour
{
    [Header("Refs")]
    public GameObject cardBack;
    public GameObject cardFront;
    public Image frontBg;
    public Image frontImage;   // imagen del sprite

    [HideInInspector] public int cardId = -1;
    [HideInInspector] public bool isFlipped = false;
    [HideInInspector] public bool isMatched = false;

    private Button _btn;
    private bool _animating = false;

    void Awake()
    {
        _btn = GetComponent<Button>();
        _btn.onClick.AddListener(OnClick);
    }

    public void Setup(int id, Color color, Sprite sprite)
    {
        cardId = id;
        frontBg.color = color;
        frontImage.sprite = sprite;
        cardBack.SetActive(true);
        cardFront.SetActive(false);
    }

    void OnClick()
    {
        if (_animating || isFlipped || isMatched) return;
        MemoryController.Instance.OnCardSelected(this);
    }

    public void FlipToFront(Action onDone = null) => StartCoroutine(Flip(true, onDone));
    public void FlipToBack(Action onDone = null) => StartCoroutine(Flip(false, onDone));

    IEnumerator Flip(bool toFront, Action onDone)
    {
        _animating = true;
        float t = 0f, dur = 0.1f;
        Vector3 original = transform.localScale;
        Vector3 flat = new Vector3(0f, original.y, original.z);

        while (t < dur)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(original, flat, t / dur);
            yield return null;
        }
        transform.localScale = flat;

        cardBack.SetActive(!toFront);
        cardFront.SetActive(toFront);

        t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(flat, original, t / dur);
            yield return null;
        }
        transform.localScale = original;

        isFlipped = toFront;
        _animating = false;
        onDone?.Invoke();
    }

    public void SetInteractable(bool v) => _btn.interactable = v;
}