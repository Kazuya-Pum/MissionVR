using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinionMelee : MinionBase
{


    protected override void Attack( int damageValue, EntityBase target, DamageType damageType = DamageType.PHYSICAL, int id = 0 )
    {
        base.Attack( damageValue, target, damageType, id );
    }

    public void test( EntityBase target )
    {
        Attack( 5, target );
    }


}
