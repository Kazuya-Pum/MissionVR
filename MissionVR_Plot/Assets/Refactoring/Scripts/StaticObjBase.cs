using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Refactoring
{
    public class StaticObjBase : EntityBase
    {

        // TODO タワーやネクサス等の処理

        protected override void Death()
        {
            GameManager.instance.photonView.RPC( "SetAnounce", PhotonTargets.All, AnounceType.DESTROY, team );
            base.Death();
        }
    }
}
