using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GetPlayerUIs : MonoBehaviour {

    LocalVariables playerLocalVariables;

	// Use this for initialization
	void Awake () {
        playerLocalVariables = this.gameObject.transform.root.gameObject.GetComponent<LocalVariables>();
        playerLocalVariables.levelText = this.gameObject.GetComponent<Text>();
    }

    // Update is called once per frame
    void Update () {
		
	}
}
