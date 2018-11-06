using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//チームの色
public enum TeamColor : byte { White = 0, Black, }

//レーンの種類
public enum Lane : byte { Top = 0, Mid, Bot }

//オブジェクトの種類(タワーやチャンピオンなど(一応))
public enum ObjectType : byte { Champion = 0, Minion, Tower, Projector, }

//ローカルな変数を管理するスクリプト
//それぞれの破壊可能オブジェクトに継承して利用する
public class LocalVariables : Photon.MonoBehaviour {

    public TeamColor team;

    public Lane lane;

    public ObjectType objType;

    //体力
    public int Hp;

    public int MaxHp;

    //マナ
    public int Mana;

    public int ManaMax;

    //体力自動回復
    public int AutomaticHpRecovery;

    //マナ自動回復
    public int AutomaticManaRecovery;

    //移動速度
    public float MoveSpeed;

    //攻撃速度
    public float AttackSpeed;

    //物理防御力
    public float PhysicalDefence;

    //物理攻撃力
    public float PhysicalOffence;

    //魔法防御力
    public float MagicalDefence;

    //魔法攻撃力
    public float MagicalOffence;

    public float Exp;

    public int Level;

    public int money;

    Chara playerChara;

    public Text levelText;

    [SerializeField]
    private Image levelFillImage;

    [SerializeField]
    public Text MoneyText;

    private NetworkManager nM;

    private int killTowerExp = 200;

    private int killTowerMoney = 500;

    [PunRPC]
    public void Damage(int dmg)
    {
        Hp -= dmg;
    }

    [PunRPC]//ミニオンの場合はこっちでダメージを与える
    public void DamageMinion(int dmg, int playerColor)
    {
        if (playerColor == (int)team)
            return;
        Hp -= dmg;
    }

    //ミニオン→NetworkManager→LocalVariablesと呼ばれる
    [PunRPC]
    public void DestroyMinion(int destroyViewID)
    {
        if(photonView.isMine)
        {
            Destroy(PhotonView.Find(destroyViewID).gameObject);
        }
    }

    [PunRPC]
    public void RecieveExp(int recievedExp)
    {
        if (photonView.isMine) {

            Exp += recievedExp;
        }
    }

    [PunRPC]
    public void Recieve_Money_NPCExp( int recievedPlayerOwnerID, int recievedExp, int recievedMoney, int destroyID)
    {
        if ( photonView.isMine)
        {
            GameObject destroyObject = PhotonView.Find(destroyID).gameObject;
            Debug.Log(this.gameObject);

            if ( destroyObject.tag == "Minion")
            {
                Debug.Log(this.gameObject);
                if ((photonView.ownerId + recievedPlayerOwnerID) % 2 == 0)
                {
                    Exp += recievedExp;
                    if (levelFillImage != null)
                        levelFillImage.fillAmount = Exp / (Level * 100);
                }

                if (photonView.ownerId == recievedPlayerOwnerID)
                {
                    money += recievedMoney;
                    MoneyText.text = "" + money;
                }

                Destroy(PhotonView.Find(destroyID).gameObject);
            }
            else if (destroyObject.tag == "Tower")
            {
                Debug.Log(this.gameObject);
                //owneridが奇数ならWhite(int値では0)、偶数ならBlack(int値では1)
                if ((photonView.ownerId + recievedPlayerOwnerID) % 2 == 0)
                {
                    Exp += recievedExp;
                    if (levelFillImage != null)
                        levelFillImage.fillAmount = Exp / (Level * 100);
                }

                if (photonView.ownerId == recievedPlayerOwnerID)
                {
                    money += recievedMoney;
                    MoneyText.text = "" + money;
                }

                if (PhotonNetwork.isMasterClient)
                {
                    PhotonNetwork.Destroy( PhotonView.Find(destroyID).gameObject);
                }
            }
        }
    }

    [PunRPC]
    public void RecieveMoney( int recievedPlayerID, int recievedMoney)
    {
        if ( this.gameObject.GetPhotonView().isMine)
        {
            if ( this.gameObject.GetPhotonView().ownerId == recievedPlayerID)
            {
                money += recievedMoney;
                MoneyText.text = "" + money;
            }
        }
    }

    [PunRPC]
    public void DestroyTowerObject(int targetViewID, int playerOwnerID)
    {
        if (PhotonNetwork.isMasterClient)
        {
            if (PhotonView.Find(targetViewID) == null)
                return;

            GameObject tower = PhotonView.Find(targetViewID).gameObject;

            if (tower.GetComponent<LocalVariables>().Hp <= 0)
            {
                nM.photonView.RPC("Send_Exp_Money", PhotonTargets.All, playerOwnerID, killTowerExp, killTowerMoney, targetViewID);
            }
        }
    }

    private void Start()
    {
        nM = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
        if ( this.gameObject.GetComponent<Chara>())
        {
            if ( photonView.isMine)
            {
                playerChara = this.gameObject.GetComponent<Chara>();
                if ( levelFillImage != null)
                    levelFillImage.fillAmount = 0;
            }
        }
    }

    private void Update()
    {
        if ( photonView.isMine)
        {
            if ( Level == 0)return;
            if ( Exp >= 100 * Level)
            {
                GameObject.Find("NetworkManager").GetComponent<NetworkManager>().CallIfPlayerLevelUpped();
                Exp -= (100 * Level);
                playerChara.LevelUpMethod();
                if ( levelFillImage != null)
                    levelFillImage.fillAmount = 0;
            }
        }
    }
}
