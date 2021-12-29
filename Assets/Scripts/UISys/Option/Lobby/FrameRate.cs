using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrameRate : CustomButton
{
    public enum FPSType { _vSync, _NoLimit, _60, _144, _300, _960 };
    public FPSType fpsType;

    public override void Process()
    {
        base.Process();
        fpsType = ( FPSType )key;
        Debug.Log( $"FrameRate {fpsType}" );

        //switch( fpsType )
        //{
        //    case FPSType._vSync:
        //    {
        //        QualitySettings.vSyncCount = 1;
        //    } break;
        //    case FPSType._NoLimit:
        //    {
        //        QualitySettings.vSyncCount = 0;
        //        Application.targetFrameRate = 0;
        //    } break;
        //    case FPSType._60:
        //    {
        //        QualitySettings.vSyncCount = 0;
        //        Application.targetFrameRate = 60;
        //    } break;
        //    case FPSType._144:
        //    {
        //        QualitySettings.vSyncCount = 0;
        //        Application.targetFrameRate = 144;
        //    } break;
        //    case FPSType._300:
        //    {
        //        QualitySettings.vSyncCount = 0;
        //        Application.targetFrameRate = 300;
        //    } break;
        //    case FPSType._960:
        //    {
        //        QualitySettings.vSyncCount = 0;
        //        Application.targetFrameRate = 960;
        //    } break;
        //}
    }
}
