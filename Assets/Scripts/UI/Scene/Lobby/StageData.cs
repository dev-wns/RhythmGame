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

        title.text = info.title;
        host.text = info.host;
        song.text = info.song;
        isPlaying.text = _info.isPlaying ? "PLAYING" : "WAITING";
        personnel.text = $"{info.personnel.current} / {info.personnel.maximum}";
    }

    public void EntryStage()
    {
        //if ( SceneBase.IsLock )
        //    return;

        //SceneBase.IsLock = true;
        Network.Inst.Send( new Packet( ENTRY_STAGE_REQ, info ) );
    }
}
