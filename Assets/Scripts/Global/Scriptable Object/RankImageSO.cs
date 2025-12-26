using UnityEngine;

[CreateAssetMenu( fileName = "RankImageSO", menuName = "Scriptable Objects/Image/RankImageSO" )]
public class RankImageSO : ScriptableObject
{
    public Sprite rankS;
    public Sprite rankA;
    public Sprite rankB;
    public Sprite rankC;
    public Sprite rankD;

    public Sprite GetSprite( double _accuracy )
    {
        return _accuracy switch 
        {
            >= 95d => rankS,
            >= 90d => rankA,
            >= 85d => rankB,
            >= 80d => rankC,
            _      => rankD,
        };
    }
}