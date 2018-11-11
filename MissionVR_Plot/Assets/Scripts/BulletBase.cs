using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent( typeof( Rigidbody ) )]
public class BulletBase : Photon.MonoBehaviour
{
    // TODO 読み込んだScriptableObjectによって挙動を変える
    // →スキル系の変更が必要になるため後回し


    #region 発射時に受け取る値
    public EntityBase owner;
    public int damageValue;
    public DamageType damageType;
    public float range;
    public Team team;
    #endregion

    /// <summary>
    /// 銃弾のインスタンスが消えるまでの時間
    /// </summary>
    [SerializeField] private float ttl;
    [SerializeField] private float speed;   // TODO 弾速もgunInfoに登録する？

    private Vector3 prev;
    private Transform tfCache;
    private Rigidbody rbCache;

    private void Awake()
    {
        ttl = ( ttl <= 0 ) ? 5 : ttl;
        range = ( range <= 0 ) ? 50 : range;

        tfCache = transform;
        rbCache = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        prev = tfCache.position;
        rbCache.AddForce( transform.forward * 10 * speed, ForceMode.Impulse );
    }

    float leaveTime;
    private void Update()
    {
        leaveTime += Time.deltaTime;

        if ( Vector3.Distance( prev, tfCache.position ) >= range || leaveTime >= ttl )
        {
            leaveTime = 0;
            StorBullet();
        }
    }

    private void OnTriggerEnter( Collider other )
    {
        EntityBase hit = other.GetComponent<EntityBase>();

        if ( !hit || hit.team != team )
        {
            if ( PhotonNetwork.isMasterClient && hit )
            {
                owner.Attack( damageValue, hit, damageType );
            }
            StorBullet();
        }
    }

    private void StorBullet()
    {
        GameManager.instance.bullets.Enqueue( this );
        rbCache.velocity = Vector3.zero;
        gameObject.SetActive( false );
    }
}