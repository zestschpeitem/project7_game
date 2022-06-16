using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class Tooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TextMeshProUGUI view;
    public string test;

    public void OnPointerEnter(PointerEventData eventData)
    {
        view.text = test;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        view.text = "";
    }
}


