using TMPro;
using UnityEngine;

public class QuestReward : ChainedInteractable
{
    public int cash = 0;
    public int item = -1;
    public override void Interact()
    {
        if (active)
        {
            OverworldController.Instance.cash += cash;
            if (item > -1 && ClothingRegistry.Instance.clothing.Count > item)
            {
                OverworldController.Instance.yourClothes.Add(item);
            }

            GameObject canvas = GameObject.Find("MainUI/ItemGroup");
            GameObject popup = Instantiate(Resources.Load<GameObject>("Popup"), canvas.transform);
            TMP_Text t = popup.transform.Find("Title").GetComponent<TMP_Text>();
            t.text = "";
            if (item > -1)
            {
                UIClothing uic = popup.transform.Find("ClothingUI").GetComponent<UIClothing>();
                ClothingStats stats = ClothingRegistry.Instance.GetStats(new int[] { item }, new ClothingStats());
                uic.index = item;
                uic.colorButton.enabled = false;
                uic.countText.enabled = false;
                uic.Spawn();
                t.text = $"New item: {stats.name} \n";
            }
            else
            {
                Destroy(popup.transform.Find("ClothingUI").gameObject);
            }
            
            if(cash > 0)
            {
                t.text = $"Acquired cash: {cash}";
            }
            Destroy(popup, 6f);
            

            CallNext();
        }
    }
}
