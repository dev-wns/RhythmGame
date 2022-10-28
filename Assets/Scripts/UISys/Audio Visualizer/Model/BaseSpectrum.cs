using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseSpectrum : MonoBehaviour
{
    public AudioVisualizer visualizer;
    public Transform prefab;
    public bool isGradationColor;
    [Range(0f,1f)]
    public float gradationColorAlpha = 1f;
    public Color color = Color.white;
    public int sortingOrder;

    public float lerpOffset = .275f;

    [Range(0, 100)]
    public int   specStartIndex;
    public int   specCount;
    public float specPower;
    public float specWidth;
    public float specBlank;

    protected float Offset => specWidth + specBlank;
    protected float Power  => specPower * AdditionalPower;
    //protected float Highest => visualizer.Highest;
    protected float Average => visualizer.Average;
    protected float Bass    => visualizer.Bass;

    protected Transform[] transforms; // 생성된 모델 안의 Transform
    private readonly float AdditionalPower = 1000f;

    protected virtual void Awake()
    {
        visualizer.OnUpdateSpectrums += UpdateSpectrums;
        CreateSpectrumModel();
    }

    protected abstract void CreateSpectrumModel();
    protected abstract void UpdateSpectrums( float[][] _values );

    protected Color GetGradationColor( int _index )
    {
        int r = 0, g = 0, b = 0;
        float a = ( 1.0f - ( ( 1.0f / specCount * ( specCount - _index ) ) ) ) / 0.25f;
        int X = ( int )Mathf.Floor( a );
        int Y = ( int )Mathf.Floor( 255 * ( a - X ) );
        switch ( X )
        {
            case 0:
            r = 255;
            g = Y;
            b = 0;
            break;
            case 1:
            r = 255 - Y;
            g = 255;
            b = 0;
            break;
            case 2:
            r = 0;
            g = 255;
            b = Y;
            break;
            case 3:
            r = 0;
            g = 255 - Y;
            b = 255;
            break;
            case 4:
            r = Y;
            g = 0;
            b = 255;
            break;
            case 5:
            r = 255;
            g = 0;
            b = 255;
            break;
        }

        return new Color( r / 255.0f, g / 255.0f, b / 255.0f, gradationColorAlpha );
    }
}
