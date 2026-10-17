using UnityEngine;
using UnityEngine.UI;

/// <summary>Owns scores, coins, upgrades, quests and saved local progress for the prototype.</summary>
public class StealEggsGameManager : MonoBehaviour
{
    public static StealEggsGameManager Instance { get; private set; }

    public Transform playerHomeBase;
    public Transform rivalHomeBase;
    public Text hudText;
    public Text messageText;
    public Text missionText;
    public Text speedButtonText;
    public Text reachButtonText;
    public Text bagButtonText;
    public Text dashButtonText;

    public int PlayerCarriedEggs { get; private set; }
    public int PlayerBankedEggs { get; private set; }
    public int RivalCarriedEggs { get; private set; }
    public int RivalBankedEggs { get; private set; }
    public int Coins { get; private set; }
    public int SpeedLevel { get; private set; }
    public int ReachLevel { get; private set; }
    public int BagLevel { get; private set; }
    public int DashLevel { get; private set; }
    public int TotalDelivered { get; private set; }
    public int TotalStolen { get; private set; }
    public int MaxCarryEggs => 5 + BagLevel * 3;
    public float SpeedMultiplier => 1f + SpeedLevel * 0.12f;
    public float PickupRangeBonus => ReachLevel * 0.45f;
    public float DashCooldown => Mathf.Max(1.2f, 4.0f - DashLevel * 0.65f);

    private int playerCarriedCoinValue;
    private int rivalCarriedCoinValue;
    private string currentMessage = "Collect eggs, then bring them back to your BLUE base!";
    private bool deliveryQuestPaid;
    private bool stealQuestPaid;
    private const string KeyPrefix = "StealEggs3D_";

    private int SpeedCost => 45 + SpeedLevel * 35;
    private int ReachCost => 35 + ReachLevel * 30;
    private int BagCost => 50 + BagLevel * 40;
    private int DashCost => 60 + DashLevel * 45;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        LoadProgress();
    }

    private void Update()
    {
        if (hudText != null)
        {
            hudText.text = "STEAL EGGS 3D\n" +
                "BLUE BASE: " + PlayerBankedEggs + " eggs\n" +
                "Bag: " + PlayerCarriedEggs + "/" + MaxCarryEggs + " eggs  |  Coins: " + Coins + "\n" +
                "Bag value: " + playerCarriedCoinValue + " coins\n" +
                "RIVAL: " + RivalBankedEggs + " eggs";
        }
        if (messageText != null) messageText.text = currentMessage;
        if (missionText != null)
        {
            string deliveryState = deliveryQuestPaid ? "COMPLETE" : (TotalDelivered >= 5 ? "REWARD READY" : "" + TotalDelivered + "/5");
            string stealState = stealQuestPaid ? "COMPLETE" : (TotalStolen >= 3 ? "REWARD READY" : "" + TotalStolen + "/3");
            missionText.text = "MISSIONS\nDeliver 5 eggs: " + deliveryState + " (+60 coins)\nSteal 3 rival eggs: " + stealState + " (+90 coins)";
        }
        if (speedButtonText != null) speedButtonText.text = SpeedLevel >= 5 ? "SPEED MAX" : "SPEED Lv." + SpeedLevel + " (" + SpeedCost + ")";
        if (reachButtonText != null) reachButtonText.text = ReachLevel >= 5 ? "RANGE MAX" : "PICKUP RANGE Lv." + ReachLevel + " (" + ReachCost + ")";
        if (bagButtonText != null) bagButtonText.text = BagLevel >= 5 ? "BAG MAX" : "BIGGER BAG Lv." + BagLevel + " (" + BagCost + ")";
        if (dashButtonText != null) dashButtonText.text = DashLevel >= 4 ? "DASH MAX" : "FASTER DASH Lv." + DashLevel + " (" + DashCost + ")";
    }

    public bool CanCollectMoreEggs()
    {
        if (PlayerCarriedEggs < MaxCarryEggs) return true;
        currentMessage = "Your bag is full! Return to the BLUE base or buy a bigger bag.";
        return false;
    }

    public void ShowMessage(string message) { currentMessage = message; }

    public void PlayerCollectedEgg(EggPickup egg)
    {
        if (egg == null || !CanCollectMoreEggs()) return;
        PlayerCarriedEggs++;
        playerCarriedCoinValue += egg.CoinValue;
        if (egg.spawnedAtRivalBase)
        {
            TotalStolen++;
            currentMessage = "RIVAL EGG STOLEN! Get it back to BLUE base for coins!";
            CheckQuests();
        }
        else currentMessage = egg.RarityLabel + " egg collected! Bring it to BLUE base.";
    }

    public void DepositPlayerEggs()
    {
        if (PlayerCarriedEggs <= 0) return;
        int delivered = PlayerCarriedEggs;
        int oldMilestones = PlayerBankedEggs / 10;
        int earned = playerCarriedCoinValue + delivered * 2;
        PlayerBankedEggs += delivered;
        TotalDelivered += delivered;
        Coins += earned;
        PlayerCarriedEggs = 0;
        playerCarriedCoinValue = 0;
        if (PlayerBankedEggs / 10 > oldMilestones)
        {
            Coins += 25;
            currentMessage = "MILESTONE! +25 bonus coins for reaching " + (PlayerBankedEggs / 10 * 10) + " eggs! Delivered " + delivered + ", earned " + earned + " coins.";
        }
        else currentMessage = "Delivered " + delivered + " egg(s)! +" + earned + " coins. Keep collecting!";
        CheckQuests();
        SaveProgress();
        StealEggsEffects.SpawnBurst(playerHomeBase != null ? playerHomeBase.position + Vector3.up * 0.5f : Vector3.zero, new Color(0.3f, 1f, 0.45f));
    }

    public void RivalCollectedEgg(EggPickup egg)
    {
        if (egg == null) return;
        RivalCarriedEggs++;
        rivalCarriedCoinValue += egg.CoinValue;
        currentMessage = "Rival took a " + egg.RarityLabel.ToLowerInvariant() + " egg. Race them back!";
    }

    public void DepositRivalEggs()
    {
        if (RivalCarriedEggs <= 0) return;
        RivalBankedEggs += RivalCarriedEggs;
        currentMessage = "Rival scored " + RivalCarriedEggs + " egg(s). Steal the rare ones to earn more!";
        RivalCarriedEggs = 0;
        rivalCarriedCoinValue = 0;
    }

    private void CheckQuests()
    {
        if (!deliveryQuestPaid && TotalDelivered >= 5)
        {
            deliveryQuestPaid = true;
            Coins += 60;
            currentMessage += " MISSION COMPLETE: +60 coins!";
        }
        if (!stealQuestPaid && TotalStolen >= 3)
        {
            stealQuestPaid = true;
            Coins += 90;
            currentMessage += " RIVAL-RAIDER MISSION: +90 coins!";
        }
        SaveProgress();
    }

    public void BuySpeedUpgrade() { BuyUpgrade(0); }
    public void BuyReachUpgrade() { BuyUpgrade(1); }
    public void BuyBagUpgrade() { BuyUpgrade(2); }
    public void BuyDashUpgrade() { BuyUpgrade(3); }

    private void BuyUpgrade(int type)
    {
        int level, cost, max;
        switch (type)
        {
            case 0: level = SpeedLevel; cost = SpeedCost; max = 5; break;
            case 1: level = ReachLevel; cost = ReachCost; max = 5; break;
            case 2: level = BagLevel; cost = BagCost; max = 5; break;
            default: level = DashLevel; cost = DashCost; max = 4; break;
        }
        if (level >= max) { currentMessage = "This upgrade is already max level."; return; }
        if (Coins < cost) { currentMessage = "Not enough coins! Need " + cost + "; you have " + Coins + ". Collect rare eggs and deliver them."; return; }
        Coins -= cost;
        switch (type)
        {
            case 0: SpeedLevel++; currentMessage = "Speed upgraded! Movement is faster."; break;
            case 1: ReachLevel++; currentMessage = "Pickup range upgraded! Collect eggs from farther away."; break;
            case 2: BagLevel++; currentMessage = "Bag upgraded! You can carry more eggs per trip."; break;
            default: DashLevel++; currentMessage = "Dash upgraded! Dash cooldown is shorter."; break;
        }
        SaveProgress();
    }

    private void LoadProgress()
    {
        PlayerBankedEggs = PlayerPrefs.GetInt(KeyPrefix + "banked", 0);
        Coins = PlayerPrefs.GetInt(KeyPrefix + "coins", 0);
        SpeedLevel = Mathf.Clamp(PlayerPrefs.GetInt(KeyPrefix + "speed", 0), 0, 5);
        ReachLevel = Mathf.Clamp(PlayerPrefs.GetInt(KeyPrefix + "reach", 0), 0, 5);
        BagLevel = Mathf.Clamp(PlayerPrefs.GetInt(KeyPrefix + "bag", 0), 0, 5);
        DashLevel = Mathf.Clamp(PlayerPrefs.GetInt(KeyPrefix + "dash", 0), 0, 4);
        TotalDelivered = PlayerPrefs.GetInt(KeyPrefix + "delivered", 0);
        TotalStolen = PlayerPrefs.GetInt(KeyPrefix + "stolen", 0);
        deliveryQuestPaid = PlayerPrefs.GetInt(KeyPrefix + "questDelivery", 0) == 1;
        stealQuestPaid = PlayerPrefs.GetInt(KeyPrefix + "questStolen", 0) == 1;
    }

    private void SaveProgress()
    {
        PlayerPrefs.SetInt(KeyPrefix + "banked", PlayerBankedEggs);
        PlayerPrefs.SetInt(KeyPrefix + "coins", Coins);
        PlayerPrefs.SetInt(KeyPrefix + "speed", SpeedLevel);
        PlayerPrefs.SetInt(KeyPrefix + "reach", ReachLevel);
        PlayerPrefs.SetInt(KeyPrefix + "bag", BagLevel);
        PlayerPrefs.SetInt(KeyPrefix + "dash", DashLevel);
        PlayerPrefs.SetInt(KeyPrefix + "delivered", TotalDelivered);
        PlayerPrefs.SetInt(KeyPrefix + "stolen", TotalStolen);
        PlayerPrefs.SetInt(KeyPrefix + "questDelivery", deliveryQuestPaid ? 1 : 0);
        PlayerPrefs.SetInt(KeyPrefix + "questStolen", stealQuestPaid ? 1 : 0);
        PlayerPrefs.Save();
    }
}
