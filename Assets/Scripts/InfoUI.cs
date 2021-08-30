using UnityEngine;
using TMPro;

public class InfoUI : MonoBehaviour
{
    public TMP_Text Name;
    public TMP_Text Desc;

    private Canvas _canvas;
    private void Awake()
    {
        _canvas = GetComponent<Canvas>();
        _canvas.worldCamera = GameObject.Find("AR Camera").GetComponent<Camera>();

        Debug.Assert(_canvas.worldCamera != null);
    }
}
