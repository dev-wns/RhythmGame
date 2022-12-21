using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RendererFadeEffectArray : BaseRendererEffectArray
{
    public RendererFadeEffectArray( SpriteRenderer[] _rdr, float _start, float _end, float _duration )
                                    : base( _rdr, _start, _end, _duration )
                                    => Restart();

    public RendererFadeEffectArray( List<SpriteRenderer> _rdr, float _start, float _end, float _duration )
                                    : base( _rdr, _start, _end, _duration )
                                    => Restart();

    public override bool Process()
    {
        if ( ReferenceEquals( rdr, null ) || isDuplicateValue || rdr.Length == 0 )
             return true;

        elapsed += ( offset * Time.deltaTime ) / duration;
        for ( int i = 0; i < rdr.Length; i++ )
        {
            Color newColor = rdr[i].color;
            newColor.a = elapsed;
            rdr[i].color = newColor;
        }

        if ( isFadeIn ? elapsed > end :
                        elapsed < end )
        {
            for ( int i = 0; i < rdr.Length; i++ )
            {
                Color newColor = rdr[i].color;
                newColor.a = end;
                rdr[i].color = newColor;
            }
            OnCompleted?.Invoke();
            return true;
        }
        return false;
    }

    public override void Restart()
    {
        for ( int i = 0; i < rdr.Length; i++ )
        {
            Color newColor = rdr[i].color;
            newColor.a     = start;
            rdr[i].color   = newColor;
        }

        offset = end - start;
        isFadeIn = start < end;
        elapsed = start;

        isDuplicateValue = Global.Math.Abs( start - end ) < float.Epsilon;
    }
}
