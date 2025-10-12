using UnityEngine;

public class MarkerRenderer : MonoBehaviour
{
    public Vector2 startPos;
    public Vector2 targetPos;
    private readonly float Duration = .5f;
    private float time;
    private bool  isStart;

    private void Update()
    {
        if ( !isStart )
             return;

        time += Time.deltaTime;
        
        float t = Mathf.Clamp01( time / Duration );
        t = t * t * ( 3f - 2f * t ); // smooth damp
        transform.localPosition = Vector2.Lerp( startPos, targetPos, t );
        if ( t >= 1f )
             isStart = false;
    }

    public void SetInfo( float _pos )
    {
        startPos  = transform.localPosition;
        targetPos = new Vector2( _pos, startPos.y );
        isStart   = true;
        time      = 0f;
    }
    public void Clear()
    {
        isStart = false;
        time = 0f;
        transform.localPosition = new Vector2( 0f, transform.localPosition.y );
        startPos  = transform.localPosition;
        targetPos = transform.localPosition;

    }
}
