using System;

namespace Global
{
    /// <summary> °¡º­¿î Math </summary>
    public static class Math
    {
        public static float  Lerp( float _start, float _end, float _t )       => _start + ( _end - _start ) * _t;
        public static double Lerp( double _start, double _end, double _t )    => _start + ( _end - _start ) * _t;
        public static double Abs( double _value )                             => _value >= 0d ? _value : -_value;
        public static float  Abs( float _value )                              => _value >= 0f ? _value : -_value;
        public static int    Abs( int _value )                                => _value >= 0 ? _value : -_value;
        public static int    Min( int _arg1, int _arg2 )                      => _arg1 < _arg2 ? _arg1 : _arg2;
        public static float  Min( float _arg1, float _arg2 )                  => _arg1 < _arg2 ? _arg1 : _arg2;
        public static double Min( double _arg1, double _arg2 )                => _arg1 < _arg2 ? _arg1 : _arg2;
        public static int    Max( int _arg1, int _arg2 )                      => _arg1 > _arg2 ? _arg1 : _arg2;
        public static float  Max( float _arg1, float _arg2 )                  => _arg1 > _arg2 ? _arg1 : _arg2;
        public static double Max( double _arg1, double _arg2 )                => _arg1 > _arg2 ? _arg1 : _arg2;
        public static long   Max( long _arg1, long _arg2 )                    => _arg1 > _arg2 ? _arg1 : _arg2;
        public static double Round( double _value )                           => _value >= 0d ? ( int )( _value + .5d ) : ( int )( _value - .5d );
        public static float  Round( float _value )                            => _value >= 0f ? ( int )( _value + .5f ) : ( int )( _value - .5f );
        public static float  Clamp01( float _value )                          => _value < 0f ? 0f : _value > 1f ? 1f : _value;
        public static double Clamp01( double _value )                         => _value < 0d ? 0d : _value > 1d ? 1d : _value;
        public static int    Clamp( int _value, int _min, int _max )          => _value < _min ? _min : _value > _max ? _max : _value;
        public static long   Clamp( long _value, long _min, long _max )         => _value < _min ? _min : _value > _max ? _max : _value;
        public static float  Clamp( float _value, float _min, float _max )    => _value < _min ? _min : _value > _max ? _max : _value;
        public static double Clamp( double _value, double _min, double _max ) => _value < _min ? _min : _value > _max ? _max : _value;
        public static int    Log10( double _value )
        {
            return ( _value >= 10000000u ) ? 7 : ( _value >= 1000000u ) ? 6 :
                   ( _value >= 100000u   ) ? 5 : ( _value >= 10000u   ) ? 4 :
                   ( _value >= 1000u     ) ? 3 : ( _value >= 100u     ) ? 2 :
                   ( _value >= 10u       ) ? 1 : 0;
        }
        public static void Shuffle<T>( T[] _data, int _min, int _max )
        {
            Random rand = new Random( ( int )DateTime.Now.Ticks );
            for ( int i = _max - 1; i > _min; i-- )
            {
                int j = rand.Next( _min, i );
                ( _data[i], _data[j] ) = ( _data[j], _data[i] );
            }
        }
    }
}