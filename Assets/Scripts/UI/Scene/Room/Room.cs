using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

using static PacketType;
public class Room : MonoBehaviour
{
    public TextMeshProUGUI title;
    public TextMeshProUGUI host;
    public TextMeshProUGUI song;
    public TextMeshProUGUI personnel;

    public STAGE_INFO info;

    public void Initialize( STAGE_INFO _info )
    {
        info = _info;
        transform.localScale = Vector3.one;

        title.text     = info.title;
        host.text      = info.host;
        song.text      = info.song;
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
