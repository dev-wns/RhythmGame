using UnityEngine;

public class ExitActivator : CanvasActivator
{
    public void Quit() => Application.Quit();

    public override void EnableCanvas()
    {
        if ( !Browser.Inst.IsSearching )
             base.EnableCanvas();
    }

    protected override void KeyBind( UnityInputAction _input )
    {
        _input.Bind( SceneAction.Main, KeyCode.Escape, EnableCanvas  );
        _input.Bind( SceneAction.Exit, KeyCode.Escape, DisableCanvas );
    }
}
