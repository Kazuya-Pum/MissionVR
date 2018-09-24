using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBase : EntityBase,IPunObservable
{
    [SerializeField] protected int myExp;
    [SerializeField] protected int myMoney;
    private int level;

    public void OnPhotonSerializeView( PhotonStream stream, PhotonMessageInfo info )
    {
        throw new System.NotImplementedException();
    }

    [PunRPC]
    protected void GetReward( int exp = 0, int money = 0 )
    {
        myExp += exp;
        if ( myExp >= level * 100 )
        {
            photonView.RPC( "LevelUp", PhotonTargets.MasterClient );
        }

        myMoney += money;
    }

    [PunRPC]
    protected void LevelUp()
    {
        myExp -= level * 100;
        level++;

        if ( myExp >= level * 100 )
        {
            LevelUp();
        }
    }
}
