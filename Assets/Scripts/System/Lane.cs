using System;
using UnityEngine;

public class Lane : MonoBehaviour
{
    public int Key { get; private set; }
    public InputSystem InputSys { get; private set; }

    public event Action<int/*Lane Key*/> OnLaneInitialize;

    [Header("Effect")]
    public SpriteRenderer laneEffect;
    private bool ShouldPlayLaneEffect;
    private Color color;

    private readonly float StartAlpha = 1f;
    private readonly float Duration = .2f;
    private float offset;
    private float alpha;

    private void Awake()
    {
        InputSys = GetComponent<InputSystem>();

        if ( ( GameSetting.CurrentVisualFlag & VisualFlag.LaneEffect ) != 0 )
        {
            InputSys.OnInputEvent += LaneEffect;
            InputSys.OnStopEffect += () => ShouldPlayLaneEffect = true;
        }

        offset = StartAlpha / Duration;
    }

    private void LaneEffect( KeyState _type )
    {
        if ( _type == KeyState.Down )
        {
            ShouldPlayLaneEffect = false;
            laneEffect.color = color;
            alpha = StartAlpha;
        }
        else if ( _type == KeyState.Up )
        {
            ShouldPlayLaneEffect = true;
        }
    }

    private void Update()
    {
        if ( ShouldPlayLaneEffect )
        {
            alpha -= offset * Time.deltaTime;
            laneEffect.color = new Color( color.r, color.g, color.b, alpha );
            if ( alpha < 0 )
                ShouldPlayLaneEffect = false;
        }
    }

    public void SetLane( int _key )
    {
        Key = _key;
        UpdatePosition( _key );
        OnLaneInitialize?.Invoke( Key );

        if ( NowPlaying.KeyCount == 4 )
        {
            color = _key == 1 || _key == 2 ? new Color( 0f, 0f, 1f, StartAlpha ) : new Color( 1f, 0f, 0f, StartAlpha );
        }
        else if ( NowPlaying.KeyCount == 6 )
        {
            color = _key == 1 || _key == 4 ? new Color( 0f, 0f, 1f, StartAlpha ) : new Color( 1f, 0f, 0f, StartAlpha );
        }
        else if ( NowPlaying.KeyCount == 7 )
        {
            color = _key == 1 || _key == 5 ? new Color( 0f, 0f, 1f, StartAlpha ) :
                                 _key == 3 ? new Color( 1f, 1f, 0f, StartAlpha ) : new Color( 1f, 0f, 0f, StartAlpha );
        }
    }

    public void UpdatePosition( int _key )
    {
        transform.position = new Vector3( GameSetting.NoteStartPos + ( GameSetting.NoteWidth * _key ) + ( GameSetting.NoteBlank * _key ) + GameSetting.NoteBlank, GameSetting.JudgePos, 0f );

        if ( GameSetting.CurrentVisualFlag.HasFlag( VisualFlag.LaneEffect ) )
        {
            laneEffect.transform.position = new Vector3( transform.position.x, GameSetting.JudgePos, transform.position.z );
            laneEffect.transform.localScale = new Vector3( GameSetting.NoteWidth, 250f, 1f );
        }
        else
        {
            laneEffect.gameObject.SetActive( false );
        }
    }
}
