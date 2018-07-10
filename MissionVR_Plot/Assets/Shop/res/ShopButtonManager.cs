using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopButtonManager : MonoBehaviour {

    ShopManager shopManager;
    ShopItem shopItem;
    public Button btn;
    public int index;

    private void Awake()
    {
        // ボタンのテキストにアイテム名を表示
        SetButton();

        btn = GetComponent<Button>();
        ShopManager.ButtonActive(btn, shopItem.itemCanBuy);
    }

    public void SetButton()
    {
        shopManager = GameObject.Find("ShopManager").GetComponent<ShopManager>();
        shopItem = shopManager.shopItem[index];
        GetComponentInChildren<Text>().text = (shopItem.itemName + "\n" + shopItem.itemPrice);
    }

    public void OnButtonClick()
    {
        shopManager.Buy(index);
    }
}
