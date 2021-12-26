using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public abstract class OptionButton : OptionBase
{
    public GameObject objectGroup;
    public TextMeshProUGUI currentValueText;
    public GameObject outline;


    protected override void Awake()
    {
        base.Awake();

        if ( contents.Length > 0 )
        {
            currentValueText.text = curOption.name;
        }
    }

    protected override void PrevMove()
    {
        base.PrevMove();
        OutlineSetting();
    }

    protected override void NextMove()
    {
        base.NextMove(); 
        OutlineSetting();
    }

    private void OutlineSetting()
    {
        outline.transform.SetParent( curOption.transform );
        RectTransform rt = outline.transform as RectTransform;
        rt.anchoredPosition = Vector2.zero;
        rt.pivot = new Vector2( .5f, .5f );
    }
}
