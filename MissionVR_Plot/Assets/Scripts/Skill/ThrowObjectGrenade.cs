using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBAEngine.Skills
{
    public class ThrowObjectGrenade : ThrowObject
    {
        //[HideInInspector]
        //public int Damage = 0;
        //[HideInInspector]
        //public float Speed = 0;
        //[HideInInspector]
        //public bool TrackingFlag = false;//ONにして追尾弾化
        //[HideInInspector]
        //public GameObject TrackTarget = null;//追尾ターゲット
        //[HideInInspector]
        //public string BombEffect = null;//着弾時発生オブジェクト
        ////[HideInInspector]
        ////public GameObject player;
        //[HideInInspector]
        //public float DestoyTime = 0;//時間経過による自壊までのタイムリミット
        [HideInInspector]
        public float BombTime = 0;//手榴弾の爆発までの時間

        List<LocalVariables> cList = new List<LocalVariables>();
        //List<IPlayer> plist = new List<IPlayer>();
        CapsuleCollider c;
        float startTime;
        // Use this for initialization
        void Start()
        {
            GetComponent<Rigidbody>().velocity = transform.forward * Speed;
            startTime = Time.time;
            if ((c = GetComponent<CapsuleCollider>()) == null)
                c = gameObject.AddComponent<CapsuleCollider>();
            //c.radius = radius;
            startTime = Time.time;
            //StartCoroutine(checkDamage());
        }

        // Update is called once per frame
        void Update()
        {
            if (!photonView.isMine) return;
            if (TrackingFlag)//追尾処理
                GetComponent<Rigidbody>().velocity = (TrackTarget.transform.position - transform.position).normalized * Speed;
            if ((Time.time - startTime) > DestoyTime && PhotonNetwork.isMasterClient)
                PhotonNetwork.Destroy(gameObject);
            if ((Time.time - startTime) > BombTime)
            {
                Chara charaPlayer = player.GetComponent<Chara>();
                for(int i = 0; i < cList.Count; i++)
                {
                    charaPlayer.networkManager.photonView.RPC("SendSkillDamage", PhotonTargets.MasterClient, player.gameObject.GetPhotonView().ownerId, cList[i].gameObject.GetPhotonView().ownerId, Damage, cList[i].gameObject.transform.root.gameObject.GetPhotonView().viewID);
                }
            }
                //PhotonNetwork.Destroy(gameObject);
        }

        //virtual public void OnTriggerEnter(Collider c)
        //{
        //    IPlayer p;
        //    if ((p = c.gameObject.GetComponent<IPlayer>()) != null)//プレイヤーに着弾時ダメージ
        //    {
        //        p.Damage(Hit(Damage));
        //    }
        //    if (BombEffect != null)
        //    {
        //        PhotonNetwork.Instantiate(BombEffect, transform.position, Quaternion.identity,0);
        //    }

        //    PhotonNetwork.Destroy(gameObject);//着弾後自壊
        //}

        private int Hit(int damage)
        {
            return damage;
        }

        void OnTriggerEnter(Collider other)
        {
            if (!photonView.isMine) return;
            if (other.tag != "Player") return;
            LocalVariables charaOther = other.GetComponent<LocalVariables>();
            //IPlayer p = other.gameObject.GetComponent<IPlayer>();
            //if (p != null)
            //    plist.Add(p);
            if(charaOther != null && charaOther.team != player.GetComponent<LocalVariables>().team)
            {
                cList.Add(charaOther);
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (!photonView.isMine) return;
            if (other.tag != "Player") return;
            LocalVariables charaOther = other.GetComponent<LocalVariables>();
            int i;
            for(i = 0;i < cList.Count; i++)
            {
                if (charaOther == cList[i]) break;
            }
            if (cList.Count != i) cList.RemoveAt(i);
            //IPlayer cp = other.gameObject.GetComponent<IPlayer>();
            //int i;
            //for (i = 0; i < plist.Count; i++)
            //{
            //    if (cp == plist[i])
            //        break;
            //}
            //if (plist.Count != i)
            //    plist.RemoveAt(i);
        }
    }
}
