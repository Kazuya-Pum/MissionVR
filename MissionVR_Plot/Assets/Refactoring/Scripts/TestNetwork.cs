using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestNetwork : Photon.MonoBehaviour
{
    [SerializeField] private GameObject[] minions;
    [SerializeField] public GunInfo[] gunList;

    void Awake()
    {
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
        PhotonNetwork.Instantiate( "Player", new Vector3( 0, 0, 0 ), Quaternion.Euler( Vector3.zero ), 0 );
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
