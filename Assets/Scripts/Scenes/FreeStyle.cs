using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeStyle : Scene
{
    public VerticalScrollSound scrollSound;
    
    public static ObjectPool<FadeBackground> bgPool;
    public FadeBackground bgPrefab, curBackground;
    private Sprite background;

    private bool IsBGLoadDone = false;

    public delegate void DelSelectSound( Song _song );
    public event DelSelectSound OnSelectSound;

    private float playback;
    private float soundEndPosition;

    #region unity callbacks
    protected override void Awake()
    {
        base.Awake();

        bgPool = new ObjectPool<FadeBackground>( bgPrefab, 5 );
        //StartCoroutine( FadeBackground() );
    }

    public void Start()
    {
        ChangePreview();
    }

    protected override void Update()
    {
        base.Update();
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



    protected override void KeyBind()
    {
        StaticSceneKeyAction scene = new StaticSceneKeyAction();
        scene.Bind( KeyCode.UpArrow, KeyType.Down, () => scrollSound.PrevMove() );
        scene.Bind( KeyCode.UpArrow, KeyType.Down, () => ChangePreview() );

        scene.Bind( KeyCode.DownArrow, KeyType.Down, () => scrollSound.NextMove() );
        scene.Bind( KeyCode.DownArrow, KeyType.Down, () => ChangePreview() );

        scene.Bind( KeyCode.Return, KeyType.Down, () => ChangeScene( SceneType.InGame ) );

        scene.Bind( KeyCode.Space, KeyType.Down, () => SoundManager.Inst.UseLowEqualizer( true ) );
        scene.Bind( KeyCode.Space, KeyType.Down, () => keyAction.ChangeAction( SceneAction.FreeStyleSetting ) );

        scene.Bind( KeyCode.LeftArrow, KeyType.Down,  () => SoundManager.Inst.SetPitch( SoundManager.Inst.Pitch - .1f ) );
        scene.Bind( KeyCode.RightArrow, KeyType.Down, () => SoundManager.Inst.SetPitch( SoundManager.Inst.Pitch + .1f ) );

        scene.Bind( KeyCode.Escape, KeyType.Down, () => ChangeScene( SceneType.Lobby ) );
        keyAction.Bind( SceneAction.FreeStyle, scene );

        StaticSceneKeyAction setting = new StaticSceneKeyAction();
        setting.Bind( KeyCode.Space, KeyType.Down, () => SoundManager.Inst.UseLowEqualizer( false ) );
        setting.Bind( KeyCode.Space, KeyType.Down, () => keyAction.ChangeAction( SceneAction.FreeStyle ) );
        keyAction.Bind( SceneAction.FreeStyleSetting, setting );

        keyAction.ChangeAction( SceneAction.FreeStyle );
    }
}