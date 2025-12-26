using UnityEngine;

namespace Global.UI
{   
    public static class Screen
    {
        // UI 작업 시 기준( FHD )
        public static int Width  = 1920;
        public static int Height = 1080;

        /// <returns> 텍스처 해상도를 스크린 해상도와의 비율만큼 변환시킵니다. </returns>
        public static Vector2 GetRatio<T>( in T _tex ) where T : Texture
        {
            float texWidth  = _tex.width;
            float texHeight = _tex.height;

            float offsetX = Width / texWidth;
            texWidth  *= offsetX;
            texHeight *= offsetX;

            float offsetY = Height / texHeight;
            if ( offsetY > 1f )
            {
                texWidth  *= offsetY;
                texHeight *= offsetY;
            }

            return new Vector2( texWidth, texHeight );
        }
    }

    public static class Alignment
    {
        public enum Pivot
        {
            TopLeft, Top, TopRight,
            Left, Center, Right,
            BottomLeft, Bottom, BottomRight,
        }

        public static Vector2 GetPivotVector( Pivot _pivot )
        {
            return _pivot switch 
            {
                Pivot.TopLeft     => new Vector2(   0f,   1f ),
                Pivot.Top         => new Vector2( 0.5f,   1f ),
                Pivot.TopRight    => new Vector2(   1f,   1f ),
                Pivot.Left        => new Vector2(   0f, 0.5f ),
                Pivot.Center      => new Vector2( 0.5f, 0.5f ),
                Pivot.Right       => new Vector2(   1f, 0.5f ),
                Pivot.BottomLeft  => new Vector2(   0f,   0f ),
                Pivot.Bottom      => new Vector2( 0.5f,   0f ),
                Pivot.BottomRight => new Vector2(   1f,   0f ),
                _                 => new Vector2( 0.5f, 0.5f ),
            };
        }
    }

    public static class Color
    {
        /// <summary> Clear All ( 0, 0, 0, 0 ) </summary>
        public static UnityEngine.Color Clear    = new UnityEngine.Color( 0f, 0f, 0f, 0f );
        /// <summary> Clear Alpha ( 1, 1, 1, 0 ) </summary>
        public static UnityEngine.Color ClearA   = new UnityEngine.Color( 1f, 1f, 1f, 0f );
        /// <summary> Clear Red, Blue, Green ( 0, 0, 0, 1 ) </summary>
        public static UnityEngine.Color ClearRGB = new UnityEngine.Color( 0f, 0f, 0f, 1f );
    }
}