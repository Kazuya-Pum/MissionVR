using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestNetwork : Photon.MonoBehaviour {

	void Awake () {
        PhotonNetwork.offlineMode = true;
        PhotonNetwork.CreateRoom( "Test" );
    }
	
}
