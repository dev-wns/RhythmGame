using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Resolution : OptionButton
{
    private enum ResolutionType { _1920_1080, _1600_1024, _1280_960, _1024_768, _800_600, }

    protected override void Awake()
    {
        base.Awake();
    }

    public override void Process()
    {
        objectGroup.SetActive( true );
        subOptionCanvas.SetActive( true );

        currentScene.ChangeKeyAction( SceneAction.Resolution );
    }

    private void ChangeValue()
    {
        currentValueText.text = curOption.name;
        var datas = curOption.name.Split( 'x' );
        int width = int.Parse( datas[0] );
        int height = int.Parse( datas[1] );
        Debug.Log( $"Width {width} Height {height}" );
        //Screen.SetResolution( width, height, GlobalSetting.CurrentFullScreenMode );
    }

    public override void KeyBind()
    {
        StaticSceneKeyAction scene = new StaticSceneKeyAction();
        scene.Bind( KeyCode.UpArrow, KeyType.Down, () => PrevMove() );
        scene.Bind( KeyCode.DownArrow, KeyType.Down, () => NextMove() );

        scene.Bind( KeyCode.Escape, KeyType.Down, () => subOptionCanvas.SetActive( false ) );
        scene.Bind( KeyCode.Escape, KeyType.Down, () => objectGroup.SetActive( false ) );
        scene.Bind( KeyCode.Escape, KeyType.Down, () => currentScene.ChangeKeyAction( SceneAction.LobbyOption ) );

        scene.Bind( KeyCode.Return, KeyType.Down, () => ChangeValue() );

        currentScene.KeyBind( SceneAction.Resolution, scene );
    }
}
