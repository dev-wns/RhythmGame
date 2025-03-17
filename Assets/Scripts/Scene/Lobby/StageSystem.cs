using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

using static PacketType;
public class StageSystem : MonoBehaviour
{
    [Header( "< Create Stage >" )]
    private Scene scene;
    public TMP_InputField title;
    public TMP_InputField password;

    [Header( "< Stage List >" )]
    public List<StageData> stages = new List<StageData>();
    public ObjectPool<StageData> pool;
    public StageData prefab;
    public Transform contents;

    private void Awake()
    {
        scene = GameObject.FindGameObjectWithTag( "Scene" ).GetComponent<Scene>();
        pool  = new ObjectPool<StageData>( prefab, contents, 5 );

        // Protocols
        ProtocolSystem.Inst.Regist( STAGE_INFO_ACK,    AckInsertStage );
        ProtocolSystem.Inst.Regist( INSERT_STAGE_INFO, AckInsertStage );
        ProtocolSystem.Inst.Regist( CREATE_STAGE_ACK,  AckEntryStage );
        ProtocolSystem.Inst.Regist( ENTRY_STAGE_ACK,   AckEntryStage );
        ProtocolSystem.Inst.Regist( UPDATE_STAGE_INFO, AckUpdateStage );
        ProtocolSystem.Inst.Regist( DELETE_STAGE_INFO, AckDeleteStage );
    }
    public void CreateStage()
    {
        STAGE_INFO protocol;
        protocol.stageSerial = 0;
        protocol.hostSerial = 0;
        protocol.title = title.text;
        protocol.password = password.text;
        protocol.host = string.Empty;
        protocol.song = string.Empty;
        protocol.isPlaying = false;
        protocol.personnel = new Personnel { current = 0, maximum = 7 };

        Network.Inst.Send( new Packet( CREATE_STAGE_REQ, protocol ) );
    }

    private void AckEntryStage( Packet _packet )
    {
        switch ( _packet.error )
        {
            case Error.OK:
            {
                GameManager.StageInfo = Packet.FromJson<STAGE_INFO>( _packet );
                scene.LoadScene( SceneType.Stage );
                AudioManager.Inst.Play( SFX.MainClick );
            }
            break;

            // 스테이지에 인원이 꽉찻거나
            // 게임 종료된 스테이지에 입장할 때( Result 처리 중인 스테이지 )
            case Error.ERR_UNABLE_PROCESS:
            default:
            {
                //scene.ActiveErrorPanel( "방에 입장할 수 없습니다." );
                //SceneBase.IsLock = false;
            }
            break;
        }
    }

    private void AckUpdateStage( Packet _packet )
    {
        var data = Packet.FromJson<STAGE_INFO>( _packet );
        foreach ( var stage in stages )
        {
            if ( stage.info.stageSerial == data.stageSerial )
            {
                stage.Initialize( data );
                return;
            }
        }
    }

    private void AckInsertStage( Packet _packet )
    {
        var data = Packet.FromJson<STAGE_INFO>( _packet );

        StageData newStage = pool.Spawn();
        newStage.Initialize( data );

        stages.Add( newStage );
    }

    private void AckDeleteStage( Packet _packet )
    {
        var data = Packet.FromJson<STAGE_INFO>( _packet );
        foreach ( var stage in stages )
        {
            if ( stage.info.stageSerial == data.stageSerial )
            {
                stages.Remove( stage );
                pool.Despawn( stage );
                return;
            }
        }
    }
}
