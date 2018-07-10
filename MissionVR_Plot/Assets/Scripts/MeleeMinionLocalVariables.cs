using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//meleeミニオンの内部値管理スクリプト
public class MeleeMinionLocalVariables : LocalVariables {

    #region UniqueVariables

    [SerializeField]
    private int meleeMinionHp = 10;

    [SerializeField]
    private int meleeMinionMana = 0;

    [SerializeField]
    private int meleeMinionPhysicalOffence = 2;

    [SerializeField]
    private int meleeMinionPhysicalDiffence = 1;

    [SerializeField]
    private int meleeMinionMagicalOffence = 1;

    [SerializeField]
    private int meleeMinionMagicalDiffence = 1;

    [SerializeField]
    private int meleeMinionHpRecovery = 0;

    [SerializeField]
    private int meleeMinionManaRecovery = 0;

    [SerializeField]
    private float meleeMinionAttackSpeed = 1;

    [SerializeField]
    private float meleeMinionMoveSpeed = 1;

    private int killMinionMoney = 25;

    private int killMinionExp = 10;

    NetworkManager networkManager;
    #endregion

    protected void Start()
    {
        //meleeMinionの時のみ
        MaxHp = meleeMinionHp;
        Hp = meleeMinionHp;
        Mana = meleeMinionMana;
        PhysicalDefence = meleeMinionPhysicalDiffence;
        PhysicalOffence = meleeMinionPhysicalOffence;
        MagicalDefence = meleeMinionMagicalDiffence;
        MagicalOffence = meleeMinionMagicalOffence;
        AutomaticHpRecovery = meleeMinionHpRecovery;
        AutomaticManaRecovery = meleeMinionManaRecovery;
        AttackSpeed = meleeMinionAttackSpeed;
        MoveSpeed = meleeMinionMoveSpeed;
        networkManager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
    }

    [PunRPC]
    public void DestroyMinionObject(int viewID, int playerID)
    {
        if (PhotonNetwork.isMasterClient)
        {
            if (PhotonView.Find(viewID) == null)
                return;

            GameObject minion = PhotonView.Find(viewID).gameObject;

            if (minion.GetComponent<LocalVariables>().Hp <= 0)
            {
                networkManager.photonView.RPC("Send_Exp_Money", PhotonTargets.All, playerID, killMinionExp, killMinionMoney, viewID);
                //networkManager.photonView.RPC("SendMoney", PhotonTargets.All, playerID, killMinionMoney);
                //Destroy(this.gameObject);
            }
        }
    }

    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            //データの送信
            stream.SendNext(Hp);
        }
        else
        {
            //データの受信
            if(this.Hp > (int)stream.ReceiveNext())
            {
                this.Hp = (int)stream.ReceiveNext();
            }
        }
    }
}