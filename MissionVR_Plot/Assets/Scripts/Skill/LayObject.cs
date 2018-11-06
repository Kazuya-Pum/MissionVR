using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBAEngine.Skills
{
    public class LayObject : Photon.MonoBehaviour
    {
        // Use this for initialization
        void Start()
        {
            StartCoroutine(SelfDestroy(0.3f));
        }

        // Update is called once per frame
        void Update()
        {

        }

        IEnumerator SelfDestroy( float time)
        {
            yield return new WaitForSeconds(time);
            if(PhotonNetwork.isMasterClient)
                PhotonNetwork.Destroy(gameObject);
        }
    }
}
