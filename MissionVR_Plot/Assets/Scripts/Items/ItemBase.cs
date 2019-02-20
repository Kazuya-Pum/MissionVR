using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName ="Items/ItemBase")]
public class ItemBase : ScriptableObject {
    //scriptableobjectなのでゲーム中に変数を変更することを考慮していない、変更する場合はコピー処理を挟むこと
    public enum ItemType {ACTIVE,NONACTIVE };
    [SerializeField]
    ItemType m_type;
    public ItemType Type { get { return m_type; } }
    [SerializeField]
    string m_name;
    public string Name { get { return m_name; } }
    [SerializeField]
    int m_cost;
    public int Cost { get { return m_cost; } }
    [SerializeField]
    int m_sellcost;
    public int SellCost { get { return m_sellcost; } }
    [SerializeField]
    Sprite m_icon;
    public Sprite Icon { get { return m_icon; } }
    [SerializeField]
    List<ItemBase> m_baseitem;//コスト軽減に必要なアイテム
    public List<ItemBase> BaseItem { get { return m_baseitem; } }
    [SerializeField]
    List<EffectTable> m_effect;
    public List<EffectTable> Effect { get { return m_effect; } }

    [System.Serializable]
    public class EffectTable
    {
        public enum EffectType { HP, MANA, PATK, PDEF, MATK, MDEF , SPEED }
        [SerializeField]
        EffectType m_key;
        public EffectType Key { get { return m_key; } }
        [SerializeField]
        int m_value;
        public int Value { get { return m_value; } }
    }
}