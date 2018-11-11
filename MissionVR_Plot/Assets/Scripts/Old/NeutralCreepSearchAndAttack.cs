using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeutralCreepSearchAndAttack : MonoBehaviour {

    GameObject neutralCreep;
    Rigidbody thisRigidBody;
    public List<GameObject> attackTask = new List<GameObject>();
    bool isRunning;
    private GameObject target;
    [SerializeField]
    private float tracespeed = 0.01f;
    LocalVariables neutralCreepLocalVariables;
    public Vector3 neutralCreepSpawnPoint;
    [SerializeField]
    private float movableRange = 100f;
    private bool attackable = true;
    [SerializeField]
    private float aroundSpawnPoint = 0.5f;
    [SerializeField]
    private float attackRange = 100f;

    // Use this for initialization
    void Start () {
        neutralCreep = gameObject.transform.root.gameObject;
        thisRigidBody = neutralCreep.GetComponent<Rigidbody>();
        neutralCreepLocalVariables = neutralCreep.GetComponent<LocalVariables>();
        thisRigidBody.constraints = RigidbodyConstraints.FreezeAll;
    }

    // Update is called once per frame
    void Update () {
        //スポーン位置と自身の位置の間の距離
        float movingRange = (neutralCreepSpawnPoint - this.gameObject.transform.position).magnitude;
        //攻撃対象がいる場合
        if(attackTask.Count != 0 && attackable)
        {
            //攻撃対象の方向ベクトル取得
            Vector3 targetDirection = attackTask[0].transform.position - this.gameObject.transform.position;
            //攻撃対象の方へ向く
            transform.LookAt(new Vector3(targetDirection.normalized.x, targetDirection.normalized.y, this.gameObject.transform.position.z));
            //攻撃対象が攻撃可能範囲内にいる場合攻撃
            if (attackRange >= targetDirection.magnitude)
            {
                thisRigidBody.constraints = RigidbodyConstraints.FreezeAll;
                //攻撃可能状態ならば攻撃
                if (!isRunning && attackable)
                {
                    StartCoroutine(AttackCoroutine(attackTask[0]));
                    isRunning = !isRunning;
                }
            }
            else
            {
                thisRigidBody.constraints = RigidbodyConstraints.None;
                thisRigidBody.constraints = RigidbodyConstraints.FreezeRotationY;
                //攻撃対象のほうへ移動する
                thisRigidBody.velocity = (new Vector3(targetDirection.x, this.gameObject.transform.position.y, targetDirection.z).normalized) * tracespeed;
            }
        }
        //元の位置に戻る
        else if(attackTask.Count == 0)
        {
            thisRigidBody.velocity = (new Vector3(neutralCreepSpawnPoint.x, this.gameObject.transform.position.y, neutralCreepSpawnPoint.z).normalized) * tracespeed;
        }

//        Debug.Log(movingRange);
//        Debug.Log(movableRange);
        //スポーン位置と自身の位置までの距離が移動可能距離よりも大きい場合攻撃対象をなくして元の位置に戻る
        if (movingRange > movableRange)
        {
//            Debug.Log(movingRange);
//            Debug.Log(movableRange);
            attackTask.Clear();
            attackable = false;
        }
        //元の位置付近にいると攻撃可能
        else if (movingRange <= aroundSpawnPoint)
        {
            attackable = true;

        }
	}

    public void SetAttackTask(GameObject target)
    {
        attackTask.Add(target);
        if (attackTask.Count >= 2)
        {
            for(int n = 1; n < attackTask.Count; n++)
            {
                attackTask.RemoveAt(n);
            }
        }
    }

    IEnumerator AttackCoroutine(GameObject target)
    {
        LocalVariables targetLocalVariables = target.GetComponent<LocalVariables>();

        /*
         * ダメージ計算式(とりあえず簡単にしておきます
         * 与えるダメージは、{(自身の物理攻撃力 - 相手の物理防御力) + (自身の魔法攻撃力 - 相手の魔法防御力)}
         */
        int dmg = (int)((neutralCreepLocalVariables.PhysicalOffence - targetLocalVariables.PhysicalDefence)
            + (neutralCreepLocalVariables.MagicalOffence - targetLocalVariables.MagicalDefence));

        targetLocalVariables.Damage(dmg);
        Debug.Log(dmg);
        yield return new WaitForSeconds(3.0f / neutralCreepLocalVariables.AttackSpeed);

        isRunning = false;

        yield break;
    }
}
