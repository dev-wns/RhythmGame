using UnityEngine;

public class ScoreMeterRenderer : MonoBehaviour
{
    public ScoreMeterSystem system;
    private SpriteRenderer rdr;
    private Color colorCache;

    public float Diff { get; private set; }
    public bool IsActive { get; private set; }

    private static readonly float AliveTime = 1f;
    private static readonly float Duration  = 5f;
    private float alpha;
    private float time;

    private void Awake()
    {
        rdr = GetComponent<SpriteRenderer>();
        Clear();
    }

    private void Update()
    {
        if ( !IsActive )
              return;

        time += Time.deltaTime;
        if ( time > AliveTime )
        {
            alpha -= Time.deltaTime / Duration;
            rdr.color = new Color( colorCache.r, colorCache.g, colorCache.b, alpha );

            if ( alpha <= 0f )
                 system.Despawn( this );
        }
    }

    public void Clear()
    {
        IsActive  = false;
        rdr.color = Color.clear;
        time      = 0f;
    }

    public void SetInfo( Color _color, float _diff )
    {
        IsActive = true;
        Diff     = _diff;
        alpha    = _color.a;
        transform.localPosition = new Vector2( Diff, transform.localPosition.y );
        colorCache = rdr.color = _color;
    }
}