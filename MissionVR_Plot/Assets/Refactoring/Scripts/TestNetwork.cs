using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestNetwork : Photon.MonoBehaviour
{
    [SerializeField] private GameObject[] minions;

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


    public void Summon()
    {
        if ( PhotonNetwork.isMasterClient )
        {
            PhotonNetwork.Instantiate( "Minion", transform.position, Quaternion.identity, 0 );
        }
    }

}
