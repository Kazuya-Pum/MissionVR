using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBAEngine.Skills
{
    public class DroneObject : PlayerObject
    {
        [SerializeField]
        private float timer = 0.1f;
        [SerializeField]
        private float life = 1f;
        float startTime = 0;
        [SerializeField]
        private float radius = 10;

        [SerializeField]
        private int damage = 10;
        [SerializeField]
        private string bulletObjest;
        [SerializeField]
        float speed = 1;//弾速
        public float Speed { get { return speed; } }
        [SerializeField]
        float angle = 0;//仰角補正
        public float Angle { get { return angle; } }
        [SerializeField]
        AudioClip muzzleSound = null;//銃撃音
        public AudioClip MuzzleSound { get { return muzzleSound; } }
        [SerializeField]
        float destroyTime = 0;//限界寿命
        //[HideInInspector]
        //public GameObject player;


        //List<IPlayer> plist = new List<IPlayer>();
        List<LocalVariables> cList = new List<LocalVariables>();
        CapsuleCollider c;

        // Use this for initialization
        void Start()
        {
            if ((c = GetComponent<CapsuleCollider>()) == null)
                c = gameObject.AddComponent<CapsuleCollider>();
            c.radius = radius;
            startTime = Time.time;
            StartCoroutine(checkDamage());
        }

        // Update is called once per frame
        void Update()
        {

        }

        IEnumerator checkDamage()
        {
            LocalVariables charaPlayer = player.GetComponent<LocalVariables>();
            do
            {
                for(int i = 0;i < cList.Count; i++)
                {
                    charaPlayer.gameObject.GetComponent<Chara>().networkManager.photonView.RPC("SendSkillDamage", PhotonTargets.MasterClient, player.GetPhotonView().ownerId, cList[i].gameObject.GetPhotonView().ownerId, damage, cList[i].gameObject.transform.root.gameObject.GetPhotonView().viewID);
                }
                //foreach (Chara c in cList)
                //    p.Damage(10);
                yield return new WaitForSeconds(timer);
            } while ((Time.time - startTime) < life);
            if(PhotonNetwork.isMasterClient)
                PhotonNetwork.Destroy(gameObject);
        }

        void OnTriggerEnter(Collider other)
        {
            if (!photonView.isMine) return;
            if (other.tag != "Player") return;
            LocalVariables charaOther = other.GetComponent<LocalVariables>();
            LocalVariables charaPlayer = player.GetComponent<LocalVariables>();
            if (charaOther == null) return;
            if(charaOther.team != charaPlayer.team)
            {
                cList.Add(charaOther);
            }
            //IPlayer p = other.gameObject.GetComponent<IPlayer>();
            //if (p != null)
            //    plist.Add(p);
        }

        void OnTriggerExit(Collider other)
        {
            if (!photonView.isMine) return;
            if (other.tag != "Player") return;
            LocalVariables charaOther = other.GetComponent<LocalVariables>();
            if (charaOther == null) return;
            int i;
            for(i = 0; i < cList.Count; i++)
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

        public  int UseSkill(IPlayer p)
        {
            Transform muzzleTransform = p.GetPlayerTransform();
            GameObject b = (GameObject)PhotonNetwork.Instantiate(bulletObjest, muzzleTransform.position, muzzleTransform.rotation, 0);
            b.transform.Rotate(-b.transform.right, angle);
            ThrowObject t = b.GetComponent<ThrowObject>();
            //Debug.Log(t);
            t.Damage = damage;
            t.Speed = speed;
            t.DestoyTime = destroyTime;
            return 0;
        }
    }
}
