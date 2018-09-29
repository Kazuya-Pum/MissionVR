using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinionBase : EntityBase
{
    private void Update()
    {
        Move( 0, 0 );
    }

    private void Move(float x, float z)
    {
        tfCache.Translate( x, 0, z, Space.World );

        Vector3 diff = tfCache.position - prev;

        if(diff.magnitude > 0.01 )
        {
            tfCache.rotation = Quaternion.LookRotation( diff );
        }

        prev = tfCache.position;
    }
}
