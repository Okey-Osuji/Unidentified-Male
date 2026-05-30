using UnityEngine;

public class playersoundeffects : MonoBehaviour
{
    public AudioClip[] audioClips; 
    private AudioSource audioSource;
    public enum SoundEffectType { Walk, Eat, pickup }
    public void PlaySoundEffect(SoundEffectType type)
    {
        if (audioSource == null || audioClips == null || audioClips.Length == 0)
        {
            Debug.LogWarning("AudioSource or AudioClips not set up properly.");
            return;
        }

        AudioClip clipToPlay = null;

        switch (type)
        {
            case SoundEffectType.Walk:
                clipToPlay = audioClips[0]; // Assuming the first clip is for walking
                break;
            case SoundEffectType.Eat:
                clipToPlay = audioClips[1]; // Assuming the second clip is for eating
                break;
            case SoundEffectType.pickup:
                clipToPlay = audioClips[2]; // Assuming the third clip is for picking up items
                break;
            default:
                Debug.LogWarning("Unknown sound effect type.");
                return;
        }

        if (clipToPlay != null)
        {
            audioSource.PlayOneShot(clipToPlay);
        }
    }
}