using UnityEngine;

[CreateAssetMenu( fileName = "BGAIconSO", menuName = "Scriptable Objects/Image/BGAIconSO" )]
public class BGAIconSO : ScriptableObject
{
    public Sprite video;
    public Sprite sprite;
    public Sprite image;

    public Sprite GetSprite( in Song _song )
    {
        return _song.hasVideo  ? video  :
               _song.hasSprite ? sprite :
                                 image;
    }
}