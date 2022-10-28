using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineSpectrum : BaseSpectrum
{
    public bool isReverse;
    [Range(0f, 1f)]
    public float decrease;
    private float[] cached;

    protected override void CreateSpectrumModel()
    {
        cached     = new float[specCount * 2];
        transforms = new Transform[specCount * 2];
        int symmetryColorIdx = 0;
        for ( int i = 0; i < specCount * 2; i++ )
        {
            Transform obj = Instantiate( prefab, this.transform );
            transforms[i] = obj.transform;

            var rdr = obj.GetComponent<SpriteRenderer>();
            rdr.sortingOrder = sortingOrder;

            rdr.color = !isGradationColor ? color :
                        i < specCount     ? GetGradationColor( i ) :
                                            GetGradationColor( symmetryColorIdx++ );
        }
    }

    protected override void UpdateSpectrums( float[][] _values )
    {
        int index;
        var halfOffset = Offset * .5f;
        for ( int i = 0; i < specCount; i++ )
        {
            index = isReverse ? specStartIndex + specCount - i - 1 : specStartIndex + i;
            float value  = ( _values[0][index] + _values[1][index] ) *.5f;

            if ( cached[i] < value )
                 cached[i] = value;

            if ( cached[i] > value )
                 cached[i] = Mathf.Clamp01( cached[i] - Mathf.Lerp( 0f, decrease, Global.Math.Abs( value - cached[i] ) ) );
            float scale = Mathf.Lerp( transforms[i].localScale.y, cached[i] * Power, lerpOffset );

            Transform left  = transforms[i];
            Transform right = transforms[specCount + i];

            Vector3 newScale = new Vector3( specWidth, scale, 1f );
            left.localScale = right.localScale = newScale;

            float posX = i == 0 ? halfOffset * ( i + 1 ) : ( Offset * ( i + 1 ) ) - halfOffset;
            posX += transform.position.x;
            left.position  = new Vector3( -posX, transform.position.y, transform.position.z );
            right.position = new Vector3(  posX, transform.position.y, transform.position.z );
        }
    }
}