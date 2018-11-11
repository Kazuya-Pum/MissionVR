using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBAEngine.Skills
{
    [CreateAssetMenu(menuName = "skills/ThrowSkillBase")]
    public class ThrowSkillBase : SkillBase
    {
        [SerializeField]
        string bulletPrehub=null;//弾オブジェクト
        [SerializeField]
        int damage=0;//ダメージ
        public int Damage { get { return damage; } } 
        [SerializeField]
        float speed=1;//弾速
        public float Speed { get { return speed; } }
        [SerializeField]
        float angle=0;//仰角補正
        public float Angle { get { return angle; } }
        [SerializeField]
        AudioClip muzzleSound=null;//銃撃音
        public AudioClip MuzzleSound { get { return muzzleSound; } }
        [SerializeField]
        float destroyTime=0;//限界寿命

        public override int UseSkill(IPlayer p,GameObject player)
        {
            base.UseSkill(p,player);
            Transform muzzleTransform = p.GetPlayerTransform();
            GameObject b = (GameObject)PhotonNetwork.Instantiate(bulletPrehub,muzzleTransform.position , muzzleTransform.rotation , 0);
            ThrowObject t = b.GetComponent<ThrowObject>();
            t.player = player;
            b.transform.Rotate(-b.transform.right,angle);
            t.Damage = damage;
            t.Speed = speed;
            t.DestoyTime = destroyTime;
            return 0;
        }
    }
}