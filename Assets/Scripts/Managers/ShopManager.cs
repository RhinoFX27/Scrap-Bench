using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class ShopManager : MonoBehaviour
{
    //This class is the way I want to structure items in the game in general
    public class item
    {
        public int id;
        public string name;
        public string description;
        public int price;
        public int quantity;

        public item(int id, string name, string description, int price, int quantity)
        {
            this.id = id;
            this.name = name;
            this.description = description;
            this.price = price;
            this.quantity = quantity;
        }
    }
    public item[] playerInventory = new item[5];
    public item[] shopItems;
    public float currency;
    [SerializeField] private TMP_Text[] currencyTexts;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    //
    void Start()
    {
        currency = 1000;
        UpdateCurrencyTexts();

        // This section of the shopManager will be used to setup all the items in the shop and allow it to be subject to change. 
        shopItems = new item[]
        {
            new item(1, "Pistol", "A basic sidearm.", 100, 1),
            new item(2, "Shotgun", "A powerful close-range weapon.", 250, 1),
            new item(3, "Rifle", "A versatile long-range weapon.", 300, 1),
            new item(4, "Sniper Rifle", "A high-precision long-range weapon.", 500, 1),
            new item(5, "SMG", "A fast-firing close to mid-range weapon.", 200, 1),
            new item(6, "Heavy Pistol", "A powerful sidearm with high damage.", 150, 1),
            new item(7, "Grenade Launcher", "A weapon that launches explosive grenades.", 400, 1),
            new item(8, "Rocket Launcher", "A weapon that fires rockets for massive damage.", 600, 1),
            new item(9, "Laser Rifle", "A futuristic energy weapon.", 350, 1),
            new item(10, "Bullets", "Ammunition for Firearms", 10, 120)
        };
        
        playerInventory = new item[]
        {
            new item(1, "Pistol", "A basic sidearm.", 100, 0),
            new item(2, "Shotgun", "A powerful close-range weapon.", 250, 0),
            new item(3, "Rifle", "A versatile long-range weapon.", 300, 0),
            new item(4, "Sniper Rifle", "A high-precision long-range weapon.", 500, 0),
            new item(5, "SMG", "A fast-firing close to mid-range weapon.", 200, 0),
            new item(6, "Heavy Pistol", "A powerful sidearm with high damage.", 150, 0),
            new item(7, "Grenade Launcher", "A weapon that launches explosive grenades.", 400, 0),
            new item(8, "Rocket Launcher", "A weapon that fires rockets for massive damage.", 600, 0),
            new item(9, "Laser Rifle", "A futuristic energy weapon.", 350, 0),
            new item(10, "Bullets", "Ammunition for Firearms", 10, 0)
        };
    }

    private void UpdateCurrencyTexts()
    {
        foreach (TMP_Text currencyText in currencyTexts)
        {
            if (currencyText == null)
            {
                Debug.LogWarning("A currency text reference is not assigned.", this);
                continue;
            }

            currencyText.text = "Currency: " + currency.ToString();
        }
    }

    // Update is called once per frame
    
    public void Buy()
    {
        GameObject ButtonRef = EventSystem.current != null
            ? EventSystem.current.currentSelectedGameObject
            : null;

        if(currency >= shopItems[ButtonRef.GetComponent<ButtonInfo>().itemId].price)
        {
            if(shopItems[ButtonRef.GetComponent<ButtonInfo>().itemId].quantity <= 0)
            {
                return;
            }else{
            currency -= shopItems[ButtonRef.GetComponent<ButtonInfo>().itemId].price;
            shopItems[ButtonRef.GetComponent<ButtonInfo>().itemId].quantity--;
            playerInventory[ButtonRef.GetComponent<ButtonInfo>().itemId].quantity++;
            UpdateCurrencyTexts();
            ButtonRef.GetComponent<ButtonInfo>().QuantityTXT.text = shopItems[ButtonRef.GetComponent<ButtonInfo>().itemId].quantity.ToString();
        }
    }
    }
    public void Sell()
    {
        GameObject ButtonRef = EventSystem.current != null
            ? EventSystem.current.currentSelectedGameObject
            : null;

        if(playerInventory[ButtonRef.GetComponent<ButtonInfo>().itemId].quantity > 0)
        {
            currency += playerInventory[ButtonRef.GetComponent<ButtonInfo>().itemId].price;
            playerInventory[ButtonRef.GetComponent<ButtonInfo>().itemId].quantity--;
            UpdateCurrencyTexts();
            ButtonRef.GetComponent<ButtonInfo>().QuantityTXT.text = playerInventory[ButtonRef.GetComponent<ButtonInfo>().itemId].quantity.ToString();
        }
    }
}

