using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using System;
[Serializable] public class Dialog
{
    [TextArea] public string text;
    public string name;
    public int character = -1;
    public string pose = "Idle";
    public bool right = false;
    public Sprite sprite;
}
public class DialogBox : MonoBehaviour
{
    public TextMeshProUGUI textComponent;
    public List<Dialog> dialog = new List<Dialog>();
    Canvas canvas;
    public float textSpeed = 0.05f;
    Animator anim;
    Transform model;
    SpriteRenderer spriteRenderer;
    void Start()
    {
        canvas = GetComponent<Canvas>();
        textComponent.text = "";
        canvas.enabled = false;
        anim = GetComponent<Animator>();
        model = transform.Find("CharacterContainer/SkeletonContainer");
        spriteRenderer = transform.Find("CharacterContainer/SpriteContainer").GetComponent<SpriteRenderer>();
    }


    void Update()
    {
        if ((Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.E)) && canvas.enabled)
        {
            if (textComponent.text == dialog[0].text)
                AdvanceDialog();
            else
            {
                StopAllCoroutines();
                textComponent.text = dialog[0].text;
            }
        }
    }

    public void BasicDialog(string[] messages)
    {
        dialog.Clear();
        foreach (string txt in messages)
        {
            dialog.Add(new Dialog { text = txt });
            StartDialog();
        }
    }

    public void StartDialog()
    {
        anim.SetBool("DialogActive", true);
        textComponent.text = "";
        SpawnCharacter(dialog[0].right, dialog[0].character, dialog[0].sprite, dialog[0].pose);
        StartCoroutine(TypeLine());
        canvas.enabled = true;
        PartyManager pm = FindFirstObjectByType<PartyManager>();
        if(pm != null){ pm.canMove = false; }
        if (GameObject.Find("GameManager") != null)
        {
            GameManager.Instance.runTimer = false;
        }
    }
    

    void AdvanceDialog()
    {
        dialog.RemoveAt(0);
        textComponent.text = "";
        if (dialog.Count > 0)
        {
            SpawnCharacter(dialog[0].right, dialog[0].character, dialog[0].sprite, dialog[0].pose);
            canvas.enabled = true;
            anim.SetBool("Right", dialog[0].right);

            StartCoroutine(TypeLine());
        }
        else
        {
            anim.SetBool("DialogActive", false);
            Invoke("DisableCanvas", 0.2f);
            spriteRenderer.enabled = false;
            foreach (Transform child in model) { Destroy(child.gameObject); }
            if (GameObject.Find("GameManager") != null)
            {
                GameManager.Instance.runTimer = true;
            }
        }
    }

    void SpawnCharacter(bool right, int character, Sprite sprite, string pose)
    {
        spriteRenderer.sprite = null;
        spriteRenderer.enabled = false;
        foreach (Transform child in model) { Destroy(child.gameObject); }
        anim.SetBool("Right", right);
        if (sprite != null)
        {
            spriteRenderer.flipX = right;
            spriteRenderer.sprite = sprite;
            spriteRenderer.enabled = true;
        }
        else if (character != -1)
        {
            if (character < OverworldController.Instance.yourTeam.Count)
            {
                Outfit outfit = OverworldController.Instance.yourTeam[character].equippedOutfit;
                GameObject skeleton = ClothingRegistry.Instance.SpawnCharacter(character, outfit, model);
                if (!right) skeleton.transform.localScale = new Vector3(skeleton.transform.localScale.x * -1, skeleton.transform.localScale.y, skeleton.transform.localScale.z);
                skeleton.GetComponent<CustomAnimator>().Play(pose, 0, canAutoUpdate:false);
            }

        }
    }

    void DisableCanvas()
    {
        canvas.enabled = false;
        PartyManager pm = FindFirstObjectByType<PartyManager>();
        if (pm != null) { pm.canMove = true; }
    }

    private IEnumerator TypeLine()
    {
        foreach (char c in dialog[0].text.ToCharArray())
        {
            textComponent.text += c;
            yield return new WaitForSeconds(textSpeed);
        }
    }
}
