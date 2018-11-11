using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName ="Items/ItemBase")]
public class ItemBase : ScriptableObject {
    [SerializeField]
    string m_name;
}
