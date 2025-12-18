using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MoneyManager : Singleton<MoneyManager>
{

    public int currentMoney = 0;

    public TextMeshProUGUI moneyText;


    private void Start()
    {
        UpdateMoneyUI();
    }

    public void AddMoney(int amount)
    {
        currentMoney += amount;
        UpdateMoneyUI();
    }

    public void SpendMoney(int amount)
    {
        if (currentMoney < amount)
            return;

        currentMoney -= amount;
        UpdateMoneyUI();
    }

    private void UpdateMoneyUI()
    {
        if (moneyText != null)
            moneyText.text = currentMoney.ToString();
    }

}
