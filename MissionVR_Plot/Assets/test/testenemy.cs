using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MOBAEngine.Skills;

public class testenemy : MonoBehaviour,IPlayer {

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    
    public void Damage(int d)
    {
        Debug.Log("Damage" + d);
    }
    
    public Transform GetPlayerTransform() { return transform; }
}
