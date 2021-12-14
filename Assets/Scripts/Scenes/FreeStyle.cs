using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class FreeStyle : Scene
{
    public VerticalScrollSound scrollSound;
    
    public static ObjectPool<FadeBackground> bgPool;
    public FadeBackground bgPrefab, curBackground;
    private Sprite background;

    private bool IsBGLoadDone = false;

    public delegate void DelSelectSound( Song _song );
    public event DelSelectSound OnSelectSound;

    #region unity callbacks
    protected override void Awake()
    {
        base.Awake();

        bgPool = new ObjectPool<FadeBackground>( bgPrefab, 5 );

        StartCoroutine( FadeBackground() );
    }

    private void Update()
    {
        if ( !SoundManager.Inst.IsPlaying() )
        {
            SoundManager.Inst.Play();

            // 중간부터 재생
            int time = GlobalSoundInfo.CurrentSound.previewTime;
            if ( time <= 0 ) SoundManager.Inst.SetPosition( ( uint )( SoundManager.Inst.Length / 3f ) );
            else             SoundManager.Inst.SetPosition( ( uint )time );
        }

        if ( Input.GetKeyDown( KeyCode.UpArrow ) ) 
        {
            scrollSound.PrevMove();
            ChangePreview();
        }

        if ( Input.GetKeyDown( KeyCode.DownArrow ) ) 
        {
            scrollSound.NextMove();
            ChangePreview();
        }

        if ( Input.GetKeyDown( KeyCode.Return ) )
        {
            //NowPlaying.Inst.Initialized( GameManager.Datas[Index] );
            Change( SceneType.InGame );
        }

        if ( Input.GetKeyDown( KeyCode.A ) )
            SoundManager.Inst.UseLowEqualizer( true );

        if ( Input.GetKeyDown( KeyCode.S ) )
            SoundManager.Inst.UseLowEqualizer( false );

        if ( Input.GetKeyDown( KeyCode.LeftArrow ) )
            SoundManager.Inst.SetPitch( SoundManager.Inst.Pitch - .1f );

        if ( Input.GetKeyDown( KeyCode.RightArrow ) )
            SoundManager.Inst.SetPitch( SoundManager.Inst.Pitch + .1f );
    }

    private void ChangePreview()
    {
        if ( scrollSound.IsDuplicate ) return;
        
        Song curSong = GlobalSoundInfo.CurrentSound;
        StartCoroutine( FadeBackground() );

        Globals.Timer.Start();
        {
            SoundManager.Inst.Load( curSong.audioPath, Sound.LoadType.Stream );
            SoundManager.Inst.Play();
        }
        OnSelectSound( curSong );

        Debug.Log( $"Sound Load {Globals.Timer.End()} ms" );

        // 중간부터 재생
        int time = curSong.previewTime;
        if ( time <= 0 ) SoundManager.Inst.SetPosition( ( uint )( SoundManager.Inst.Length / 3f ) );
        else             SoundManager.Inst.SetPosition( ( uint )time );
    }

    protected IEnumerator LoadBackground( string _path )
    {
        Texture2D tex = new Texture2D( 1, 1, TextureFormat.ARGB32, false );
        byte[] binaryData = File.ReadAllBytes( _path );

        while ( !tex.LoadImage( binaryData ) ) yield return null;
        background = Sprite.Create( tex, new Rect( 0, 0, tex.width, tex.height ), new Vector2( .5f, .5f ), GlobalSetting.PPU, 0, SpriteMeshType.FullRect );

        IsBGLoadDone = true;
    }

    public IEnumerator FadeBackground()
    {
        Globals.Timer.Start();
        {
            StartCoroutine( LoadBackground( GlobalSoundInfo.CurrentSound.imagePath ) );
            yield return new WaitUntil( () => IsBGLoadDone );
            if ( curBackground != null )
                curBackground.Despawn();

            curBackground = bgPool.Spawn();
            curBackground.image.sprite = background;

            IsBGLoadDone = false;
        }
        Debug.Log( $"BackgroundLoad {Globals.Timer.elapsedMilliSeconds} ms" );
    }
    #endregion
}