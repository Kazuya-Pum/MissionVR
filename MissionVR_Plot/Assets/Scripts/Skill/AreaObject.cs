using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBAEngine.Skills {
    public class AreaObject : Photon.MonoBehaviour {
        [HideInInspector]
        public float Timer=0.1f;
        [HideInInspector]
        public float Life = 1f;
        float startTime = 0;
        [HideInInspector]
        public float Radius = 10;
        [HideInInspector]
        public int Damage=10;

        List<IPlayer> plist = new List<IPlayer>();
        CapsuleCollider c;

        // Use this for initialization
        void Start() {
            if ((c = GetComponent<CapsuleCollider>()) == null)
                c = gameObject.AddComponent<CapsuleCollider>();
            c.radius = Radius;
            startTime = Time.time;
            StartCoroutine(checkDamage());
        }

        // Update is called once per frame
        void Update() {

        }

        IEnumerator checkDamage()
        {
            do
            {
                foreach (IPlayer p in plist)
                    p.Damage(10);
                yield return new WaitForSeconds(Timer);
            } while ((Time.time - startTime) < Life);
            if(PhotonNetwork.isMasterClient)
                PhotonNetwork.Destroy(gameObject);
        }
        
        void OnTriggerEnter(Collider other)
        {
            if (!photonView.isMine) return;
            IPlayer p = other.gameObject.GetComponent<IPlayer>();
            if (p != null)
                plist.Add(p);
        }

        void OnTriggerExit(Collider other)
        {
            if (!photonView.isMine) return;
            IPlayer cp = other.gameObject.GetComponent<IPlayer>();
            int i;
            for (i=0;i<plist.Count;i++) {
                if (cp == plist[i])
                    break;
            }
            if (plist.Count != i)
                plist.RemoveAt(i);
        }
    }
}
