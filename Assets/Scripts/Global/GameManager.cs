using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public static bool IsMultiPlaying { get; set; }
    public static USER_INFO? UserInfo { get; set; }
    public static STAGE_INFO? StageInfo { get; set; }

}
