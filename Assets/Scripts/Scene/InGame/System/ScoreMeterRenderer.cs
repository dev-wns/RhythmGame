using UnityEngine;
using UnityEngine.UI;

public class ScoreMeterRenderer : MonoBehaviour, IObjectPool<ScoreMeterRenderer>
{
    public ObjectPool<ScoreMeterRenderer> pool { get; set; }

    private SpriteRenderer rdr;
    private Color colorCache;

    private static readonly float AliveTime = .875f;
    private static readonly float Duration  = 2.25f;
    private float alpha;
    private float offset;
    public bool isStart;
    private float time;

    private void Awake()
    {
        rdr = GetComponent<SpriteRenderer>();
        Clear();
    }

    private void Update()
    {
        if ( !isStart )
             return;

        time += Time.deltaTime;
        if ( time > AliveTime )
        {
            alpha -= ( Time.deltaTime / Duration ) * offset;
            rdr.color = new Color( colorCache.r, colorCache.g, colorCache.b, alpha );

            if ( alpha <= 0f )
            {
                Clear();
                pool.Despawn( this );
            }
        }
    }

    public void Clear()
    {
        isStart   = false;
        rdr.color = Color.clear;
        time      = 0f;
    }

    public void SetInfo( Color _color, Vector2 _pos )
    {
        isStart = true;
        colorCache = rdr.color = _color;
        alpha = offset = _color.a;
        transform.position = _pos;
    }
}