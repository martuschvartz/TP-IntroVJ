using _Project.Scripts.Utility;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

/*
 * MIRA DEL HUD
 *
 * Va en la Image "Crosshair" del Canvas HUD (dentro del prefab Player).
 * Muestra un puntito chiquito en el centro de la pantalla; al mirar algo
 * interactuable lo cambia por una manito, y si es una pista, por la libreta.
 */
[RequireComponent(typeof(Image))]
public class CrosshairUI : MonoBehaviour
{
    [Header("Sin nada interactuable")]
    [SerializeField] private Sprite dotSprite;
    [SerializeField] private Vector2 dotSize = new Vector2(12f, 12f);

    [Header("Interactuable común")]
    [SerializeField] private Sprite handSprite;
    [SerializeField] private Vector2 handSize = new Vector2(64f, 64f);

    [Header("Pista")]
    [FormerlySerializedAs("notebookSprite")]
    [SerializeField] private Sprite clueSprite;
    [FormerlySerializedAs("notebookSize")]
    [SerializeField] private Vector2 clueSize = new Vector2(64f, 64f);

    private Image _image;
    private RectTransform _rect;

    private void Awake()
    {
        _image = GetComponent<Image>();
        _rect = (RectTransform)transform;
        ShowIcon(CrosshairIcon.Dot);
    }

    private void OnEnable()
    {
        GameEvents.CrosshairIconChanged += ShowIcon;
    }

    private void OnDisable()
    {
        GameEvents.CrosshairIconChanged -= ShowIcon;
    }

    private void ShowIcon(CrosshairIcon icon)
    {
        switch (icon)
        {
            case CrosshairIcon.Hand:
                _image.sprite = handSprite;
                _rect.sizeDelta = handSize;
                break;
            case CrosshairIcon.Clue:
                _image.sprite = clueSprite;
                _rect.sizeDelta = clueSize;
                break;
            default:
                _image.sprite = dotSprite;
                _rect.sizeDelta = dotSize;
                break;
        }
    }
}
