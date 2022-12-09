using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineSpectrum : BaseSpectrum
{
    private float[] cached;

    [Range(0f, 50f)] 
    public float decreaseLerpPower;
    public float increasePower;
    public bool isReverse;
    public bool isPositionUpdate;

    protected override void CreateSpectrumModel()
    {
        cached     = new float[specCount];
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

            transforms[i].position = i < specCount ? new Vector3( -GetIndexToPositionX( i ),             transform.position.y, transform.position.z ) :
                                                     new Vector3(  GetIndexToPositionX( i - specCount ), transform.position.y, transform.position.z );
        }
    }

    protected override void UpdateSpectrums( float[][] _values )
    {
        for ( int i = 0; i < specCount; i++ )
        {
            // �ν����� ���� ����Ʈ�� ������ġ���� ���� �޾ƿ´�.
            int index = isReverse ? specStartIndex + specCount - i - 1 : specStartIndex + i;

            // ���׷���( 0 : Left, 1 : Right ) ����Ʈ�� �� ���.
            float value  = ( _values[0][index] + _values[1][index] ) * .5f;

            // ���� ���� ����Ʈ�� ���� ���̰� Ŭ���� ������ ��ȭ�ϵ��� �Ѵ�.
            float diffAbs = Global.Math.Abs( cached[i] - value );
            float amount  = Global.Math.Lerp( 0f, 1f, diffAbs * decreaseLerpPower * Time.deltaTime );
            cached[i] += cached[i] < value ? amount * increasePower : -amount;

            // ���� ������ ������ ����.
            Transform left  = transforms[i];
            Transform right = transforms[specCount + i];
            left.localScale = right.localScale = new Vector3( specWidth, cached[i] * Power, 1f );

            if ( isPositionUpdate )
            {
                float posX = GetIndexToPositionX( i );
                left.position  = new Vector3( -posX, transform.position.y, transform.position.z );
                right.position = new Vector3(  posX, transform.position.y, transform.position.z );
            }
        }
    }

    private float GetIndexToPositionX( int _index )
    {
        return _index == 0 ? Offset * .5f : ( Offset * ( _index + 1 ) ) - ( Offset * .5f );
    }
}