using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class LobbyMainScroll : ScrollOption, IKeyBind
{
    private Scene scene;
    public GameObject optionCanvas, exitCanvas;

    private RectTransform rt;

    protected override void Awake()
    {
        base.Awake();

        scene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<Scene>();
        rt = transform as RectTransform;
        
        // ScrollOption
        IsLoop = true;
        var childRT = CurrentOption.transform as RectTransform;
        rt.DOAnchorPosX( -childRT.anchoredPosition.x, .25f );

        // Key
        KeyBind();
    }
    public override void PrevMove()
    {
        base.PrevMove();
        var childRT = CurrentOption.transform as RectTransform;
        rt.DOAnchorPosX( -childRT.anchoredPosition.x, .25f );

        Debug.Log( $"{PreviousIndex} -> {CurrentIndex} {CurrentOption.name}" );
    }

    public override void NextMove()
    {
        base.NextMove();
        var childRT = CurrentOption.transform as RectTransform;
        rt.DOAnchorPosX( -childRT.anchoredPosition.x, .25f );

        Debug.Log( $"{PreviousIndex} -> {CurrentIndex} {CurrentOption.name}" );
    }

    public void GotoFreeStyle()
    {
        if ( NowPlaying.Inst.IsParseSongs )
        {
            scene.LoadScene( SceneType.FreeStyle );
            SoundManager.Inst.Play( SoundSfxType.MainClick );
        }
    }

    public void ShowOptionCanvas()
    {
        optionCanvas.SetActive( true );
        scene.ChangeAction( SceneAction.Option );
        SoundManager.Inst.Play( SoundSfxType.MenuClick );
    }

    public void ShowExitCanvas()
    {
        exitCanvas.SetActive( true );
        scene.ChangeAction( SceneAction.Exit );
        SoundManager.Inst.Play( SoundSfxType.MenuClick );
    }

    public void KeyBind()
    {
        scene.Bind( SceneAction.Main, KeyCode.Return, () => CurrentOption.Process() );

        scene.Bind( SceneAction.Main, KeyCode.LeftArrow, () => SoundManager.Inst.Play( SoundSfxType.MainSelect ) );
        scene.Bind( SceneAction.Main, KeyCode.LeftArrow, () => PrevMove() );

        scene.Bind( SceneAction.Main, KeyCode.RightArrow, () => SoundManager.Inst.Play( SoundSfxType.MainSelect ) );
        scene.Bind( SceneAction.Main, KeyCode.RightArrow, () => NextMove() );
    }
}
