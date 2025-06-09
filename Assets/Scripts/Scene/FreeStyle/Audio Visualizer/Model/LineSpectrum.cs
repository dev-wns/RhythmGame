using UnityEngine;

public class LineSpectrum : MonoBehaviour
{
    [Header("Base")]
    public AudioVisualizer visualizer;
    private float[]     buffer;
    private Transform[] transforms; // 생성된 모델 안의 Transform

    [Header("Color")]
    [Range(0f,1f)] public float alpha = 1f;

    [Header("Spectrum")]
    public Transform prefab;
    public int   sortingOrder;
    public int   specCount;
    public int   startIndex;
    public float width;
    public float blank;
    public float power;

    [Header("Speed Control")]
    [Range(0f, 50f)]
    public float dropAmount;

    [Header("Normalize")]
    public bool IsNormalized;
    public int  normalizedRange = 2;

    [Header("Etc.")]
    public bool isReverse;

    private void Awake()
    {
        if ( visualizer is not null )
        {
            visualizer.OnUpdateSpectrums += UpdateSpectrums;
            CreateSpectrumModel();
        }
    }

    private void CreateSpectrumModel()
    {
        buffer = new float[specCount];
        transforms = new Transform[specCount * 2];
        int symmetryColorIdx = 0;

        for ( int i = 0; i < specCount * 2; i++ )
        {
            Transform obj = Instantiate( prefab, transform );
            transforms[i] = obj.transform;

            var rdr = obj.GetComponent<SpriteRenderer>();
            rdr.sortingOrder = sortingOrder;

            // Gradation Reverse
            rdr.color              = i < specCount ? GetGradationColor( specCount - i ) : GetGradationColor( specCount - symmetryColorIdx++ );
            transforms[i].position = i < specCount ? new Vector3( transform.position.x - GetIndexToPositionX( i ),             transform.position.y, transform.position.z ) :
                                                     new Vector3( transform.position.x + GetIndexToPositionX( i - specCount ), transform.position.y, transform.position.z );
        }
    }

    private void UpdateSpectrums( float[][] _values )
    {
        for ( int i = 0; i < specCount; i++ )
        {
            // 인스펙터 상의 스펙트럼 시작위치부터 값을 받아온다.
            int index = isReverse ? startIndex + specCount - i - 1 : startIndex + i;

            // 스테레오( 0 : Left, 1 : Right ) 스펙트럼 값 평균.
            float value = ( _values[0][index] + _values[1][index] ) * .5f;
            if ( IsNormalized )
            {
                float sumValue = 0f;
                int start = Global.Math.Clamp( index - normalizedRange, 0, 4096 );
                int end   = Global.Math.Clamp( index + normalizedRange, 0, 4096 );
                for ( int idx = start; idx <= end; idx++ )
                    sumValue += ( _values[0][idx] + _values[1][idx] ) * .5f;

                value = sumValue / ( end - start + 1 );
            }

            buffer[i] -= ( ( buffer[i] * dropAmount ) * Time.deltaTime );
            buffer[i] = Mathf.Max( buffer[i], value );

            Transform left  = transforms[i];
            Transform right = transforms[specCount + i];
            left.localScale = right.localScale = new Vector3( width, buffer[i] * power, 1f );
        }
    }

    private float GetIndexToPositionX( int _index )
    {
        return _index == 0 ? ( width + blank ) * .5f : ( ( width + blank ) * ( _index + 1 ) ) - ( ( width + blank ) * .5f );
    }

    private Color GetGradationColor( int _index )
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

        return new Color( r / 255.0f, g / 255.0f, b / 255.0f, alpha );
    }
}