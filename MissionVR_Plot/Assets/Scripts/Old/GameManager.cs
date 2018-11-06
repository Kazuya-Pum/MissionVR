using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlaySide
{
    Black , White
}

public class GameManager : MonoBehaviour {


    [SerializeField] private GameObject[] enemies;/*敵オブジェクトを入れる*/

    private Vector3 spawnPos_Enemy;/*敵の生成位置*/


	// Use this for initialization
	void Start () {
        

	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public IEnumerator SpawnMinions(int spawnCount , float interval)/*ミニオンを生成する関数*/
    {
        

        for (int a = 0;a < spawnCount; a++)
        {
            /*生成位置の空オブジェクトを探す*/
            GameObject[] pos = GameObject.FindGameObjectsWithTag("EnemySpawnPos");
            /*生成位置はランダム*/
            spawnPos_Enemy = pos[Random.Range(0, pos.Length)].transform.position;

            GameObject enemy = (GameObject) Instantiate(enemies[Random.Range(0, enemies.Length)], spawnPos_Enemy, Quaternion.identity);
            Vector3 enemyPosition = enemy.transform.position;
            enemyPosition.y = 1.5f;
            enemy.transform.position = enemyPosition;
            yield return new WaitForSeconds(interval);
        }

    }
}
