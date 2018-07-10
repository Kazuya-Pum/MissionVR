using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MOBAEngine.Skills;
using System;

public class test : MonoBehaviour ,IPlayer{
    [SerializeField]
    SkillBase[] b;

    // Use this for initialization
    void Start ()
    {
        StartCoroutine(aaa());
    }
	
	// Update is called once per frame
	void Update ()
    {
    }

    IEnumerator aaa()
    {
        while (true)
        {
            foreach (SkillBase aa in b)
            {
                aa.UseSkill(this,gameObject);
                for(int i=0;i<3;i++)
                yield return null;
            }
        }
    }
    
    public void Damage(int d) { }
    public Transform GetPlayerTransform(){ return transform; }
}
