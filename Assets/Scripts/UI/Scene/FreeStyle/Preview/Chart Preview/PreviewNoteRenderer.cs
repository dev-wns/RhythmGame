using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PreviewNoteRenderer : MonoBehaviour, IObjectPool<PreviewNoteRenderer>
{
    public ObjectPool<PreviewNoteRenderer> pool { get; set; }
    protected Note note;
    public Image head, body, tail;
    public float HeadPos => transform.position.y;
    public float TailPos => transform.position.y + BodyLength;
    public double Time => note.time;
    public double Distance => note.noteDistance;
    public double SliderTime => note.sliderTime;
    public double SliderDistance => note.sliderDistance;
    public bool IsSlider => note.isSlider;
    public bool IsKeyDown { get; set; }
    public float BodyLength { get; private set; }
    public KeySound Sound => note.keySound;
    
    protected float column;
    protected static readonly Color NoteFailColor = new Color( .5f, .5f, .5f, 1f );
    protected double newDistance;

    protected static bool IsOnlyBody = false;

    private void Awake()
    {
        head.rectTransform.sizeDelta        = new Vector2( PreviewNoteSystem.NoteWidth, PreviewNoteSystem.NoteHeight );
        body.rectTransform.anchoredPosition = IsOnlyBody ? Vector2.zero : new Vector2( 0, PreviewNoteSystem.NoteHeight * .5f );
        tail.rectTransform.sizeDelta        = new Vector2( PreviewNoteSystem.NoteWidth, PreviewNoteSystem.NoteHeight );
    }

    public void SetInfo( in Note _note, float _startPos, Color _color )
    {
        note = _note;

        column = _startPos + ( note.lane * ( PreviewNoteSystem.NoteWidth ) ) + ( ( note.lane + 1 ) * GameSetting.NoteBlank );
        newDistance = note.noteDistance;

        head.enabled = IsOnlyBody && IsSlider ? false :
                       IsOnlyBody && !IsSlider ? true : true;
        body.enabled = IsOnlyBody ? true : IsSlider;
        tail.enabled = IsOnlyBody ? false : IsSlider;

        head.rectTransform.sizeDelta        = new Vector2( PreviewNoteSystem.NoteWidth, PreviewNoteSystem.NoteHeight );
        body.rectTransform.anchoredPosition = IsOnlyBody ? Vector2.zero : new Vector2( 0, PreviewNoteSystem.NoteHeight * .5f );
        tail.rectTransform.sizeDelta        = new Vector2( PreviewNoteSystem.NoteWidth, PreviewNoteSystem.NoteHeight );

        head.color = body.color = tail.color = _color;
    }

    private void LateUpdate()
    {
        if ( IsSlider )
        {
            if ( Distance < PreviewNoteSystem.Distance )
                 newDistance = PreviewNoteSystem.Distance;

            BodyLength = ( float )( ( SliderDistance - newDistance ) * GameSetting.Weight );

            float length = Global.Math.Clamp( IsOnlyBody ? BodyLength : BodyLength - PreviewNoteSystem.NoteHeight, 0f, float.MaxValue );
            body.rectTransform.sizeDelta        = new Vector2( PreviewNoteSystem.NoteWidth, length );
            tail.rectTransform.anchoredPosition = new Vector2( 0f, length );
            
            transform.localPosition = new Vector2( column, -390f + ( float )( newDistance - PreviewNoteSystem.Distance ) * GameSetting.Weight );
            if ( SliderTime - PreviewNoteSystem.Playback < 0d )
                 pool.Despawn( this );
        }
        else
        {
            transform.localPosition = new Vector2( column, -390f + ( float )( newDistance - PreviewNoteSystem.Distance ) * GameSetting.Weight );
            if ( Time < PreviewNoteSystem.Playback )
                 pool.Despawn( this );
        }
        
    }
}