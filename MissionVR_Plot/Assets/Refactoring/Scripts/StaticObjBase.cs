using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Refactoring
{
    public class StaticObjBase : EntityBase
    {

        // TODO タワーやネクサス等の処理

        [PunRPC]
        protected override void Death()
        {

            
            base.Death();
        }
    }
}
