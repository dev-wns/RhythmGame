using UnityEngine;

public class ReloadActivator : CanvasActivator
{
    protected override void KeyBind( UnityInputAction _input )
    {
        _input.Bind( SceneAction.ReLoad, KeyCode.Escape, DisableCanvas );
    }
}
