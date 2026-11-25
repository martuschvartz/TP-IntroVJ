using System.Collections;
using _Project.Scripts.Managers;
using _Project.Scripts.Utility;
using UnityEngine;

// Finales (Carcel / "No carcel"): boton "Volver a inicio" abajo en el medio,
// que aparece con un fade despues de que termina la animacion de la reja y lleva al menu.
// Va en cualquier objeto de la escena (esta en la Main Camera).
public class EndingMenuButton : MonoBehaviour
{
    [Tooltip("Segundos antes de que aparezca el boton (para no tapar la reja).")]
    [SerializeField] private float showDelay = 5f;
    [SerializeField] private float fadeDuration = 1f;

    private CanvasGroup _group;

    private IEnumerator Start()
    {
        UIFactory.EnsureEventSystem();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        var canvas = UIFactory.CreateOverlayCanvas("EndingMenuCanvas", 100);
        _group = canvas.GetComponent<CanvasGroup>();
        _group.alpha = 0f;
        _group.interactable = false;
        _group.blocksRaycasts = false;

        var button = UIFactory.CreateButton(canvas.transform, "VOLVER A INICIO", UIFactory.DarkColor,
            new Vector2(380f, 86f), 30f, GoToMenu);
        var rt = (RectTransform)button.transform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0f, 90f);

        yield return new WaitForSeconds(showDelay);

        for (float t = 0f; t < fadeDuration; t += Time.unscaledDeltaTime)
        {
            _group.alpha = Mathf.SmoothStep(0f, 1f, t / fadeDuration);
            yield return null;
        }
        _group.alpha = 1f;
        _group.interactable = true;
        _group.blocksRaycasts = true;
    }

    private void GoToMenu()
    {
        GameProgress.Reset();
        SceneAdministrator.Load(SceneName.Menu);
    }
}
