using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBAEngine.Skills
{
    [CreateAssetMenu]
    public class AreaSkillBase : SkillBase
    {
        [SerializeField]
        float timer=0;
        public float Timer { get { return timer; } }//ダメージ間隔
        [SerializeField]
        float life = 1;
        public float Life { get { return life; } }//持続時間
        [SerializeField]
        float radius=0;
        public float Radius { get { return radius; } }//半径
        [SerializeField]
        int damage=0;
        public int Damage { get { return damage; } }
        [SerializeField]
        string areaPrehub;

        public override int UseSkill(IPlayer p,GameObject player)
        {
            base.UseSkill(p,player);
            Transform playerTransform = p.GetPlayerTransform();
            GameObject b = PhotonNetwork.Instantiate(areaPrehub,playerTransform.position,playerTransform.rotation,0);
            b.GetComponent<PlayerObject>().player = player;
            AreaObject a = b.GetComponent<AreaObject>();
            if (a == null)
                a = b.AddComponent<AreaObject>();
            a.Damage = damage;
            a.Timer = Timer;
            a.Radius = radius;
            a.Life = life;
            return 0;
        }
    }
}