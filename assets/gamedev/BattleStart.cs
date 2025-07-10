using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleStarter : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject stageSelectUIPanel;    // Assign in Inspector
    public GameObject skillSelectUIPanel;    // Assign in Inspector

    private bool isPlayerNear = false;
    private string selectedStage = null;

    void Update()
    {
        if (!isPlayerNear) return;

        if (Input.GetKeyDown(KeyCode.F))
        {
            if (!stageSelectUIPanel.activeSelf && !skillSelectUIPanel.activeSelf)
            {
                OpenStageSelectUI();
            }
            else
            {
                CloseAllUIs();
            }
        }
    }

    private void OpenStageSelectUI()
    {
        stageSelectUIPanel.SetActive(true);
        skillSelectUIPanel.SetActive(false);
        ToggleCursor(true);
    }

    private void OpenSkillSelectUI()
    {
        skillSelectUIPanel.SetActive(true);
        stageSelectUIPanel.SetActive(false);
        ToggleCursor(true);
    }

    private void CloseAllUIs()
    {
        stageSelectUIPanel.SetActive(false);
        skillSelectUIPanel.SetActive(false);
        ToggleCursor(false);
    }

    private void ToggleCursor(bool visible)
    {
        Cursor.lockState = visible ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = visible;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            isPlayerNear = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            CloseAllUIs();
        }
    }

    // Called by stage buttons in StageSelect UI
    public void SelectStage(string stageSceneName)
    {
        selectedStage = stageSceneName;
        Debug.Log($"Stage selected: {selectedStage}");
        // Optionally update UI to show selected stage
    }

    // Called by Next button in StageSelect UI
    public void OnNextFromStageSelect()
    {
        if (!string.IsNullOrEmpty(selectedStage))
        {
            GameManager.Instance.selectedStageID = selectedStage;
            OpenSkillSelectUI();
        }
        else
        {
            Debug.LogWarning("Please select a stage before proceeding.");
            // Optionally show a UI warning message
        }
    }

    // Called by Confirm button in SkillSelect UI
    public void OnConfirmSkillSelect()
    {
        if (!string.IsNullOrEmpty(GameManager.Instance.selectedStageID))
        {
            Debug.Log($"Loading scene: {GameManager.Instance.selectedStageID}");
            CloseAllUIs();
            SceneManager.LoadScene(GameManager.Instance.selectedStageID);
        }
        else
        {
            Debug.LogWarning("No stage selected!");
        }
    }
}