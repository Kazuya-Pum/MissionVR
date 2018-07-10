using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBAEngine.Skills{
    public class ThrowObject : Photon.MonoBehaviour {
        [HideInInspector]
        public int Damage=0;
        [HideInInspector]
        public float Speed=0;
        [HideInInspector]
        public bool TrackingFlag=false;//ONにして追尾弾化
        [HideInInspector]
        public GameObject TrackTarget=null;//追尾ターゲット
        [HideInInspector]
        public GameObject player;
        [HideInInspector]
        public string BombEffect=null;//着弾時発生オブジェクト
        [HideInInspector]
        public float DestoyTime=0;//時間経過による自壊までのタイムリミット

        float startime;
        // Use this for initialization
        void Start() {
            GetComponent<Rigidbody>().velocity = transform.forward * Speed;
            startime = Time.time;
        }

        // Update is called once per frame
        void Update() {
            if (!photonView.isMine) return;
            if(TrackingFlag)//追尾処理
                GetComponent<Rigidbody>().velocity = (TrackTarget.transform.position - transform.position).normalized * Speed;
            if ((Time.time - startime) > DestoyTime)
                PhotonNetwork.Destroy(gameObject);
        }

        //virtual public void OnTriggerEnter(Collider c)
        //{
        //    if (c.tag != "Player") return;
        //    Chara charaPlayer = player.GetComponent<Chara>();
        //    Chara charaOther = c.GetComponent<Chara>();
        //    if(charaOther != null && charaOther.teamColorPlayer != charaPlayer.teamColorPlayer)
        //    {
        //        charaPlayer.networkManager.photonView.RPC("SendSkillDamage", PhotonTargets.MasterClient, player.GetPhotonView().ownerId, c.gameObject.GetPhotonView().ownerId, Damage);
        //    }
        //    //IPlayer p;
        //    //if ((p = c.gameObject.GetComponent<IPlayer>()) != null)//プレイヤーに着弾時ダメージ
        //    //{
        //    //    p.Damage(Hit(Damage));
        //    //}
        //    if (BombEffect != null)
        //    {
        //        PhotonNetwork.Instantiate(BombEffect, transform.position,Quaternion.identity,0);
        //    }

        //    PhotonNetwork.Destroy(gameObject);//着弾後自壊
        //}

        private int Hit(int damage)
        {
            return damage;
        }
    }
}