using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FreqColor : MonoBehaviour
{
    public MultipleFreqBand freqBand;
    public List<Image> leftBars  = new List<Image>();
    public List<Image> rightBars = new List<Image>();
    public bool isReverse;

    [Header("Frequency")]
    private float[] buffer;
    private int     freqCount;
    public float decreasePower = 1f;

    private void Awake()
    {
        freqBand.OnUpdateBand += UpdateColors;
        freqCount = ( int )( ( leftBars.Count + rightBars.Count ) * .5f );
        buffer = new float[freqCount];

        if ( isReverse ) rightBars.Reverse();
        else             leftBars.Reverse();
    }

    private void UpdateColors( float[] _values )
    {
        for ( int i = 0; i < freqCount; i++ )
        {
            float value = Global.Math.Clamp( _values[i] - freqBand.Average, 0f, 1f );
            buffer[i] = buffer[i] < value ? value :
                                            Global.Math.Clamp( buffer[i] - ( ( .001f + buffer[i] ) * decreasePower * Time.deltaTime ), .05f, 1f );

            leftBars[i].color = rightBars[i].color = new Color( 1, 1, 1, buffer[i] );
        }
    }
}
