using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MinionSpawnController : Photon.MonoBehaviour
{
    [SerializeField] private Transform spawnPoints;
    [SerializeField] private float spawnIntervalTime;
    private WaitForSeconds spawnInterval;

    private void Awake()
    {
        spawnInterval = new WaitForSeconds( spawnIntervalTime );
    }

    private void Start()
    {
        GameManager.instance.onGameStart += OnGameStart;
    }

    public void OnGameStart()
    {
        if ( PhotonNetwork.isMasterClient )
        {
            StartCoroutine( "MinionSpawning" );
        }
    }

    private IEnumerator MinionSpawning()
    {
        while ( true )
        {
            foreach ( Transform teamPoint in spawnPoints )
            {
                foreach ( Transform point in teamPoint )
                {
                    MinionAI minionAI = GameManager.instance.Summon( 0, point, (Team)System.Enum.Parse( typeof( Team ), teamPoint.name ) ).GetComponent<MinionAI>();
                }
            }
            yield return spawnInterval;
        }
    }
}
