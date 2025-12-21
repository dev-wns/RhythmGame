using UnityEngine;

public class LaneBase : MonoBehaviour
{
    public int Key     { get; private set; } // Lane Index
    public int VKey     { get; private set; } // Virtual KeyCode
    public KeyCode UKey { get; private set; } // Unity KeyCode

    public virtual void Initialize( int _lane )
    {
        Key = _lane;
        UKey = InputManager.Keys[( GameKeyCount ) NowPlaying.KeyCount][Key];
        VKey = InputManager.GetVirtualKey( UKey );

        // 인덱스에 맞는 위치에 레인 배치
        transform.position = new Vector3( GameSetting.NoteStartPos + ( GameSetting.NoteWidth * Key ) + ( GameSetting.NoteBlank * Key ) + GameSetting.NoteBlank, GameSetting.JudgePos, 0f );
    }
}
