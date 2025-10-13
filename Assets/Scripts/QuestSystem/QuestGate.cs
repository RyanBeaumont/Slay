using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]public class Quest
{
    public string name;
    [TextArea] public string description;
    public float progress = 0f;

}
public class QuestGate : ChainedInteractable
{
    public Quest requiredQuest;
    public bool giveQuest = true;
    public List<Dialog> incompleteDialog = new List<Dialog>();

    public override void Interact()
    {
        if (active && requiredQuest != null && !OverworldController.Instance.IsQuestComplete(requiredQuest.name))
        {
            FindFirstObjectByType<DialogBox>().StartDialog(incompleteDialog);
            if (!OverworldController.Instance.IsQuestActive(requiredQuest.name) && giveQuest == true)
            {
                OverworldController.Instance.StartQuest(requiredQuest); //Start the quest if it's not in the quest list
                GameObject canvas = GameObject.Find("MainUI/ItemGroup");
                GameObject popup = Instantiate(Resources.Load<GameObject>("Popup"), canvas.transform);
                TMP_Text t = popup.transform.Find("Title").GetComponent<TMP_Text>();
                t.text = $"New quest: {requiredQuest.name}";
                Destroy(popup.transform.Find("ClothingUI").gameObject);
                Destroy(popup, 6f);
            }
            blockInteraction = true;
            return; // stop chain
        }

        blockInteraction = false;
        CallNext(); // allow further interactions
    }
}



