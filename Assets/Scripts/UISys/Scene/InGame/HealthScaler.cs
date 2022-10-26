using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class HealthScaler : MonoBehaviour
{
    public HealthSystem system;
    public RectTransform Pos;
    public float speed = 10f;
    private Transform tf;
    private float health, curHealth;
    private Vector2 initScl;

    private bool isReverse = true;
    private float loop0_1;

    private void Awake()
    {
        tf = transform;
        initScl = tf.localScale;
        health = curHealth = HealthSystem.MaxHealth;
        system.OnChangedHealth += _health => health = _health;
    }

    private void Start()
    {
        transform.position = Pos.position;
    }

    private void FixedUpdate()
    {
        float deltaTime = speed * Time.fixedDeltaTime;
        loop0_1 += isReverse ? deltaTime : -deltaTime;
        isReverse = loop0_1 <= 0f || loop0_1 >= 1f ? !isReverse : isReverse;

        float offset = Global.Math.Lerp( curHealth, health, deltaTime );
        curHealth = offset < 1f ? 0f : offset;

        float maxHeight = Global.Math.Clamp( initScl.y * curHealth / HealthSystem.MaxHealth, 0f, initScl.y );
        float minHeight = Global.Math.Clamp( maxHeight * .9f, 0f, initScl.y );
        float height    = ( minHeight + ( ( maxHeight - minHeight ) * loop0_1 ) );

        tf.localScale = new Vector2( initScl.x, Global.Math.Clamp( height, minHeight, maxHeight ) );
    }
}
