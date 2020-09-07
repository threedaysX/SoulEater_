using Necromancy.UI;
using UnityEngine;

[RequireComponent(typeof(TextRendererParticleSystem))]
public class TextParticlePopup : MonoBehaviour, IDamageGenerator, ITextGenerator
{
    public TextRendererParticleSystem textRenderer;

    public void SetupDamage(bool isCritical, int damageAmount, Color color, float? startSize = null)
    {
        string message = damageAmount.ToString();
        if (isCritical)
        {
            message += "!";
        }
        textRenderer.SpawnParticle(message, color, transform.position, startSize);
    }

    public void SetupTextMessage(string message, Color color, float? startSize = null)
    {
        textRenderer.SpawnParticle(message, color, transform.position, startSize);
    }
}
