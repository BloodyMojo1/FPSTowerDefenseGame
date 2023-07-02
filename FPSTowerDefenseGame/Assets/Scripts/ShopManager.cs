using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    public GameObject ShopArea;

    [Header("Shop Button")]
    
    [SerializeField] private int buttonAmount;
    [SerializeField] private GameObject buttonOptionPrefab;

    private GameObject buttonGameObject;

    [Header("Loot Options")]

    [SerializeField] private List<ShopLoot> lootList = new List<ShopLoot>();
    private ShopLoot droppedItem;
    

    ShopLoot GetDroppedItem()
    {
        int randomNumber = Random.Range(1, 101); //Rolls between 1-100
        List<ShopLoot> possibleItems = new List<ShopLoot>();
        
        //Gets loops through all loot in the loot list
        foreach (ShopLoot item in lootList)
        {
            if (randomNumber <= item.dropChance) //Checks each items drop chance & adds them to a new list
            {
                possibleItems.Add(item);
            }
        }
        if (possibleItems.Count > 0)
        {
            ShopLoot droppedItem = possibleItems[Random.Range(0, possibleItems.Count)]; //Rnadomly pick any option in the availble item list.
            return droppedItem;
        }
        return null;
    }

    /// <summary>
    /// Instantiates certain amount of button options
    /// Adds all instantiated buttons with what dropped item values
    /// </summary>
    public void InstantiateLoot()
    {
        //Instantia
        for (int i = 0; i < buttonAmount; i++)  //Loops through & instantiates buttons
        {
            droppedItem = GetDroppedItem(); 

            if (droppedItem != null)
            {
                buttonGameObject = Instantiate(buttonOptionPrefab, Vector3.zero, Quaternion.identity, transform.GetChild(0));  //Instaniates Button prefab
                //buttonGameObject.GetComponent<SpriteRenderer>().sprite = droppedItem.lootSprite;
                TextMeshProUGUI buttonText = buttonGameObject.GetComponentInChildren<TextMeshProUGUI>(); 
                Button[] buttons = buttonGameObject.GetComponentsInChildren<Button>();  //Get all availble buttons

                //Asigns each buttons with loot values
                for(int b = 0; b < buttons.Length; b++)
                {
                    int cacheIndex = droppedItem.itemCost; //Gives each button their loot values
                    buttons[b].onClick.AddListener(() => BuyButton(cacheIndex)); //Checks if button has been pressed
                }
                buttonText.SetText(System.Convert.ToString(droppedItem.name)); //Changes button name with loot option
            }
        }
    }

    /// <summary>
    /// Destroys all buttons
    /// </summary>
    public void DestroyButtons()
    {
        foreach (Transform clone in ShopArea.transform)
        {
            GameObject.Destroy(clone.gameObject);
        }
    }

    public void BuyButton(int index)
    {
        CurrencyManager.main.SpendCurrency(index);
    }
}
