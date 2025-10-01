using UnityEngine;

namespace RyansFunctions
{
    [CreateAssetMenu(fileName = "New Card", menuName = "Card")]
    public class Card : ScriptableObject
    {
        public string cardName;
        public string text;
        public CardType cardType;
        public int[] clothes;
        public Color[] colors;
        public Sprite customImage;

        public enum CardType
        {
            Summon, Action
        }
        public enum DamageType
        {
            Physical, Magical
        }

    }
}

