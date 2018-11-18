﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SearchArea : Photon.MonoBehaviour
{
    private AIBase aiBase;

    private void Awake()
    {
        aiBase = transform.parent.GetComponent<AIBase>();
    }

    private void OnTriggerEnter( Collider other )
    {
        EntityBase entity = other.GetComponent<EntityBase>();
        if ( entity )
        {
            aiBase.OnCheck( entity );
        }
    }

    //private void OnTriggerStay( Collider other )
    //{
    //    EntityBase entity = other.GetComponent<EntityBase>();
    //    if ( entity )
    //    {
    //        aiBase.OnCheck( entity );
    //    }
    //}

    private void OnTriggerExit( Collider other )
    {
        EntityBase entity = other.GetComponent<EntityBase>();

        if ( entity )
        {
            aiBase.OnLost( entity );
        }
    }
}
