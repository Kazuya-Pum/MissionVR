using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MOBAEngine.Skills
{
    [CreateAssetMenu]
    public class TokenSkillBase : SkillBase  {

        [SerializeField]
        string summonPrehub;

        public override int UseSkill(IPlayer p,GameObject player)
        {
            base.UseSkill(p,player);
            Transform muzzleTransform = p.GetPlayerTransform();
            GameObject b = (GameObject)PhotonNetwork.Instantiate(summonPrehub,muzzleTransform.position , muzzleTransform.rotation , 0);
            b.GetComponent<PlayerObject>().player = player;
            return 0;
        }
    }
}