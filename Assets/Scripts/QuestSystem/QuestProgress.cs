using UnityEngine;
using TMPro;
using System.Collections.Generic;
public class QuestProgress : ChainedInteractable
{
    public string quest;
    public List<Dialog> failsafeText;
    public float progressToAdd;
    public override void Interact()
    {
        if (active && GameObject.Find("Dialog"))
        {
            Debug.Log("Quest Interact");
            if (OverworldController.Instance.IsQuestActive(quest))
            {
                float p = OverworldController.Instance.CompleteQuest(quest, progressToAdd);
                GameObject canvas = GameObject.Find("MainUI/ItemGroup");
                GameObject popup = Instantiate(Resources.Load<GameObject>("Popup"), canvas.transform);
                TMP_Text t = popup.transform.Find("Title").GetComponent<TMP_Text>();
                if (p >= 1)
                {
                    t.text = $"Completed quest: {quest}";
                }
                else
                {
                    t.text = $"{quest} ({Mathf.Round(p*100)}%)";
                }
                Destroy(popup.transform.Find("ClothingUI").gameObject);
                Destroy(popup, 6f);
                blockInteraction = false;
                CallNext();
            }
            else
            {
                blockInteraction = true;
                FindFirstObjectByType<DialogBox>().StartDialog(failsafeText);
            }
            
        }
    }
}
