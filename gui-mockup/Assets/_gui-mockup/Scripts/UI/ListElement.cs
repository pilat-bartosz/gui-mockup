using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class ListElement : MonoBehaviour
{
    [Tooltip("Should be in the order of DataItem.CategoryType")] [SerializeField]
    private List<Sprite> _badgeSprites;

    [Header("References")]
    [SerializeField] private Image _badgeImage;
    [SerializeField] private GameObject _glowObject;
    [SerializeField] private TextMeshProUGUI _numberText;
    [SerializeField] private TextMeshProUGUI _text;

    private void Awake()
    {
        Assert.IsNotNull(_badgeSprites);
        Assert.IsTrue(_badgeSprites.Count == (int)DataItem.CategoryType.LENGTH,
            "Badge images should be assigned properly"
        );
    }

    public void UpdateElement(DataItem.CategoryType type, string number, string text, bool isSpecial)
    {
        _badgeImage.sprite = _badgeSprites[(int)type];
        _numberText.text = number;
        _text.text = text;
        
        _glowObject.SetActive(isSpecial);
        
        gameObject.SetActive(true);
    }

    public void Disable()
    {
        gameObject.SetActive(false);
    }
}