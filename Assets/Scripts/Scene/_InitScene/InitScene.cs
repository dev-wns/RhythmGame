using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

public class InitScene : Scene
{
    [Header( "Rotate Loading Icon" )]
    public GameObject icon;
    public float rotateSpeed = 100f;
    private float curValue;

    protected override void Awake()
    {
        base.Awake();

        QualitySettings.maxQueuedFrames = 0;
        Cursor.visible = false;
        DOTween.Init( true, false, LogBehaviour.Default ).SetCapacity( 50, 20 );

        OnKeyBind += KeyBind;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        OnKeyBind -= KeyBind;
    }

    protected async override void Start()
    {
        await FadeIn();
        await Task.Run( DataStorage.Inst.LoadSongs );
        await UniTask.WaitForSeconds( 3f );

        await LoadScene( SceneType.FreeStyle );
    }

    private void Update()
    {
        curValue -= Time.deltaTime * rotateSpeed;
        icon.transform.rotation = Quaternion.Euler( new Vector3( 0f, 0f, curValue ) );
    }

    public void KeyBind( UnityInputAction _input )
    {
        _input.Bind( SceneAction.Main, KeyCode.Return, () => { } );
    }

    protected override void Connect() { }

    protected override void Disconnect() { }
}
