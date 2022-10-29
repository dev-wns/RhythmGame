using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

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
        LoadScene( SceneType.Lobby );
    }

    protected override void Awake()
    {
        base.Awake();
        NowPlaying NP = NowPlaying.Inst;
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
