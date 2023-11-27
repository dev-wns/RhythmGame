using DG.Tweening;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

public class InitScene : Scene
{
    protected override void Awake()
    {
        base.Awake();

        QualitySettings.maxQueuedFrames = 0;
        Cursor.visible = false;
        DOTween.Init( true, false, LogBehaviour.Default ).SetCapacity( 50, 20 );

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

        QualitySettings.vSyncCount = SystemSetting.CurrentFrameRate == FrameRate.vSync ? 1 : 0;
        switch ( SystemSetting.CurrentFrameRate )
        {
            case FrameRate.vSync:
            case FrameRate.No_Limit:
            Application.targetFrameRate = 0;
            break;

            case FrameRate._60:
            case FrameRate._144:
            case FrameRate._240:
            case FrameRate._960:
            {
                QualitySettings.vSyncCount = 0;
                var frame = ( SystemSetting.CurrentFrameRate ).ToString().Replace( "_", " " );
                Application.targetFrameRate = int.Parse( frame );
            } break;
        }
    }

    protected async override void Start()
    {
        StartCoroutine( ParsingAfterSwitchScene() );
        await Task.Run( NowPlaying.Inst.Load );
    }

    private IEnumerator ParsingAfterSwitchScene()
    {
        yield return StartCoroutine( FadeIn() );

        yield return new WaitUntil( () => !NowPlaying.IsParsing );

        yield return YieldCache.WaitForSeconds( 3f );

        LoadScene( SceneType.FreeStyle );
    }

    public override void KeyBind()
    {
        Bind( ActionType.Main, KeyCode.Return, () => { } );
    }

    public override void Connect() { }

    public override void Disconnect() { }
}
