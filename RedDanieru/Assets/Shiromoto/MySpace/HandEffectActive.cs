using UnityEngine;

public class HandEffectActive : MonoBehaviour
{
    private ParticleSystem effect;

    private void Awake()
    {
        effect = GetComponent<ParticleSystem>();
    }

    public void HandsOn()
    {
        effect.Play();
    }

    public void HandsOff()
    {
        effect.Stop();
    }
}
