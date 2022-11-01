using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleSpectrum : BaseSpectrum
{
    [Header("Center Image")]
    public Transform centerImage;
    [Min(0f)] public float radius;
    [Min(0f)] public float distance;
    private float imageScale;
    private float bassAmount = 1f;

    protected override void Awake()
    {
        base.Awake();
        if ( visualizer.hasBass )
             visualizer.OnUpdateBass += UpdateBass;
    }

    protected override void CreateSpectrumModel()
    {
        if ( centerImage )
        {
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
            float scale = Mathf.Lerp( transforms[i].localScale.y, value * Power, lerpOffset );

            Transform left  = transforms[i];
            Transform right = transforms[( specCount * 2 ) - 1 - i];

            Vector3 newScale = new Vector3( specWidth, scale, 1f );
            left.localScale = right.localScale = newScale;

            float bassValue = radius * .5f * bassAmount;
            left.position   = left.up  * bassValue;
            right.position  = right.up * bassValue;
        }
    }

    protected void UpdateBass( float _amount )
    {
        bassAmount = 1f + _amount;
        float newImageScale = imageScale * bassAmount;
        centerImage.localScale = new Vector3( newImageScale, newImageScale, newImageScale );
    }
}
