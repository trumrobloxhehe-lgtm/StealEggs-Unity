using UnityEngine;
using UnityEngine.UI;

/// <summary>Tracks the local prototype's carried eggs and banked scores.</summary>
public class StealEggsGameManager : MonoBehaviour
{
    public static StealEggsGameManager Instance { get; private set; }

    public Transform playerHomeBase;
    public Transform rivalHomeBase;
    public Text hudText;
    public Text messageText;

    public int PlayerCarriedEggs { get; private set; }
    public int PlayerBankedEggs { get; private set; }
    public int RivalCarriedEggs { get; private set; }
    public int RivalBankedEggs { get; private set; }

    private string currentMessage = "Collect eggs and return to the blue base!";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        if (hudText != null)
        {
            hudText.text = "STEAL EGGS 3D\n" +
                           "Your base (BLUE): " + PlayerBankedEggs + " eggs\n" +
                           "Carrying: " + PlayerCarriedEggs + " eggs\n" +
                           "Rival base (ORANGE): " + RivalBankedEggs + " eggs (carrying " + RivalCarriedEggs + ")";
        }

        if (messageText != null)
            messageText.text = currentMessage;
    }

    public void PlayerCollectedEgg(bool fromRivalBase)
    {
        PlayerCarriedEggs++;
        currentMessage = fromRivalBase
            ? "You stole an egg from the rival side! Return to BLUE base."
            : "Egg collected! Return to BLUE base to score.";
    }

    public void DepositPlayerEggs()
    {
        if (PlayerCarriedEggs <= 0) return;
        int deposited = PlayerCarriedEggs;
        PlayerBankedEggs += deposited;
        PlayerCarriedEggs = 0;
        currentMessage = "Delivered " + deposited + " egg(s)! Keep collecting.";
    }

    public void RivalCollectedEgg(bool fromRivalBase)
    {
        RivalCarriedEggs++;
        currentMessage = "The rival picked up an egg. Race them to the bases!";
    }

    public void DepositRivalEggs()
    {
        if (RivalCarriedEggs <= 0) return;
        int deposited = RivalCarriedEggs;
        RivalBankedEggs += deposited;
        RivalCarriedEggs = 0;
        currentMessage = "Rival delivered " + deposited + " egg(s). You can still catch up!";
    }
}
