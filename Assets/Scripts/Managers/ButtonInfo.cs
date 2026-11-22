using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ButtonInfo : MonoBehaviour
{
    public int itemId;
    public TMP_Text QuantityTXT;
    public TMP_Text PriceTXT;
    public GameObject ShopManager;
    [SerializeField] private bool usePlayerInventory;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    // Update is called once per frame
    void Update()
    {
        ShopManager shopManager = ShopManager.GetComponent<ShopManager>();
        var items = usePlayerInventory ? shopManager.playerInventory : shopManager.shopItems;
        PriceTXT.text = "Price: $" + items[itemId].price.ToString();
        QuantityTXT.text = items[itemId].quantity.ToString();
    }
}
