using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaneEffect : MonoBehaviour
{
    private Lane lane;
    private InputSystem inputSystem;
    private SpriteRenderer rdr;

    private static readonly Color LaneColorRed  = new Color( 1f, .7f, .7f, .35f );
    private static readonly Color LaneColorBlue = new Color( .7f, .7f, 1f, .35f );

    private Color color; 
    
    private void Awake()
    {
        rdr = GetComponent<SpriteRenderer>();
        
        lane = GetComponentInParent<Lane>();
        lane.OnLaneInitialize += Initialize;
        
        inputSystem = GetComponentInParent<InputSystem>();
        inputSystem.OnInputEvent += LaneEffectEnabled;

        transform.localScale = new Vector3( GameSetting.NoteWidth, ( Screen.height * .5f ) - GameSetting.JudgePos, 1f );
    }

    private void Initialize( int _key )
    {
        if ( _key == 1 || _key == 4 ) color = LaneColorBlue;
        else color = LaneColorRed;
    }

    private void LaneEffectEnabled( bool _isEnable )
    {
        // sprite renderer Enable로 활성화 시키는것보다
        // color 값 변경하는게 6배정도 빠름.

        // laneEffect.enabled = _isEnable;
        rdr.color = _isEnable ? color : Color.clear;
    }
}
