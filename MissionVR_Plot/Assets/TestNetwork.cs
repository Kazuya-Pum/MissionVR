using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestNetwork : MonoBehaviour {

	// Use this for initialization
	void Awake () {
        PhotonNetwork.offlineMode = true;
        PhotonNetwork.CreateRoom( "Test" );
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
