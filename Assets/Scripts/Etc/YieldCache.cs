using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YieldCache : MonoBehaviour
{
    class FloatComparer : IEqualityComparer<float>
    {
        bool IEqualityComparer<float>.Equals( float x, float y )
        {
            return x == y;
        }

        int IEqualityComparer<float>.GetHashCode( float obj )
        {
            return obj.GetHashCode();
        }
    }

    private static readonly Dictionary<float/*time*/, WaitForSeconds> times = new Dictionary<float, WaitForSeconds>( new FloatComparer() );
    public static readonly WaitForEndOfFrame WaitForEndOfFrame = new WaitForEndOfFrame();
    public static WaitForSeconds WaitForSeconds( float _time )
    {
        WaitForSeconds wfs;
        if ( !times.TryGetValue( _time, out wfs ) )
            times.Add( _time, wfs = new WaitForSeconds( _time ) );
        return wfs;
    }
}