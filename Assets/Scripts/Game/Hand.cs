using System.Collections;
using UnityEngine.UI;
using UnityEngine;

public class Hand : MonoBehaviour
{
    [SerializeField] private Image[] cardImages;
    [SerializeField] private Player _player;
    private void Start()
    {
        StartCoroutine(ShowCards(_player,12f));
    }

    private IEnumerator ShowCards(Player player, float delay)
    {
        yield return new WaitForSeconds(delay);
        for (int i = 0; i < cardImages.Length; i++)
        {
            string path = $"Sprites/Cards/{(int)player.Cards[i].Suit}_{(int)player.Cards[i].Value}";
            Sprite sprite = Resources.Load<Sprite>(path);
            if (sprite == null)
            {
                Debug.LogError($"Sprite not found at path: {path}");
            }
            else
            {
                cardImages[i].sprite = sprite;
            }
        }
    }

}