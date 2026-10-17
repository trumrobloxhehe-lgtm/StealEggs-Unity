using UnityEngine;
using UnityEngine.UI;

/// <summary>Runtime binding for prototype HUD shop and hatch buttons.</summary>
[RequireComponent(typeof(Button))]
public class StealEggsShopButton : MonoBehaviour
{
    public enum ActionType { HatchEgg, BuySpeed, BuyReach, BuyBag, BuyDash }
    public ActionType actionType;
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(Execute);
    }

    private void OnDestroy()
    {
        if (button != null) button.onClick.RemoveListener(Execute);
    }

    private void Execute()
    {
        StealEggsGameManager manager = StealEggsGameManager.Instance;
        if (manager == null) return;
        switch (actionType)
        {
            case ActionType.HatchEgg: manager.HatchEgg(); break;
            case ActionType.BuySpeed: manager.BuySpeedUpgrade(); break;
            case ActionType.BuyReach: manager.BuyReachUpgrade(); break;
            case ActionType.BuyBag: manager.BuyBagUpgrade(); break;
            case ActionType.BuyDash: manager.BuyDashUpgrade(); break;
        }
    }
}
