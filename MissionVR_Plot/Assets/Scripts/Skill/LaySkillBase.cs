using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBAEngine.Skills{
    [CreateAssetMenu(menuName = "skills/LaySkillBase")]
    public class LaySkillBase : SkillBase
    {
        [SerializeField]
        float range = 0;
        public float Range { get { return range; } }
        [SerializeField]
        int damage;//負の値で回復
        public int Damage { get { return damage; } }
        [SerializeField]
        string layPrehub;
        //[HideInInspector]
        //public GameObject player;
        [SerializeField]
        bool penetrate=true;//貫通弾
        [HideInInspector]
        public TeamColor teamColorSkillObject;

        public override int  UseSkill(IPlayer p,GameObject player)
        {
            base.UseSkill(p,player);
            Transform t = p.GetPlayerTransform();
            RaycastHit[] r = new RaycastHit[1];
            if (penetrate==true)
                r= Physics.RaycastAll(t.position, t.forward,range);
            else
                Physics.Raycast(t.position, t.forward, out r[0], range);
            LocalVariables enemy;
            player = p.GetPlayerTransform().gameObject;
            foreach (RaycastHit h in r)
            {
                enemy = h.collider.GetComponent<LocalVariables>();
                if (enemy != null && enemy.team != teamColorSkillObject && enemy != p.GetPlayerTransform().GetComponent<LocalVariables>())
                {
                    player.GetComponent<Chara>().networkManager.photonView.RPC("SendSkillDamage", PhotonTargets.MasterClient, player.GetPhotonView().ownerId, enemy.gameObject.GetPhotonView().ownerId, damage, enemy.gameObject.transform.root.gameObject.GetPhotonView().viewID);
                }
            }

            //ここからエフェクト描画用オブジェクト生成処理
            GameObject g = PhotonNetwork.Instantiate(layPrehub,player.transform.position,Quaternion.identity,0);
            LineRenderer l = g.GetComponent<LineRenderer>();
            if (l == null)
                l= g.AddComponent<LineRenderer>();
            if (g.GetComponent<LayObject>() == null)
                g.AddComponent<LayObject>();
            l.positionCount = 2;
            l.SetPosition(0, t.position);
            l.SetPosition(1, t.position + t.forward * range);
            //ここまでエフェクト描画用オブジェクト生成処理

           
            return 0;
        }
    }
}