using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestDialogUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject panel;

    [Header("Tekst")]
    [SerializeField] private TMP_Text questNameText;
    [SerializeField] private TMP_Text descriptionText;

    [Header("Przyciski")]
    [SerializeField] private Button acceptButton;
    [SerializeField] private Button completeButton;
    [SerializeField] private Button closeButton;

    private Action onAcceptCallback;
    private Action onCompleteCallback;

    private void Start()
    {
        panel.SetActive(false);
        acceptButton.onClick.AddListener(OnAcceptClicked);
        completeButton.onClick.AddListener(OnCompleteClicked);
        closeButton.onClick.AddListener(Hide);
    }

    public void ShowQuestOffer(QuestData quest, Action onAccept)
    {
        onAcceptCallback = onAccept;
        questNameText.text = quest.questName;
        descriptionText.text = quest.description + $"\n\nNagroda: {quest.xpReward} XP";
        acceptButton.gameObject.SetActive(true);
        completeButton.gameObject.SetActive(false);
        panel.SetActive(true);
    }

    public void ShowQuestComplete(QuestData quest, Action onComplete)
    {
        onCompleteCallback = onComplete;
        questNameText.text = quest.questName;
        descriptionText.text = "Znalazłeś " + quest.targetItem.itemName + "! Odbierz nagrodę.";
        acceptButton.gameObject.SetActive(false);
        completeButton.gameObject.SetActive(true);
        panel.SetActive(true);
    }

    public void ShowMessage(string message)
    {
        questNameText.text = "";
        descriptionText.text = message;
        acceptButton.gameObject.SetActive(false);
        completeButton.gameObject.SetActive(false);
        panel.SetActive(true);
    }

    public void Hide() => panel.SetActive(false);

    private void OnAcceptClicked()
    {
        Hide();
        onAcceptCallback?.Invoke();
    }

    private void OnCompleteClicked()
    {
        Hide();
        onCompleteCallback?.Invoke();
    }
}
