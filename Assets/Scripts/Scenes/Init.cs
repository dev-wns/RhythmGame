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

    protected override void Awake()
    {
        base.Awake();
        // setting
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 144;
        Screen.SetResolution( 1920, 1080, true );
        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;

        text.color = new Color( 255, 255, 255, 0 );
    }

    private void Start()
    {
        StartCoroutine( ShowFadeText() );
    }

    private IEnumerator ShowFadeText()
    {
        yield return YieldCache.WaitForSeconds( 1f );

        text.DOFade( 1f, 5f );
        yield return YieldCache.WaitForSeconds( 2f );
        Change( SceneType.Lobby );
    }
}
