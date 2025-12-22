using TMPro;
using UnityEngine;

using static PacketType;
public class StageData : MonoBehaviour
{
    public TextMeshProUGUI title;
    public TextMeshProUGUI host;
    public TextMeshProUGUI song;
    public TextMeshProUGUI isPlaying;
    public TextMeshProUGUI personnel;

    public STAGE_INFO info;

    public void Initialize( STAGE_INFO _info )
    {
        info = _info;
        transform.localScale = Vector3.one;

        title.SetText( info.title );
        host.SetText( info.host );
        song.SetText( info.song );
        isPlaying.SetText( _info.isPlaying ? "PLAYING" : "WAITING" );
        personnel.SetText( "{0} / {1}", info.personnel.current, info.personnel.maximum );
    }

    public void EntryStage()
    {
        //if ( SceneBase.IsLock )
        //    return;

        //SceneBase.IsLock = true;
        Network.Inst.Send( new Packet( ENTRY_STAGE_REQ, info ) );
    }
}
