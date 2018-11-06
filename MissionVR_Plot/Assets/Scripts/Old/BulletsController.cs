using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletsController : MonoBehaviour
{
    [HideInInspector]
    public TeamColor teamColor;
    [HideInInspector]
    public GameObject originPlayer;
    [HideInInspector]
    public NetworkManager networkManager;
    [HideInInspector]
    public int ownerID;
    [HideInInspector]
    public int playerID;

    private float range = 100;/*射程距離*/
    private int attack = 5;/*攻撃力*/

    private LocalVariables targetLocalVariables;
    private int destroyTowerMoney;//タワーを破壊したときに入るmoney
    private int destroyTowerExp;//タワーを破壊したときに入るexp
    private Vector3 pos;
    private float dist;

    public bool IsMine
    {
        get;
        private set;
    }

    public void Initialize( bool i_isMine)
    {
        IsMine = i_isMine;
    }

    private void Start()
    {
        networkManager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
        pos = transform.position;
    }

    //Update is called once per frame
    void Update()
    {
        Vector3 newPos = transform.position;
        dist += Vector3.Magnitude(newPos - pos);
        pos = transform.position;
        if (dist > range)
        {
            Destroy(this.gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "StageObject")
        {
            Destroy(this.gameObject);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "StageObject")
        {
            Destroy(this.gameObject);
        }

        if (!IsMine) return;

        if (other.gameObject.tag == "Player")
        {
            if (other.gameObject.GetPhotonView().ownerId % 2 == 1 && teamColor == TeamColor.White)
                return;
            if (other.gameObject.GetPhotonView().ownerId % 2 == 0 && teamColor == TeamColor.Black)
                return;

            int otherPlayerID = other.gameObject.GetPhotonView().ownerId;
            int otherPhotonViewID = other.gameObject.GetPhotonView().viewID;

            networkManager.photonView.RPC("SendDamage", PhotonTargets.MasterClient, originPlayer.GetPhotonView().ownerId, otherPlayerID, otherPhotonViewID);

            Destroy(this.gameObject);
        }
        else if (other.gameObject.tag == "MinionBodyCollider")
        {
            if (other.transform.parent.GetComponent<LocalVariables>().team == teamColor)
                return;

            targetLocalVariables = other.transform.root.GetComponent<LocalVariables>();
            targetLocalVariables.DamageMinion(attack, (int)teamColor);

            targetLocalVariables.photonView.RPC("DestroyMinionObject", PhotonTargets.MasterClient, other.transform.root.gameObject.GetPhotonView().viewID, ownerID);

            Destroy(this.gameObject);
        }
        else if (other.gameObject.tag == "Tower")
        {
            targetLocalVariables = other.gameObject.GetComponent<LocalVariables>();
            if (targetLocalVariables.team == teamColor)
            {
                Destroy(this.gameObject);
                return;
            }

            targetLocalVariables.photonView.RPC("Damage", PhotonTargets.MasterClient, attack);
            targetLocalVariables.photonView.RPC("DestroyTowerObject", PhotonTargets.MasterClient, other.gameObject.GetPhotonView().viewID, ownerID);

            Destroy(this.gameObject);
        }
        else if (other.gameObject.tag == "Projector")
        {
            targetLocalVariables = other.gameObject.GetComponent<LocalVariables>();
            if (targetLocalVariables.team == teamColor)
                return;

            targetLocalVariables.photonView.RPC("Damage", PhotonTargets.MasterClient, attack);
            Destroy(this.gameObject);
        }
    }
}