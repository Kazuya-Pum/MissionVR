using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Refactoring
{

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
            EntityBase hit = other.GetComponent<EntityBase>();

            if ( !hit || hit.team != team )
            {
                if ( PhotonNetwork.isMasterClient && hit )
                {
                    owner.Attack( damageValue, hit, damageType );
                }
                Destroy( gameObject );
            }
        }
    }
}