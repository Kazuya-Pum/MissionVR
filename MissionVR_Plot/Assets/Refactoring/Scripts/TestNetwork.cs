using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestNetwork : Photon.MonoBehaviour
{

    public static TestNetwork instance;

    [SerializeField] private DataBaseFormat dataBase;

    public Transform[] spawnPoint;

    public DataBaseFormat DataBase
    {
        get
        {
            return dataBase;
        }
    }

    void Awake()
    {
        if ( instance == null )
        {
            instance = this;
        }
        else
        {
            Destroy( gameObject );
        }

        //PhotonNetwork.offlineMode = true;

        PhotonNetwork.ConnectUsingSettings( "v1.0" );
    }

    void OnJoinedLobby()
    {
        Debug.Log( "joined lobby" );
        PhotonNetwork.JoinOrCreateRoom( "Test", null, null );
    }

    void OnJoinedRoom()
    {
        Debug.Log( "joined room : " + PhotonNetwork.room.Name + ", isMasterClient : " + PhotonNetwork.isMasterClient );


        InstantiatePlayer( ( PhotonNetwork.room.PlayerCount % 2 == 0 ) ? Team.WHITE : Team.BLACK );
    }

    PlayerBase player;
    void InstantiatePlayer( Team team )
    {
        player = PhotonNetwork.Instantiate( "Player", spawnPoint[(int)team].position, spawnPoint[(int)team].rotation, 0 ).GetComponent<PlayerBase>();
        player.photonView.RPC( "FetchTeam", PhotonTargets.AllBuffered, team );
        PlayerController.instance.player = player;
        player.photonView.RPC( "FetchSetting", PhotonTargets.AllBuffered, PlayerController.instance.sensitivity );
    }

    [PunRPC]
    public void Summon()
    {
        PhotonNetwork.InstantiateSceneObject( "Minion", Vector3.zero, Quaternion.identity, 0, null );
    }


    public void testSummon()
    {
        photonView.RPC( "Summon", PhotonTargets.MasterClient );
    }
}

[System.Serializable]
public class DataBaseFormat
{
    public Color allyColor;
    public Color enemyColor;

    public GunInfo[] gunInfos;
}

[System.Serializable]
public class GunInfo
{
    public string name;
    public GameObject bullet;
    public float fireRate;
    public float range;
    //public GameObject model;
}