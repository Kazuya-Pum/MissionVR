using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinionMelee : MinionBase {

    public void test(MinionBase minion)
    {
        Attack( 5, minion );
    }

    protected override void Attack( int damageValue, EntityBase target, DamageType damageType = DamageType.PHYSICAL, int id = 0 )
    {
        base.Attack( damageValue, target, damageType, id );
    }
}
