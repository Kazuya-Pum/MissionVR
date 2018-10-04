using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BulletBase : Photon.MonoBehaviour
{
    #region 発射時に受け取る値
    public int ownerId;
    public int damageValue;
    public DamageType damageType;
    public float range;
    #endregion

    /// <summary>
    /// 銃弾のインスタンスが消えるまでの時間
    /// </summary>
    [SerializeField] private float ttl;
    [SerializeField] private float speed;

    private Vector3 prev;
    private Transform tfCache;

    private void Awake()
    {
        ttl = ( ttl <= 0 ) ? 5 : ttl;
        range = ( range <= 0 ) ? 50 : range;

        tfCache = transform;
    }

    private void Start()
    {
        prev = tfCache.position;
        GetComponent<Rigidbody>().AddForce( transform.forward * 10 * speed, ForceMode.Impulse );
    }

    private void Update()
    {
        ttl -= Time.deltaTime;

        if ( Vector3.Distance( prev, tfCache.position ) >= range || ttl <= 0 )
        {
            Destroy( gameObject );
        }
    }

    private void OnTriggerEnter( Collider other )
    {
        // TODO 味方に当たった場合は消えずにすり抜けさせる
        if ( ownerId > 0 && other.GetComponent<EntityBase>() )
        {
            PhotonView.Find( ownerId ).photonView.RPC( "Attack", PhotonTargets.MasterClient, damageValue, other.GetComponent<EntityBase>(), damageType, ownerId );
        }

        Destroy( gameObject );
    }
}
