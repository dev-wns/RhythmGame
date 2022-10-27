using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleSpectrum : BaseSpectrum
{
    public Transform centerImage;

    [Min(0f)]
    public float radius;
    [Min(0f)]
    public float distance;

    private bool hasCenterImage;
    private float imageScale;

    protected override void CreateSpectrumModel()
    {
        if ( centerImage )
        {
            hasCenterImage = true;
            imageScale = radius - distance;
            centerImage.localScale = new Vector3( imageScale, imageScale, imageScale );
        }

        int symmetryColorIdx = specCount;
        float angle = 180f / specCount;
        transforms = new Transform[specCount * 2];
        for ( int i = 0; i < specCount * 2; i++ )
        {
            Transform obj = Instantiate( prefab, this.transform );
            transforms[i] = obj.transform;
            transforms[i].rotation = Quaternion.Euler( new Vector3( 0f, 0f, angle + angle * i ) );

            var rdr = obj.GetComponent<SpriteRenderer>();
            rdr.color = !isGradationColor ? color :
                        i < specCount     ? GetGradationColor( i ) :
                                            GetGradationColor( symmetryColorIdx-- );
        }
    }

    protected override void UpdateSpectrums( float[][] _values )
    {
        for ( int i = 0; i < specCount; i++ )
        {
            float value = ( _values[0][i] + _values[1][i] ) *.5f;
            value = ( value / Highest ) * Power;

            float y = transforms[i].localScale.y;
            float scale = Mathf.Lerp( y, value, lerpOffset );

            Transform left  = transforms[i];
            Transform right = transforms[( specCount * 2 ) - 1 - i];

            Vector3 newScale = new Vector3( specWidth, scale, 1f );
            left.localScale = right.localScale = newScale;


            float bassValue = radius * .5f * Bass;
            left.position   = left.up  * bassValue;
            right.position  = right.up * bassValue;
        }

        if ( hasCenterImage )
        {
            float newImageScale = imageScale * Bass;
            centerImage.localScale = new Vector3( newImageScale, newImageScale, newImageScale );
        }
    }
}
