using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardDisplay : MonoBehaviour
{
    public TMP_Text nameText;
    public TMP_Text descriptionText;
    public Image artworkImage;
    public Image backgroundImage;

    public void SetCard(CardData cardData)
    {
        nameText.text = cardData.cardName;
        descriptionText.text = cardData.cardDescription;
        artworkImage.sprite = cardData.artwork;
        backgroundImage.sprite = cardData.background;
    }
}
