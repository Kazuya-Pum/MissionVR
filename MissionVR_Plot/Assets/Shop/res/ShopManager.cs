using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

//わからないところのコメント追記しました
//ボタンの位置調整を行うためのコード追記しました
//moneyをcharaスクリプトではなく、localVariablesスクリプトで扱うように変更しました

public class ShopManager : MonoBehaviour
{
    public Chara chara;
    public LocalVariables p_localVariables;

    [SerializeField] private GameObject shopButton;
    [SerializeField] private Text textMoney;
    [SerializeField] private int s_money;
    public ShopItem[] shopItem;

    private Button firstButton;
    private int zeroIDIndex;



    private void Start()
    {
        // 再生時にショップを開いていれば閉じる
        if (SceneManager.GetSceneByName("ShopScene").isLoaded)
        {
            SceneManager.UnloadSceneAsync("ShopScene");
        }
        chara = GameObject.Find("CapsulePlayer").GetComponent<Chara>();
        chara.shopManager = this;
        //p_localVariables = chara.playerLocalVariables;
        //s_money = 1000;
    }

    void Update()
    {
        GetKey();
    }

    private void GetKey()
    {
        // 暫定で"C"キーで、現在のシーンを維持したまま、バックグラウンドでShopSceneを開く
        if (Input.GetKeyDown(KeyCode.C) || Input.GetButtonDown("Square"))
        {
            if (SceneManager.GetSceneByName("ShopScene").isLoaded == false)
            {
                StartCoroutine(ShowShop());
            }
            else
            {
                SceneManager.UnloadSceneAsync("ShopScene");
            }
        }
    }

    // ロードが完了してから処理
    private IEnumerator ShowShop()
    {
        yield return SceneManager.LoadSceneAsync("ShopScene", LoadSceneMode.Additive);
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("ShopScene"));

        textMoney = GameObject.Find("TextMoney").GetComponent<Text>();

        if(p_localVariables != null)
        {
            s_money = p_localVariables.money;      // お金を取得
            //s_money = 1000;
            textMoney.text = "残金：" + s_money;
        }

        firstButton = GameObject.FindGameObjectWithTag("firstButton").GetComponent<Button>();
        firstButton.Select();

        ChackCanBuy();
        yield break;
    }

    /// <summary>
    /// 購入処理
    /// 購入した際は、アイテムの空き欄のうち最も小さいインデックスに入る
    /// </summary>
    /// <param name="index">アイテムの番号</param>
    public void Buy(int index)
    {
        
        
        //アイテム枠に空きがないとき
        if(zeroIDIndex == -1)
        {
            //買いたいもののIDをリストから探す
            for(int i = 0; i < chara.havingItemID.Length; i++)
            {
                //見つけたら個数追加
                if(chara.havingItemID[i] == shopItem[i].itemID )
                {
                    shopItem[index].itemBought++;
                    s_money -= shopItem[index].itemPrice;
                }
            }
        }
        else
        {
            //アイテム欄の空きのうち、もっとも小さいものを探す
            for(int i = 0; i < chara.havingItemID.Length; i++)
            {
                if (chara.havingItemID[i] == 0)
                {
                    shopItem[index].itemBought++;
                    s_money -= shopItem[index].itemPrice;
                    chara.havingItemID[i] = shopItem[i].itemID;

                }
            }
        }



        //お金を同期
        try
        {
            p_localVariables.money = s_money;
        }
        catch
        {
            Debug.LogError("お金を同期できません。p_localVariablesがnullの可能性があります。");
        }

        //if(p_localVariables != null)
        //{
        //    p_localVariables.money = s_money;      // お金を同期
        //}

        //Debug.Log("残金:" + s_money);
        textMoney.text = "残金：" + s_money;
        ChackCanBuy(index);
    }

    /// <summary>
    /// 購入可能かどうか
    /// </summary>
    /// <param name="n">選択していたボタンが購入不可になった場合に選択をずらす</param>
    public void ChackCanBuy(int n = -1)
    {
        zeroIDIndex = -1;//アイテムIDが０、つまりアイテム欄に空きがあるとき、そのアイテム欄のインデックスを記録する
        //Debug.Log(chara);
        //アイテム欄の空きを探査
        for (int index = 0; index < chara.havingItemID.Length; index++)
            if (chara.havingItemID[index] == 0)
            {
                //空きがあればそのインデックスを記録
                zeroIDIndex = index;
                break;
            }


        if (zeroIDIndex != -1)//アイテム欄に空きがあるとき
        {
            for (int i = 0; i < shopItem.Length; i++)
            {
                //お金がなかったらfalse
                if (s_money >= shopItem[i].itemPrice)
                {
                    shopItem[i].itemCanBuy = true;
                }
                else
                {
                    shopItem[i].itemCanBuy = false;
                }

                //購入可能な最大数になってたらfalse
                if (shopItem[i].itemBought == shopItem[i].itemBoughtMax)
                    shopItem[i].itemCanBuy = false;
               
                else
                    shopItem[i].itemCanBuy = true;
            }
        }
        else//空きがないとき、全部購入不可能にする
        {
            for(int index = 0; index < shopItem.Length; index++)
            {
                shopItem[index].itemCanBuy = false;
            }
        }

        ShopButtonManager sbm;
        GameObject buyButton = GameObject.Find("BuyButton");

        foreach (Transform child in buyButton.transform)
        {
            sbm = child.GetComponent<ShopButtonManager>();
            ButtonActive(sbm.btn, shopItem[sbm.index].itemCanBuy);
        }

        if (n >= 0 && shopItem[n].itemCanBuy == false)
        {
            EventSystem.current.SetSelectedGameObject(GameObject.Find("DummyButton"));
        }
    }

    /// <summary>
    /// ボタンの有効・無効を切り替える
    /// </summary>
    /// <param name="btn">対象のボタン</param>
    /// <param name="active">有効かどうか</param>
    public static void ButtonActive(Button btn, bool active)
    {
        btn.interactable = active;
    }

    #region Editor
#if UNITY_EDITOR
    [CustomEditor(typeof(ShopManager))]
    public class ShopEditor : Editor
    {
        ShopManager shopManager;

        public override void OnInspectorGUI()
        {
            shopManager = target as ShopManager;

            base.OnInspectorGUI();

            if (GUILayout.Button("Update"))
            {
                EditorSceneManager.OpenScene("Assets/Shop/res/ShopScene.unity", OpenSceneMode.Additive);
                DestoryButton();
            }

            if (GUILayout.Button("Close"))
            {
                EditorSceneManager.CloseScene(SceneManager.GetSceneByName("ShopScene"), false);
            }
        }

        private void DestoryButton()
        {
            Transform btnParent = GameObject.Find("BuyButton").transform;
            int count = btnParent.childCount;

            for (int i = 0; i < count; i++)
            {
                DestroyImmediate(btnParent.GetChild(0).gameObject);
            }

            Create(btnParent);
        }

        private void Create(Transform btnParent)
        {
            
            int vertical = 0;
            int horizontal = 0;
            int space = 160;
            GameObject temp;
            for (int i = 0; i < shopManager.shopItem.Length; i++)
            {
                vertical = i % 5;
                //Debug.Log(vertical);
                horizontal = i / 5;
                // プレハブ状態を維持して生成
                temp = PrefabUtility.InstantiatePrefab(shopManager.shopButton) as GameObject;
                //最初に選択するボタンを設定
                temp.tag = (i == 0) ? "firstButton" : "Untagged";
                // 親を設定
                temp.transform.SetParent(btnParent);

                // 大きさがおかしいので戻す
                temp.GetComponent<RectTransform>().localScale = Vector3.one;

                //位置調整
                Vector2 position = Vector2.one;
                position.x = space * horizontal;
                position.y = -space * vertical + 20;
                temp.transform.localPosition = position;

                // 番号を設定
                temp.GetComponent<ShopButtonManager>().index = i;

                // sceneに反映
                temp.GetComponent<ShopButtonManager>().SetButton();

                Undo.RegisterCreatedObjectUndo(temp, "UpdateButton");
            }
        }
    }
#endif
    #endregion
}


[System.Serializable]//インスペクター上で変数を変更できる
public class ShopItem
{
    public string itemName;   // アイテム名
    public int itemPrice;     // 価格
    public bool itemCanBuy;   // 購入可能
    public int itemBought;    // 購入個数
    public int itemBoughtMax; //購入可能最大数
    public int itemID;        //アイテムID
    public float coolDwon;    //アイテムのクールダウンにかかる時間
    public Sprite itemImage;  //アイテムの画像
}
