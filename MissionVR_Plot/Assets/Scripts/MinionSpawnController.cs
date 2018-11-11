using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MinionSpawnController : Photon.MonoBehaviour
{
    [SerializeField] private Transform spawnPoints;
    [SerializeField] private float spawnIntervalTime;
    [SerializeField] private float waveIntervalTime;
    [SerializeField] private int waveSpawnLimit;
    private WaitForSeconds spawnInterval;
    private WaitForSeconds waveInterval;

    private void Awake()
    {
        spawnInterval = new WaitForSeconds( spawnIntervalTime );
        waveInterval = new WaitForSeconds( waveIntervalTime );
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
        int spawnCount = 0;
        while ( true )
        {
            foreach ( Transform teamPoint in spawnPoints )
            {
                foreach ( Transform point in teamPoint )
                {
                    MinionAI minionAI = GameManager.instance.Summon( 0, point, (Team)System.Enum.Parse( typeof( Team ), teamPoint.name ) ).GetComponent<MinionAI>();
                }
            }

            spawnCount++;
            if ( spawnCount < waveSpawnLimit )
            {
                yield return spawnInterval;
            }
            else
            {
                spawnCount = 0;
                yield return waveInterval;
            }
        }
    }
}
