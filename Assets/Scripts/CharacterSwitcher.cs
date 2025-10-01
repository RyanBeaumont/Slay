using Unity.VisualScripting;
using UnityEngine;

public class CharacterSwitcher : MonoBehaviour
{
    public int characterIndex = 0;
    Animator animator;
    public GameObject[] _characters;

    void Start()
    {
        SetCharacter();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            characterIndex++;
            if (characterIndex >= _characters.Length) characterIndex = 0;
            SetCharacter();
            var r = Mathf.RoundToInt(Random.Range(1, 3));
            animator.Play($"Pose{r}");
        }
    }

    void SetCharacter()
{
    for (int i = 0; i < _characters.Length; i++)
    {
        _characters[i].SetActive(i == characterIndex);
    }
}
}
