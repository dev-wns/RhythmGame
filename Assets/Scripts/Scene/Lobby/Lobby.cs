using UnityEngine;

using static PacketType;
public class Lobby : Scene
{
    public GameObject loginCanvas;
    public GameObject createStageCanvas;
    public PlayerInfo playerInfo;

    protected override void Awake()
    {
        base.Awake();

        if ( !Network.Inst.IsConnected )
            Network.Inst.Connect( "127.0.0.1" );

        if ( DataStorage.UserInfo is null )
        {
            EnableLoginCanvas();
        }
        else
        {
            playerInfo.UpdateUserInfo( DataStorage.UserInfo.Value );
            DisableLoginCanvas();
        }
    }

    protected override void Start()
    {
        base.Start();

        // 생성되어있는 스테이지 정보 요청
        if ( Network.Inst.IsConnected )
            Network.Inst.Send( new Packet( STAGE_INFO_REQ ) );
    }

    public void EnableLoginCanvas() => EnableCanvas( ActionType.Login, loginCanvas, false );

    public void DisableLoginCanvas() => DisableCanvas( ActionType.Main, loginCanvas, false );

    public void EnableCreateStageCanvas() => EnableCanvas( ActionType.CreateStage, createStageCanvas );

    public void DisableCreateStageCanvas() => DisableCanvas( ActionType.Main, createStageCanvas );

    public void MoveToStage()
    {
        AudioManager.Inst.Play( SFX.MainClick );
        //LoadScene( SceneType.Stage );
    }

    public void MoveToFreeStyle()
    {
        AudioManager.Inst.Play( SFX.MainClick );
        //LoadScene( SceneType.FreeStyle );
    }

    public override void Connect() { }
    public override void Disconnect() { }

    public override void KeyBind()
    {
        // Login
        Bind( ActionType.Login, KeyCode.Escape, DisableLoginCanvas );

        // Create Room
        Bind( ActionType.CreateStage, KeyCode.Escape, DisableCreateStageCanvas );
    }
}
