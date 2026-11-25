using UnityEngine;

// StateMachineBehaviour: va pegado a un ESTADO del Animator Controller, no a un GameObject.
// Cuando el Animator entra al estado arranca a contar; pasados "delay" segundos dispara
// el trigger, y la transicion que escucha ese trigger cambia de animacion.
// Ej: en el fragmento del living, Matias respira (Breathing Idle) y a los 5 s pasa a
// Standing W_Briefcase Idle.
public class TriggerAfterTime : StateMachineBehaviour
{
    [Tooltip("Segundos que se queda en este estado antes de disparar el trigger.")]
    [SerializeField] private float delay = 5f;

    [Tooltip("Nombre del parametro Trigger del Animator.")]
    [SerializeField] private string triggerName = "Briefcase";

    private float _elapsed;
    private bool _fired;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _elapsed = 0f;
        _fired = false;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_fired) return;

        _elapsed += Time.deltaTime;
        if (_elapsed >= delay)
        {
            animator.SetTrigger(triggerName);
            _fired = true;
        }
    }
}
