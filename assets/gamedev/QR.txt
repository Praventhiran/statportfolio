using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using ZXing;
using System.Collections;

public class QRCodeScanner : MonoBehaviour
{
    [Header("QR Camera")]
    private WebCamTexture camTexture;
    private Color32[] cameraColorData;
    private int width, height;
    private CancellationTokenSource cts = new CancellationTokenSource();
    private BarcodeReader qrReader = new BarcodeReader
    {
        AutoRotate = false,
        Options = new ZXing.Common.DecodingOptions
        {
            TryHarder = false,
            PossibleFormats = new System.Collections.Generic.List<BarcodeFormat> { BarcodeFormat.QR_CODE }
        }
    };
    private bool _hasScanned;
    private bool startDecoding;

    [Header("Panel References")]

    public GameObject cameraPanel;
    public GameObject manualInputPanel;
    public GameObject cardDisplayPanel;
    public SelectedCardsUI selectedCardsUI;

    [Header("UI References")]
    public Image cardDisplayImage;
    public Text skillNameText;
    public Text descriptionCardDescriptionText;
    public Text cardIDText;
    public Text healthText;
    public Text attackText;
    public Text defenseText;
    public Text critRateText;
    public Text critDamageText;
    public Button applyButton;
    public GameObject qrScannerPanel;
    public GameObject skillUI;
    public InputField manualInputField;
    public RawImage cameraFeedRawImage;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip scanSuccessSFX;
    [SerializeField] private AudioClip scanErrorSFX;
    [SerializeField] private AudioClip unlockSFX;

    private string currentValidSkillID;

    void Start()
    {
        // Disable scanning outside Lobby map
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "LobbyMap")
        {
            this.enabled = false;
            return;
        }

        camTexture = new WebCamTexture();
        cameraFeedRawImage.texture = camTexture;
        camTexture.Play();

        width = camTexture.width;
        height = camTexture.height;
        cameraColorData = new Color32[width * height];

        ThreadPool.QueueUserWorkItem(ScanQRCode, cts.Token);

        applyButton.interactable = false;
        skillNameText.text = "";
        
    }

    private void Update()
    {
        if (!startDecoding && camTexture != null && camTexture.isPlaying)
        {
            cameraColorData = camTexture.GetPixels32();
            startDecoding = true;
        }
    }

    //private void OnGUI()
    //{
    //    if (qrScannerPanel.activeSelf && camTexture != null)
    //    {
    //        //GUI.DrawTexture(new Rect(50, 50, 400, 400), camTexture, ScaleMode.ScaleToFit);
    //    }
    //}

    private void OnEnable()
    {
        if (camTexture != null) camTexture.Play();
    }

    private void OnDisable()
    {
        if (camTexture != null) camTexture.Pause();
    }

    private void OnDestroy()
    {
        if (camTexture != null) camTexture.Stop();
        cts.Cancel();
        cts.Dispose();
    }

    private void ScanQRCode(object obj)
    {
        var token = (CancellationToken)obj;
        while (!token.IsCancellationRequested && !_hasScanned)
        {
            if (startDecoding)
            {
                var result = qrReader.Decode(cameraColorData, width, height);
                if (result != null)
                {
                    _hasScanned = true;
                    string scannedID = result.Text.Trim();
                    ProcessScannedSkill(scannedID);
                }
                startDecoding = false;
            }
        }
    }

    public void SubmitManualInput()
    {
        string input = manualInputField.text.Trim();
        if (!string.IsNullOrEmpty(input))
        {
            ProcessScannedSkill(input);
        }
    }

    private void ProcessScannedSkill(string cardID)
    {
        UnityMainThreadDispatcher.Enqueue(() =>
        {
            if (GameManager.Instance == null)
            {
                Debug.LogWarning("GameManager missing! Can't unlock skill");
                skillNameText.text = "Error: GameManager missing";
                applyButton.interactable = false;
                currentValidSkillID = null;
                return;
            }

            if (GameManager.Instance.IsCardUnlocked(cardID))
            {
                skillNameText.text = $"Already unlocked: {cardID}";
                applyButton.interactable = false;
                currentValidSkillID = null;
                return;
            }

            if (SkillManager.Instance != null && SkillManager.Instance.allCards.ContainsKey(cardID))
            {
                BaseCard card = SkillManager.Instance.allCards[cardID];
                // Important: only add to GameManager here, do NOT unlock SkillManager yet
                GameManager.Instance.UnlockCard(cardID);

                // Show CardDisplayPanel and description
                cameraPanel.SetActive(false);
                manualInputPanel.SetActive(false);
                cardDisplayPanel.SetActive(true);

                cardDisplayImage.sprite = card.cardImage;
                skillNameText.text = card.cardName;
                descriptionCardDescriptionText.text = card.description;

                // Display Card ID
                cardIDText.text = $"ID: {card.cardID}";

                // Display stats based on card type
                if (card is UltimateSkillCard ultiCard)
                {
                    healthText.text = $"Health: {ultiCard.healthBonus}";
                    attackText.text = $"Attack: {ultiCard.attackBonus}";
                    defenseText.text = $"Defense: {ultiCard.defenseBonus}";
                    critRateText.text = $"Crit Rate: {ultiCard.critRateBonus}%";
                    critDamageText.text = $"Crit Damage: {ultiCard.critDamageBonus}x";
                }
                else if (card is StatBoostCard statCard)
                {
                    healthText.text = $"Health: {statCard.healthBonus}";
                    attackText.text = $"Attack: {statCard.attackBonus}";
                    defenseText.text = $"Defense: {statCard.defenseBonus}";
                    critRateText.text = $"Crit Rate: {statCard.critRateBonus}%";
                    critDamageText.text = $"Crit Damage: {statCard.critDamageBonus}x";
                }
                else
                {
                    // No stats available for other card types
                    healthText.text = "Health: N/A";
                    attackText.text = "Attack: N/A";
                    defenseText.text = "Defense: N/A";
                    critRateText.text = "Crit Rate: N/A";
                    critDamageText.text = "Crit Damage: N/A";
                }

                applyButton.interactable = true;

                currentValidSkillID = cardID;

                PlaySFX(scanSuccessSFX);
            }
            else
            {
                skillNameText.text = "Invalid card!";
                applyButton.interactable = false;
                currentValidSkillID = null;
                PlaySFX(scanErrorSFX);

                // Clear the additional UI fields to avoid stale data
                cardIDText.text = "";
                healthText.text = "";
                attackText.text = "";
                defenseText.text = "";
                critRateText.text = "";
                critDamageText.text = "";
            }
        });
    }

    public void DisplayCardFromInventory(StatBoostCard card)
    {
        if (card == null) return;

        // Show card display panel and hide others
        cameraPanel.SetActive(false);
        manualInputPanel.SetActive(false);
        cardDisplayPanel.SetActive(true);

        cardDisplayImage.sprite = card.cardImage;
        skillNameText.text = card.cardName;
        descriptionCardDescriptionText.text = card.description;

        // Show Card ID and stats, similar to ProcessScannedSkill
        cardIDText.text = $"ID: {card.cardID}";
        healthText.text = $"Health: {card.healthBonus}";
        attackText.text = $"Attack: {card.attackBonus}";
        defenseText.text = $"Defense: {card.defenseBonus}";
        critRateText.text = $"Crit Rate: {card.critRateBonus}%";
        critDamageText.text = $"Crit Damage: {card.critDamageBonus}x";

        applyButton.interactable = !GameManager.Instance.IsCardUnlocked(card.cardID);

        currentValidSkillID = card.cardID;
    }

    public void ApplySkill()
    {
        if (string.IsNullOrEmpty(currentValidSkillID)) return;

        if (SkillManager.Instance == null || selectedCardsUI == null) return;

        BaseCard card = SkillManager.Instance.allCards[currentValidSkillID];

        if (card.cardType == CardType.UltimateSkill)
        {
            selectedCardsUI.UpdateUltimateCard(card as UltimateSkillCard);
        }
        else if (card.cardType == CardType.StatBoost)
        {
            selectedCardsUI.AddStatCard(card as StatBoostCard);
        }

        applyButton.interactable = false;
        currentValidSkillID = null;

        Debug.Log("Card selected in UI only. Not confirmed yet.");
    }

    public void ConfirmSelectionAndStartBattle()
    {
        if (GameManager.Instance == null || SkillManager.Instance == null || selectedCardsUI == null)
        {
            Debug.LogWarning("Missing references, cannot confirm and start battle");
            return;
        }

        Debug.Log("Confirming selected cards and starting battle...");

        // Clear previously selected stat cards in GameManager
        GameManager.Instance.selectedStatCardIDs.Clear();

        // Unlock ultimate card if any
        UltimateSkillCard ultiCard = selectedCardsUI.GetCurrentUltimateCard();
        if (ultiCard != null)
        {
            SkillManager.Instance.UnlockSkill(ultiCard.cardID);
            GameManager.Instance.UnlockCard(ultiCard.cardID);
        }

        // Unlock all selected stat cards
        foreach (var statCard in selectedCardsUI.GetSelectedStatCards())
        {
            SkillManager.Instance.UnlockSkill(statCard.cardID);
            GameManager.Instance.UnlockCard(statCard.cardID);
            GameManager.Instance.selectedStatCardIDs.Add(statCard.cardID);
        }

        // Load next scene
        string nextScene = GameManager.Instance.selectedStageID;
        if (!string.IsNullOrEmpty(nextScene))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextScene);
        }
        else
        {
            Debug.LogWarning("No stage selected to load.");
        }
    }

    public void StartScan()
    {
        _hasScanned = false;
        currentValidSkillID = null;
        if (camTexture != null && !camTexture.isPlaying)
            camTexture.Play();
        qrScannerPanel.SetActive(true);
        applyButton.interactable = false;
        skillNameText.text = "";
    }

    public void QuitScanning()
    {
        if (camTexture != null && camTexture.isPlaying)
            camTexture.Stop();

        if (qrScannerPanel != null)
            qrScannerPanel.SetActive(false);
        if (skillUI != null)
            skillUI.SetActive(false);

        _hasScanned = false;
        currentValidSkillID = null;
        Debug.Log("Scanning cancelled");
    }

    private void PlaySFX(AudioClip clip)
    {
        UnityMainThreadDispatcher.Enqueue(() =>
        {
            if (audioSource != null && clip != null)
                audioSource.PlayOneShot(clip);
        });
    }
}
