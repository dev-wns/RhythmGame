using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lane : MonoBehaviour
{
    public int Key { get; private set; }
    public NoteSystem  NoteSys  { get; private set; }
    public InputSystem InputSys { get; private set; }

    private void Awake()
    {
        NoteSys  = GetComponent<NoteSystem>();
        InputSys = GetComponent<InputSystem>();
    }

    public void SetLane( int _key )
    {
        Key = _key;
        NoteSys.lane = InputSys.lane = this;

        transform.position = new Vector3( GameSetting.NoteStartPos + ( GameSetting.NoteWidth * Key ) +
                                        ( GameSetting.NoteBlank * Key ) + GameSetting.NoteBlank,
                                          GameSetting.JudgePos, 0f );
    }
}
