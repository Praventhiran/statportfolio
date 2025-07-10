using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SkillManager : MonoBehaviour
{
    public static SkillManager Instance;

    [Header("References")]
    public GameObject player; // Assign via Inspector or auto-assign on scene load
    [SerializeField] private ComboAttack comboAttackScript; // Assign similarly

    public Dictionary<string, BaseCard> allCards = new Dictionary<string, BaseCard>();
    private List<BaseCard> unlockedCards = new List<BaseCard>();
    private List<StatBoostCard> unlockedStatCards = new List<StatBoostCard>(); 
    private Skill activeSkill;
    private UltimateSkillCard equippedUltimateCard;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadAllCards();
            Debug.Log("SkillManager initialized.");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Debug.Log("[SkillManager] Start() called");
        LoadUnlockedSkillsFromGameManager();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj;
            comboAttackScript = playerObj.GetComponent<ComboAttack>();
            Debug.Log("[SkillManager] Player and ComboAttack references updated on scene load.");

            LoadUnlockedSkillsFromGameManager();
            ApplyStatBonusesToPlayer();

            // Reset ultimate flag for active skill if it is a ClawComboSkill or other ultis
            ResetUltimateFlagIfApplicable();
        }
        else
        {
            Debug.LogWarning("[SkillManager] Player GameObject not found on scene load.");
            player = null;
            comboAttackScript = null;
        }
    }

    private void ResetUltimateFlagIfApplicable()
    {
        if (activeSkill == null) return;

        if (activeSkill is ClawComboSkill clawSkill)
        {
            clawSkill.ResetUltimateFlag();
        }
        else if (activeSkill is LuffyPunchSkill punchSkill)
        {
            punchSkill.ResetUltimateFlag();  // You will implement this method similarly
        }
        // Add other ultis here with ResetUltimateFlag method
    }

    private void Update()
    {
        if (activeSkill != null && player != null)
        {
            activeSkill.UpdateCooldown(Time.deltaTime);

            if (Input.GetKeyDown(KeyCode.Q))
            {
                Debug.Log($"Q pressed. Checking if skill '{activeSkill.skillName}' can activate...");
                if (activeSkill.CanActivate())
                {
                    Debug.Log($"Activating skill: {activeSkill.skillName}");
                    activeSkill.ActivateSkill(player);
                }
                else
                {
                    Debug.Log($"Skill '{activeSkill.skillName}' cannot activate now (on cooldown or active).");
                }
            }
        }
    }

    private void LoadAllCards()
    {
        allCards.Clear();
        BaseCard[] cards = Resources.LoadAll<BaseCard>("Cards");
        Debug.Log($"[Loading] Found {cards.Length} cards in Resources/Cards/");

        foreach (BaseCard card in cards)
        {
            string trimmedID = card.cardID.Trim();
            Debug.Log($"[Card] ID: '{trimmedID}' | Name: {card.cardName}");

            if (!allCards.ContainsKey(trimmedID))
            {
                allCards.Add(trimmedID, card);
            }
            else
            {
                Debug.LogWarning($"Duplicate card ID found: {trimmedID}");
            }
        }
        Debug.Log($"[Success] Total cards loaded: {allCards.Count}");
    }

    public void LoadUnlockedSkillsFromGameManager()
    {
        if (GameManager.Instance == null) return;

        Debug.Log("[SkillManager] Loading cards from GameManager...");
        foreach (string cardID in GameManager.Instance.unlockedCardIDs)
        {
            string trimmedID = cardID.Trim();
            if (!allCards.ContainsKey(trimmedID))
            {
                Debug.LogWarning($"Card ID not found in allCards: '{trimmedID}'");
            }
            else
            {
                UnlockSkill(trimmedID);
                Debug.Log($"Loaded card: {trimmedID}");
            }
        }

        if (activeSkill != null)
            Debug.Log($"Active skill after loading: {activeSkill.skillName}");
        else
            Debug.LogWarning("No active skill assigned after loading.");
    }

    public string GetSkillName(string cardID)
    {
        string trimmedID = cardID.Trim();
        if (allCards.TryGetValue(trimmedID, out BaseCard card))
        {
            if (card is UltimateSkillCard ultimateCard)
                return ultimateCard.skillToUnlock.skillName;
        }
        return "Unknown Skill";
    }

    public void UnlockSkill(string cardID)
    {
        string trimmedID = cardID.Trim();

        if (!allCards.ContainsKey(trimmedID))
        {
            Debug.LogWarning($"No card found for ID: {trimmedID}");
            return;
        }

        BaseCard card = allCards[trimmedID];

        if (!unlockedCards.Contains(card))
        {
            unlockedCards.Add(card);
            Debug.Log($"[Card Unlocked] {card.cardName}");
            ApplyCard(card);
        }
        else
        {
            Debug.Log($"[Card Already Unlocked] {card.cardName}");
        }
    }

    private void ApplyCard(BaseCard card)
    {
        if (card.cardType == CardType.UltimateSkill)
        {
            ApplyUltimateCard(card as UltimateSkillCard);
        }
        else if (card.cardType == CardType.StatBoost)
        {
            ApplyStatBoostCard(card as StatBoostCard);
        }
    }

    private void ApplyUltimateCard(UltimateSkillCard card)
    {
        if (card == null || card.skillToUnlock == null) return;

        activeSkill = card.skillToUnlock;
        equippedUltimateCard = card;
        Debug.Log($"[Ultimate Skill Equipped] {card.skillToUnlock.skillName}");

        // Apply ultimate card stat bonuses same as stat boost cards
        if (player != null)
        {
            var stats = player.GetComponent<CharacterStats>();
            if (stats != null)
            {
                stats.maxHP += card.healthBonus;
                stats.attack += card.attackBonus;
                stats.defense += card.defenseBonus;
                stats.critRate += card.critRateBonus;
                stats.critDamage += card.critDamageBonus;
            }
        }

    }

    private void ApplyStatBoostCard(StatBoostCard card)
    {
        if (card == null) return;
        if (!unlockedStatCards.Contains(card))
        {
            unlockedStatCards.Add(card);
            Debug.Log($"[Stat Card Equipped] {card.cardName}");
        }
    }

    public void ApplyStatBonusesToPlayer()
    {
        if (player == null) return;

        var stats = player.GetComponent<CharacterStats>();
        if (stats == null) return;

        // Reset stats to base if needed
        stats.Initialize();

        // Add bonuses from all unlocked stat cards
        foreach (var statCard in unlockedStatCards)
        {
            stats.maxHP += statCard.healthBonus;
            stats.attack += statCard.attackBonus;
            stats.defense += statCard.defenseBonus;
            stats.critRate += statCard.critRateBonus;
            stats.critDamage += statCard.critDamageBonus;   
        }

        if (equippedUltimateCard != null)
        {
            stats.maxHP += equippedUltimateCard.healthBonus;
            stats.attack += equippedUltimateCard.attackBonus;
            stats.defense += equippedUltimateCard.defenseBonus;
            stats.critRate += equippedUltimateCard.critRateBonus;
            stats.critDamage += equippedUltimateCard.critDamageBonus;
        }

        // Update current HP to new max if you want
        stats.currentHP = stats.maxHP;

        Debug.Log("[SkillManager] Applied stat bonuses to player from stat cards.");
    }
}
