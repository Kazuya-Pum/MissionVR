using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinionMelee : MinionBase {

    public void test(MinionBase minion)
    {
        Attack( 5, minion );
    }

    protected override void Attack(int damageValue, MinionBase target)
    {
        base.Attack( damageValue, target );
    }
}
