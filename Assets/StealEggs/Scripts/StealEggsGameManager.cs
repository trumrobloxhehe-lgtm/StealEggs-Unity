using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Local prototype economy: eggs, hatching, passive pet income, quests, upgrades and training.</summary>
public class StealEggsGameManager : MonoBehaviour
{
    public static StealEggsGameManager Instance { get; private set; }

    [Header("Scene references")]
    public Transform player;
    public Transform playerHomeBase;
    public Transform rivalHomeBase;
    public Transform petShowcase;
    public StealEggsGuardianAI[] guardians;

    [Header("HUD")]
    public Text hudText;
    public Text messageText;
    public Text missionText;
    public Text hatchButtonText;
    public Text speedButtonText;
    public Text reachButtonText;
    public Text bagButtonText;
    public Text dashButtonText;

    public int PlayerCarriedEggs { get; private set; }
    public int PlayerBankedEggs { get; private set; }
    public int RivalCarriedEggs { get; private set; }
    public int RivalBankedEggs { get; private set; }
    public int Coins { get; private set; }
    public int UnhatchedEggs { get; private set; }
    public int SpeedLevel { get; private set; }
    public int ReachLevel { get; private set; }
    public int BagLevel { get; private set; }
    public int DashLevel { get; private set; }
    public int TotalDelivered { get; private set; }
    public int TotalStolen { get; private set; }
    public float TrainingSpeed { get; private set; }
    public int PetCount => petRecords.Count;
    public float PetIncomePerSecond { get; private set; }
    public int MaxCarryEggs => 5 + BagLevel * 3;
    public float SpeedMultiplier => (1f + SpeedLevel * .12f) * (1f + TrainingSpeed / 250f);
    public float PickupRangeBonus => ReachLevel * .45f;
    public float DashCooldown => Mathf.Max(1.2f, 4f - DashLevel * .65f);

    private int playerCarriedCoinValue;
    private int rivalCarriedCoinValue;
    private string currentMessage = "Train, steal eggs, escape the guardians, and return to the BLUE base!";
    private bool deliveryQuestPaid;
    private bool stealQuestPaid;
    private const string KeyPrefix = "StealEggs3D_";
    private readonly List<PetRecord> petRecords = new List<PetRecord>();
    private float passiveCoinAccumulator;
    private float autosaveTimer;

    private int SpeedCost => 45 + SpeedLevel * 35;
    private int ReachCost => 35 + ReachLevel * 30;
    private int BagCost => 50 + BagLevel * 40;
    private int DashCost => 60 + DashLevel * 45;
    private int HatchCost => 1;

    [System.Serializable]
    private class PetRecord
    {
        public string name;
        public float income;
        public Color color;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        LoadProgress();
    }

    private void Start()
    {
        RebuildPetShowcase();
        ShowMessage(currentMessage);
    }

    private void Update()
    {
        passiveCoinAccumulator += PetIncomePerSecond * Time.deltaTime;
        if (passiveCoinAccumulator >= 1f)
        {
            int earned = Mathf.FloorToInt(passiveCoinAccumulator);
            Coins += earned;
            passiveCoinAccumulator -= earned;
        }
        autosaveTimer += Time.deltaTime;
        if (autosaveTimer >= 10f) { autosaveTimer = 0f; SaveProgress(); }

        if (hudText != null)
        {
            hudText.text = "STEAL EGGS 3D\n" +
                "BLUE BASE: " + PlayerBankedEggs + " eggs  |  Carry: " + PlayerCarriedEggs + "/" + MaxCarryEggs + "\n" +
                "Coins: " + Coins + "  |  Pets: " + PetCount + "  |  Income: " + PetIncomePerSecond.ToString("0.0") + "/s\n" +
                "Unhatched eggs: " + UnhatchedEggs + "  |  Trained Speed: " + TrainingSpeed.ToString("0");
        }
        if (messageText != null) messageText.text = currentMessage;
        if (missionText != null)
        {
            string deliveryState = deliveryQuestPaid ? "COMPLETE" : (TotalDelivered >= 5 ? "REWARD READY" : TotalDelivered + "/5");
            string stealState = stealQuestPaid ? "COMPLETE" : (TotalStolen >= 3 ? "REWARD READY" : TotalStolen + "/3");
            missionText.text = "MISSIONS\nDeliver 5 eggs: " + deliveryState + " (+60 coins)\nSteal 3 rival eggs: " + stealState + " (+90 coins)";
        }
        if (hatchButtonText != null) hatchButtonText.text = UnhatchedEggs > 0 ? "HATCH EGG (" + UnhatchedEggs + ")" : "NO EGGS TO HATCH";
        if (speedButtonText != null) speedButtonText.text = SpeedLevel >= 5 ? "SPEED MAX" : "SPEED Lv." + SpeedLevel + " (" + SpeedCost + ")";
        if (reachButtonText != null) reachButtonText.text = ReachLevel >= 5 ? "RANGE MAX" : "PICKUP RANGE Lv." + ReachLevel + " (" + ReachCost + ")";
        if (bagButtonText != null) bagButtonText.text = BagLevel >= 5 ? "BAG MAX" : "BIGGER BAG Lv." + BagLevel + " (" + BagCost + ")";
        if (dashButtonText != null) dashButtonText.text = DashLevel >= 4 ? "DASH MAX" : "FASTER DASH Lv." + DashLevel + " (" + DashCost + ")";
    }

    public void ShowMessage(string message) { currentMessage = message; }

    public void TrainSpeed(float amount)
    {
        if (amount <= 0f) return;
        float old = TrainingSpeed;
        TrainingSpeed = Mathf.Min(100f, TrainingSpeed + amount);
        if (Mathf.FloorToInt(old / 10f) != Mathf.FloorToInt(TrainingSpeed / 10f)) SaveProgress();
    }

    public bool CanCollectMoreEggs()
    {
        if (PlayerCarriedEggs < MaxCarryEggs) return true;
        currentMessage = "Your bag is full! Deliver the eggs or upgrade your bag.";
        return false;
    }

    public void PlayerCollectedEgg(EggPickup egg)
    {
        if (egg == null || !CanCollectMoreEggs()) return;
        PlayerCarriedEggs++;
        playerCarriedCoinValue += egg.CoinValue;
        if (egg.spawnedAtRivalBase)
        {
            TotalStolen++;
            currentMessage = "RIVAL EGG STOLEN! Escape the guardian and reach your BLUE base!";
            CheckQuests();
        }
        else currentMessage = egg.RarityLabel + " egg collected! Return to BLUE base.";
        AlertNearestGuardian(egg.transform.position);
    }

    private void AlertNearestGuardian(Vector3 eggPosition)
    {
        if (guardians == null || guardians.Length == 0) return;
        StealEggsGuardianAI nearest = null;
        float best = float.MaxValue;
        foreach (StealEggsGuardianAI guardian in guardians)
        {
            if (guardian == null) continue;
            float d = Vector3.Distance(guardian.transform.position, eggPosition);
            if (d < best) { best = d; nearest = guardian; }
        }
        if (nearest != null && best < 18f) nearest.Alert();
    }

    public void GuardianCaughtPlayer(string guardianName, Vector3 sourcePosition)
    {
        if (PlayerCarriedEggs > 0)
        {
            PlayerCarriedEggs--;
            int lostValue = PlayerCarriedCoinValue / Mathf.Max(1, PlayerCarriedEggs + 1);
            playerCarriedCoinValue = Mathf.Max(0, playerCarriedCoinValue - lostValue);
            currentMessage = guardianName + " caught you! You lost one egg. Recover and try again.";
            StealEggsEffects.SpawnBurst(player != null ? player.position : sourcePosition, new Color(1f, .25f, .18f));
        }
        else currentMessage = guardianName + " caught you! Train more speed to escape next time.";
        if (player != null) player.SendMessage("OnCaughtByGuardian", sourcePosition, SendMessageOptions.DontRequireReceiver);
    }

    public void DepositPlayerEggs()
    {
        if (PlayerCarriedEggs <= 0) return;
        int delivered = PlayerCarriedEggs;
        int oldMilestones = PlayerBankedEggs / 10;
        int earned = playerCarriedCoinValue + delivered * 2;
        PlayerBankedEggs += delivered;
        UnhatchedEggs += delivered;
        TotalDelivered += delivered;
        Coins += earned;
        PlayerCarriedEggs = 0;
        playerCarriedCoinValue = 0;
        if (PlayerBankedEggs / 10 > oldMilestones)
        {
            Coins += 25;
            currentMessage = "MILESTONE! +25 bonus coins. Delivered " + delivered + " egg(s), +" + earned + " coins.";
        }
        else currentMessage = "Delivered " + delivered + " egg(s)! +" + earned + " coins. Hatch an egg at your base!";
        CheckQuests();
        SaveProgress();
        StealEggsEffects.SpawnBurst(playerHomeBase != null ? playerHomeBase.position + Vector3.up * .5f : Vector3.zero, new Color(.3f, 1f, .45f));
    }

    public void RivalCollectedEgg(EggPickup egg)
    {
        if (egg == null) return;
        RivalCarriedEggs++;
        rivalCarriedCoinValue += egg.CoinValue;
        currentMessage = "The rival grabbed a " + egg.RarityLabel.ToLowerInvariant() + " egg. Race them back!";
    }

    public void DepositRivalEggs()
    {
        if (RivalCarriedEggs <= 0) return;
        RivalBankedEggs += RivalCarriedEggs;
        currentMessage = "The rival delivered " + RivalCarriedEggs + " egg(s). Steal from their side to catch up!";
        RivalCarriedEggs = 0;
        rivalCarriedCoinValue = 0;
    }

    public void HatchEgg()
    {
        if (UnhatchedEggs <= 0) { currentMessage = "No eggs to hatch. Steal eggs and deliver them to your base first."; return; }
        UnhatchedEggs--;
        float roll = Random.value;
        string name;
        float income;
        Color color;
        if (roll < .52f) { name = RandomCommonName(); income = Random.Range(1f, 4f); color = new Color(.35f, .82f, .43f); }
        else if (roll < .80f) { name = RandomRareName(); income = Random.Range(5f, 14f); color = new Color(.2f, .8f, 1f); }
        else if (roll < .94f) { name = RandomEpicName(); income = Random.Range(18f, 55f); color = new Color(.62f, .38f, 1f); }
        else if (roll < .99f) { name = RandomLegendaryName(); income = Random.Range(80f, 250f); color = new Color(1f, .68f, .1f); }
        else { name = RandomMythicName(); income = Random.Range(350f, 1000f); color = new Color(1f, .25f, .55f); }
        PetRecord pet = new PetRecord { name = name, income = income, color = color };
        petRecords.Add(pet);
        PetIncomePerSecond += income;
        CreatePetVisual(pet, petRecords.Count - 1);
        Coins += 5;
        currentMessage = "HATCHED: " + name + "! Your pets now earn " + income.ToString("0.0") + " coins/s.";
        SaveProgress();
        StealEggsEffects.SpawnBurst(petShowcase != null ? petShowcase.position : Vector3.zero, color);
    }

    private string RandomCommonName() { string[] n = { "Sproutling", "Pebble Pup", "Moss Chick", "Cloud Bunny", "Pond Hopper" }; return n[Random.Range(0,n.Length)]; }
    private string RandomRareName() { string[] n = { "Frost Foxlet", "Glowfin", "Bramble Kit", "Moon Moth", "Copper Cub" }; return n[Random.Range(0,n.Length)]; }
    private string RandomEpicName() { string[] n = { "Crystal Stag", "Storm Lynx", "Ember Drake", "Prism Owl" }; return n[Random.Range(0,n.Length)]; }
    private string RandomLegendaryName() { string[] n = { "Solar Phoenix", "Aurora Dragon", "Thunder Gryphon" }; return n[Random.Range(0,n.Length)]; }
    private string RandomMythicName() { string[] n = { "Celestial Leviathan", "Void Crown Serpent" }; return n[Random.Range(0,n.Length)]; }

    private void RebuildPetShowcase()
    {
        if (petShowcase == null) return;
        for (int i = petShowcase.childCount - 1; i >= 0; i--) Destroy(petShowcase.GetChild(i).gameObject);
        PetIncomePerSecond = 0f;
        for (int i = 0; i < petRecords.Count; i++)
        {
            PetIncomePerSecond += petRecords[i].income;
            CreatePetVisual(petRecords[i], i);
        }
    }

    private void CreatePetVisual(PetRecord pet, int index)
    {
        if (petShowcase == null) return;
        int row = index / 4;
        int col = index % 4;
        Vector3 position = petShowcase.position + new Vector3((col - 1.5f) * 1.15f, .65f + row * .45f, row * .8f);
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        body.name = "Pet - " + pet.name;
        body.transform.position = position;
        body.transform.localScale = new Vector3(.75f, .68f, .75f);
        Renderer renderer = body.GetComponent<Renderer>();
        renderer.material.color = pet.color;
        Collider collider = body.GetComponent<Collider>();
        if (collider != null) Destroy(collider);
        StealEggsPetVisual visual = body.AddComponent<StealEggsPetVisual>();
        visual.Initialize(pet.name, pet.income, pet.color);
        GameObject earL = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        earL.name = pet.name + " Ear"; earL.transform.SetParent(body.transform, false);
        earL.transform.localPosition = new Vector3(-.35f,.35f,0f); earL.transform.localScale = new Vector3(.35f,.45f,.35f);
        earL.GetComponent<Renderer>().material.color = pet.color * .85f;
        Destroy(earL.GetComponent<Collider>());
        GameObject earR = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        earR.name = pet.name + " Ear"; earR.transform.SetParent(body.transform, false);
        earR.transform.localPosition = new Vector3(.35f,.35f,0f); earR.transform.localScale = new Vector3(.35f,.45f,.35f);
        earR.GetComponent<Renderer>().material.color = pet.color * .85f;
        Destroy(earR.GetComponent<Collider>());
    }

    private void CheckQuests()
    {
        if (!deliveryQuestPaid && TotalDelivered >= 5)
        {
            deliveryQuestPaid = true; Coins += 60; currentMessage += " Delivery mission complete: +60 coins!";
        }
        if (!stealQuestPaid && TotalStolen >= 3)
        {
            stealQuestPaid = true; Coins += 90; currentMessage += " Rival-raider mission complete: +90 coins!";
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
        if (Coins < cost) { currentMessage = "Not enough coins! Need " + cost + "; you have " + Coins + ". Deliver rare eggs or hatch pets for passive income."; return; }
        Coins -= cost;
        switch (type)
        {
            case 0: SpeedLevel++; currentMessage = "Speed upgraded! Movement gets faster."; break;
            case 1: ReachLevel++; currentMessage = "Pickup range upgraded! Collect eggs from farther away."; break;
            case 2: BagLevel++; currentMessage = "Bag upgraded! You can carry more eggs."; break;
            default: DashLevel++; currentMessage = "Dash upgraded! Cooldown is shorter."; break;
        }
        SaveProgress();
    }

    private void LoadProgress()
    {
        PlayerBankedEggs = PlayerPrefs.GetInt(KeyPrefix + "banked", 0);
        Coins = PlayerPrefs.GetInt(KeyPrefix + "coins", 0);
        UnhatchedEggs = PlayerPrefs.GetInt(KeyPrefix + "unhatched", 0);
        SpeedLevel = Mathf.Clamp(PlayerPrefs.GetInt(KeyPrefix + "speed", 0), 0, 5);
        ReachLevel = Mathf.Clamp(PlayerPrefs.GetInt(KeyPrefix + "reach", 0), 0, 5);
        BagLevel = Mathf.Clamp(PlayerPrefs.GetInt(KeyPrefix + "bag", 0), 0, 5);
        DashLevel = Mathf.Clamp(PlayerPrefs.GetInt(KeyPrefix + "dash", 0), 0, 4);
        TotalDelivered = PlayerPrefs.GetInt(KeyPrefix + "delivered", 0);
        TotalStolen = PlayerPrefs.GetInt(KeyPrefix + "stolen", 0);
        TrainingSpeed = Mathf.Clamp(PlayerPrefs.GetFloat(KeyPrefix + "training", 0f), 0f, 100f);
        deliveryQuestPaid = PlayerPrefs.GetInt(KeyPrefix + "questDelivery", 0) == 1;
        stealQuestPaid = PlayerPrefs.GetInt(KeyPrefix + "questStolen", 0) == 1;
        int count = Mathf.Clamp(PlayerPrefs.GetInt(KeyPrefix + "petCount", 0), 0, 1000);
        for (int i = 0; i < count; i++)
        {
            PetRecord p = new PetRecord();
            p.name = PlayerPrefs.GetString(KeyPrefix + "pet" + i + "name", "Sproutling");
            p.income = PlayerPrefs.GetFloat(KeyPrefix + "pet" + i + "income", 1f);
            p.color = new Color(PlayerPrefs.GetFloat(KeyPrefix + "pet" + i + "r", .35f),
                PlayerPrefs.GetFloat(KeyPrefix + "pet" + i + "g", .8f),
                PlayerPrefs.GetFloat(KeyPrefix + "pet" + i + "b", .4f));
            petRecords.Add(p);
        }
    }

    private void SaveProgress()
    {
        PlayerPrefs.SetInt(KeyPrefix + "banked", PlayerBankedEggs);
        PlayerPrefs.SetInt(KeyPrefix + "coins", Coins);
        PlayerPrefs.SetInt(KeyPrefix + "unhatched", UnhatchedEggs);
        PlayerPrefs.SetInt(KeyPrefix + "speed", SpeedLevel);
        PlayerPrefs.SetInt(KeyPrefix + "reach", ReachLevel);
        PlayerPrefs.SetInt(KeyPrefix + "bag", BagLevel);
        PlayerPrefs.SetInt(KeyPrefix + "dash", DashLevel);
        PlayerPrefs.SetInt(KeyPrefix + "delivered", TotalDelivered);
        PlayerPrefs.SetInt(KeyPrefix + "stolen", TotalStolen);
        PlayerPrefs.SetFloat(KeyPrefix + "training", TrainingSpeed);
        PlayerPrefs.SetInt(KeyPrefix + "questDelivery", deliveryQuestPaid ? 1 : 0);
        PlayerPrefs.SetInt(KeyPrefix + "questStolen", stealQuestPaid ? 1 : 0);
        PlayerPrefs.SetInt(KeyPrefix + "petCount", petRecords.Count);
        for (int i = 0; i < petRecords.Count; i++)
        {
            PetRecord p = petRecords[i];
            PlayerPrefs.SetString(KeyPrefix + "pet" + i + "name", p.name);
            PlayerPrefs.SetFloat(KeyPrefix + "pet" + i + "income", p.income);
            PlayerPrefs.SetFloat(KeyPrefix + "pet" + i + "r", p.color.r);
            PlayerPrefs.SetFloat(KeyPrefix + "pet" + i + "g", p.color.g);
            PlayerPrefs.SetFloat(KeyPrefix + "pet" + i + "b", p.color.b);
        }
        PlayerPrefs.Save();
    }
}
