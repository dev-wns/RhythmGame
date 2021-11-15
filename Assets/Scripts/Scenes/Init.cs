using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;

public class Init : Scene
{
    public TextMeshProUGUI text;
    public Slider slider;

    private float progressValue = 0f;

    protected override void Awake()
    {
        base.Awake();

        GameManager.loadProgress = ProgressChange;
        text.color = new Color( 255, 255, 255, 0 );
    }

    private void Start()
    {
        text.DOFade( 1f, 5f );
    }

    private void Update()
    {
        if ( GameManager.isDone )
        {
            Change( SceneType.Lobby );
        }
    }

    public void ProgressChange( float _offset )
    {
        progressValue += _offset;
        slider.DOValue( progressValue, .5f );
    }
}
