using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InitScene : Scene
{
    public List<Canvas> canvasList;
    //private readonly float CanvasWaitTime = 1f;
    //private int canvasIndex;

    public Image fadeImage;

    private IEnumerator Process()
    {
        fadeImage.enabled = true;
        fadeImage.color = Color.black;

        yield return YieldCache.WaitForSeconds( 1f );

        fadeImage.DOFade( 0f, .7f );

        yield return YieldCache.WaitForSeconds( 3f );

        fadeImage.DOFade( 1f, .7f );

        //yield return YieldCache.WaitForSeconds( CanvasWaitTime );

        //while ( true )
        //{
        //    canvasList[canvasIndex].gameObject.SetActive( true );

        //    yield return YieldCache.WaitForSeconds( CanvasWaitTime );

        //    fadeImage.DOFade( 1f, .7f );

        //    yield return YieldCache.WaitForSeconds( CanvasWaitTime );

        //    canvasList[canvasIndex].gameObject.SetActive( false );
        //    fadeImage.DOFade( 0f, .7f );

        //    if ( canvasIndex + 1 < canvasList.Count ) ++canvasIndex;
        //    else                                      break;
        //}

        fadeImage.enabled = false;
        LoadScene( SceneType.FreeStyle );
    }

    protected override void Awake()
    {
        base.Awake();
        var replace = SystemSetting.CurrentResolution.ToString().Replace( "_", " " );
        var split = replace.Trim().Split( ' ' );

        var width  = int.Parse( split[0] );
        var height = int.Parse( split[1] );
        switch ( SystemSetting.CurrentScreenMode )
        {
            case ScreenMode.Exclusive_FullScreen:
            Screen.SetResolution( width, height, FullScreenMode.ExclusiveFullScreen );
            break;

            case ScreenMode.FullScreen_Window:
            Screen.SetResolution( width, height, FullScreenMode.FullScreenWindow );
            break;

            case ScreenMode.Windowed:
            Screen.SetResolution( width, height, FullScreenMode.Windowed );
            break;
        }
    }

    protected override void Start()
    {
        base.Start();
        QualitySettings.vSyncCount = 0;

        StartCoroutine( Process() );
    }

    public override void KeyBind()
    {
        Bind( ActionType.Main, KeyCode.Return, () => { } );
    }

    public override void Connect() { }

    public override void Disconnect() { }
}
