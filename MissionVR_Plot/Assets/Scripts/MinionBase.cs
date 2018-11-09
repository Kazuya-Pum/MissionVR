using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinionBase : MobBase
{
    protected override void Awake()
    {
        base.Awake();
        entityType = EntityType.MINION;
    }
}