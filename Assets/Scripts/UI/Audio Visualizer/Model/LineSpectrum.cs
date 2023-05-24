using UnityEngine;

public class LineSpectrum : BaseSpectrum
{
    private float[] buffer;

    protected Transform[] transforms; // 생성된 모델 안의 Transform

    [Header("Speed Control")]
    [Range(0f, 50f)] 
    public float decreasePower;

    [Header("Normalize")]
    public bool IsNormalized;
    public readonly int NormalizedRange = 2;

    [Header("Etc.")]
    public bool isReverse;
    public bool isPositionUpdate;

    protected override void CreateSpectrumModel()
    {
        buffer     = new float[specCount];
        transforms = new Transform[specCount * 2];
        int symmetryColorIdx = 0;

        for ( int i = 0; i < specCount * 2; i++ )
        {
            Transform obj = Instantiate( prefab, transform );
            transforms[i] = obj.transform;

            var rdr = obj.GetComponent<SpriteRenderer>();
            rdr.sortingOrder = sortingOrder;

            rdr.color = !isGradationColor ? color :
                        i < specCount     ? GetGradationColor( i ) :
                                            GetGradationColor( symmetryColorIdx++ );

            transforms[i].position = i < specCount ? new Vector3( transform.position.x + -GetIndexToPositionX( i ),             transform.position.y, transform.position.z ) :
                                                     new Vector3( transform.position.x +  GetIndexToPositionX( i - specCount ), transform.position.y, transform.position.z );
        }
    }

    protected override void UpdateSpectrums( float[][] _values )
    {
        for ( int i = 0; i < specCount; i++ )
        {
            // 인스펙터 상의 스펙트럼 시작위치부터 값을 받아온다.
            int index = isReverse ? specStartIndex + specCount - i - 1 : specStartIndex + i;

            // 스테레오( 0 : Left, 1 : Right ) 스펙트럼 값 평균.
            float value = ( _values[0][index] + _values[1][index] ) * .5f;
            if ( IsNormalized )
            {
                float sumValue = 0f;
                int start = Global.Math.Clamp( index - NormalizedRange, 0, 4096 );
                int end   = Global.Math.Clamp( index + NormalizedRange, 0, 4096 );
                for ( int idx = start; idx <= end; idx++ )
                      sumValue += ( _values[0][idx] + _values[1][idx] ) * .5f;

                value = sumValue / ( end - start + 1 );
            }

            // 이전 값과의 차이가 클수록 빠르게 변화하도록 한다.
            float diffAbs = Global.Math.Abs( buffer[i] - value );
            float cache   = Global.Math.Clamp( buffer[i] - ( diffAbs * decreasePower * Time.deltaTime ), .0005f, 1f );
            buffer[i] = cache < value ? value : cache;


            // 계산된 값으로 스케일 조절.
            Transform left  = transforms[i];
            Transform right = transforms[specCount + i];
            left.localScale = right.localScale = new Vector3( specWidth, buffer[i] * Power, 1f );

            if ( isPositionUpdate )
            {
                float posX = GetIndexToPositionX( i );
                left.position  = new Vector3( transform.position.x + -posX, transform.position.y, transform.position.z );
                right.position = new Vector3( transform.position.x +  posX, transform.position.y, transform.position.z );
            }
        }
    }

    private float GetIndexToPositionX( int _index )
    {
        return _index == 0 ? Offset * .5f : ( Offset * ( _index + 1 ) ) - ( Offset * .5f );
    }
}