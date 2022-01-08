using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum JUDGE_TYPE { Kool, Cool, Good, Miss }
public class Judgement : MonoBehaviour
{
    private RectTransform rt;

    private const int Kool = 22;
    private const int Cool = 35 + Kool;
    private const int Good = 28 + Cool;

    private int koolCount, coolCount, goodCount, missCount;
    public bool isShowRange = true;
    public Transform koolImage, coolImage, goodImage;


    private void Awake()
    {
        rt = transform as RectTransform;
        rt.anchoredPosition = new Vector3( 0f, GlobalSetting.JudgeLine, -1f );
        rt.sizeDelta = new Vector3( GlobalSetting.GearWidth, GlobalSetting.JudgeHeight, 1f );

        if ( !isShowRange )
        {
            koolImage.gameObject.SetActive( false );
            coolImage.gameObject.SetActive( false );
            goodImage.gameObject.SetActive( false );
        }

        koolImage.transform.position = transform.position;
        coolImage.transform.position = transform.position;
        goodImage.transform.position = transform.position;
    }

    private void Update()
    {
        if ( isShowRange )
        {
            koolImage.localScale = new Vector2( transform.localScale.x, ( InGame.GetChangedTimed( Mathf.Abs( InGame.Playback - Kool ) ) - InGame.PlaybackChanged ) * InGame.Weight );
            coolImage.localScale = new Vector2( transform.localScale.x, ( InGame.GetChangedTimed( Mathf.Abs( InGame.Playback - Cool ) ) - InGame.PlaybackChanged ) * InGame.Weight );
            goodImage.localScale = new Vector2( transform.localScale.x, ( InGame.GetChangedTimed( Mathf.Abs( InGame.Playback - Good ) ) - InGame.PlaybackChanged ) * InGame.Weight );
        }
    }

    public bool IsCalculated( float _diff )
    {
        bool isDone = true;
        float diffAbs = Mathf.Abs( _diff );

        // Kool 22 Cool 35 Good 28
        if ( diffAbs <= Kool )
        {
            GameManager.Combo++;
            GameManager.Kool++;
            koolCount++;
        }
        else if ( diffAbs > Kool && diffAbs <= Cool )
        {
            GameManager.Combo++;
            GameManager.Cool++;
            coolCount++;
        }
        else if ( diffAbs > Cool && diffAbs <= Good )
        {
            GameManager.Combo++;
            GameManager.Good++;
            goodCount++;
        }
        else
        {
            isDone = false;
        }

        return isDone;
    }

    public bool IsMiss( float _diff )
    {
        if ( _diff < -Good )
        {
            missCount++;
            return true;
        }
        else
        {
            return false;
        }
    }
}
