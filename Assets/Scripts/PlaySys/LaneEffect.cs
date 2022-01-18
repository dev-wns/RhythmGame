using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaneEffect : MonoBehaviour
{
    private Lane lane;
    private SpriteRenderer rdr;
    private static readonly Color LaneColorRed = new Color( 1f, 0f, 0f, .25f );
    private static readonly Color LaneColorBlue = new Color( 0f, 0f, 1f, .25f );

    private Color color; 
    
    private void Awake()
    {
        lane = GetComponentInParent<Lane>();
        rdr = GetComponent<SpriteRenderer>();

        lane.InputSys.OnInputEvent += LaneEffectEnabled;

        if ( lane.Key == 1 || lane.Key == 4 ) color = LaneColorBlue;
        else                                  color = LaneColorRed;

        transform.localScale = new Vector3( GameSetting.NoteWidth, ( Screen.height * .5f ) - GameSetting.JudgePos, 1f );
    }

    private void LaneEffectEnabled( bool _isEnable )
    {
        // sprite renderer Enable로 활성화 시키는것보다
        // color 값 변경하는게 6배정도 빠름.

        // laneEffect.enabled = _isEnable;
        rdr.color = _isEnable ? color : Color.clear;
    }
}
