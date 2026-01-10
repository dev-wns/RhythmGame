using Cysharp.Threading.Tasks;
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

        //if ( DataStorage.UserInfo is null )
        {
            //EnableLoginCanvas();
        }
        //else
        {
            //playerInfo.UpdateUserInfo( DataStorage.UserInfo.Value );
            //DisableLoginCanvas();
        }
    }

    protected override void Start()
    {
        base.Start();

        // 생성되어있는 스테이지 정보 요청
        if ( Network.Inst.IsConnected )
            Network.Inst.Send( new Packet( STAGE_INFO_REQ ) );
    }

    //public void EnableLoginCanvas() => EnableCanvas( SceneAction.Login, loginCanvas, false );

    //public void DisableLoginCanvas() => DisableCanvas( SceneAction.Main, loginCanvas, false );

    //public void EnableCreateStageCanvas() => EnableCanvas( SceneAction.CreateStage, createStageCanvas );

    //public void DisableCreateStageCanvas() => DisableCanvas( SceneAction.Main, createStageCanvas );

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

    protected override void Connect() { }
    protected override void Disconnect() { }
    //public override void KeyBind()
    //{
    //    // Login
    //    Bind( SceneAction.Login, KeyCode.Escape, DisableLoginCanvas );

    //    // Create Room
    //    Bind( SceneAction.CreateStage, KeyCode.Escape, DisableCreateStageCanvas );
    //}
}
