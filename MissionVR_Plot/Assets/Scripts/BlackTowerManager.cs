using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Blackチームのタワーの判定処理管理スクリプト
public class BlackTowerManager :MonoBehaviour{

    private TowerManager towerManager;

    #region UniqueVariables
 //   [SerializeField]
    protected int towerHp = 500;

    protected int towerMana = 0;

    protected int towerPhysicalOffence = 10;

    protected int towerPhysicalDiffence = 1;

    protected int towerMagicalOffence = 10;

    protected int towerMagicalDiffence = 1;

    protected int towerHpRecovery = 0;

    protected int towerManaRecovery = 0;

    protected float towerAttackSpeed = 1;

    protected float towerMoveSpeed = 0;

    GameObject parent;

    LocalVariables towerLocalVariables;

    #endregion

    // Use this for initialization
    void Awake () {

        TowerNumberManager towerNumManager = GameObject.Find("TowerManager").GetComponent<TowerNumberManager>();

        parent = this.gameObject.transform.parent.gameObject;
        towerLocalVariables = parent.GetComponent<LocalVariables>();

        //localな変数の管理スクリプト上の変数に返す
        towerLocalVariables.Hp = towerNumManager.towerHP;
        towerLocalVariables.MaxHp = towerNumManager.towerHP;
        towerLocalVariables.Mana = towerMana;
        towerLocalVariables.PhysicalDefence = towerPhysicalDiffence;
        towerLocalVariables.PhysicalOffence = towerPhysicalOffence;
        towerLocalVariables.MagicalDefence = towerMagicalDiffence;
        towerLocalVariables.MagicalOffence = towerMagicalOffence;
        towerLocalVariables.AutomaticHpRecovery = towerHpRecovery;
        towerLocalVariables.AutomaticManaRecovery = towerManaRecovery;
        towerLocalVariables.AttackSpeed = towerAttackSpeed;
        towerLocalVariables.MoveSpeed = towerMoveSpeed;

        towerManager = this.gameObject.GetComponent<TowerManager>();

        //このタワーのチームの設定
        towerLocalVariables.team = TeamColor.Black;

        //オブジェクトの種類設定(必要ないかも)
        towerLocalVariables.objType = ObjectType.Tower;
		
	}

    private void OnTriggerEnter(Collider other)
    {

        if (!PhotonNetwork.isMasterClient) return;

        if (other.gameObject.tag == "Minion" || other.gameObject.tag == "Player")
            towerManager.Enter(other, towerLocalVariables.team);
    }

    private void OnTriggerExit(Collider other)
    {

        if (!PhotonNetwork.isMasterClient) return;

        if (other.gameObject.tag == "Minion" || other.gameObject.tag == "Player")
            towerManager.Exit(other, towerLocalVariables.team);
    }

    private void OnTriggerStay(Collider other)
    {

        if (!PhotonNetwork.isMasterClient) return;

        if (other.gameObject.tag == "Minion" || other.gameObject.tag == "Player")
            towerManager.Stay(other, towerLocalVariables.team);
    }
}
