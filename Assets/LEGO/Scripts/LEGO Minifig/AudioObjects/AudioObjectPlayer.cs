using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Unity.LEGO.Minifig
{
    [ExecuteAlways]
    public class AudioObjectPlayer : MonoBehaviour
    {
        public delegate void SoundObjectCallbackFunction();

        private const float timeToWaitAfterSoundFinishesBeforeDestroyingGameObject = 0.1f;
        public AudioObject AudioObject { get; set; }
        private GameObject _soundGameObject;
        private AudioSource _audioSource;
        private AudioClip _audioClip;
        private IEnumerator _playingAudioMonitoringCoroutine;
        private IEnumerator _playingMonitoringCallbackCoroutine;

        public int PlaySound(int clipNumber, float spatialBlend = 0.0f, GameObject gameObject = null,
            SoundObjectCallbackFunction soundObjectCallbackFunction = null)
        {
            _soundGameObject = this.gameObject;
            _audioSource = _soundGameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
            _audioSource.clip = AudioObject.audioClips[clipNumber];
            _audioSource.loop = AudioObject.loop;
            _audioSource.spatialBlend = 0.0f;

            if (AudioObject.audioMixerGroup == null || AudioObject.audioMixerGroup.name == "")
            {
                Debug.LogWarning($"No audio mixer set for {AudioObject.name}");
            }
            else
            {
                _audioSource.outputAudioMixerGroup = AudioObject.audioMixerGroup;
            }

            if (AudioObject.randomPitch)
            {
                _audioSource.pitch = Random.Range(AudioObject.minPitch, AudioObject.maxPitch);
            }

            if (AudioObject.randomVolume)
            {
                _audioSource.volume = Random.Range(AudioObject.minVolume, AudioObject.maxVolume);
            }

            if (!AudioObject.loop)
            {
                _audioSource.loop = false;
            }
            else
            {
                if (AudioObject.audioClips.Length == 1)
                {
                    _audioSource.loop = true;
                }
                else
                {
                    //number of clips is more than 1
                    if (AudioObject.audioObjectType == AudioObjectType.Music)
                    {
                        _audioSource.loop = false;
                    }
                    else
                    {
                        _audioSource.loop = true;
                    }
                }

            }

            if (AudioObject.fadesEnabled)
            {
                float tempVolume = _audioSource.volume;
                _audioSource.volume = 0.0f;
                _audioSource.Play();
                FadeInSound(AudioObject.fadeInTime, tempVolume);
            }
            else
            {
                _audioSource.Play();

            }

            if (!AudioObject.loop)
            {
                AudioClip audioClip = AudioObject.audioClips[clipNumber];
                _playingAudioMonitoringCoroutine = MonitorAudioSource(audioClip.length);
                StartCoroutine(_playingAudioMonitoringCoroutine);
            }

            if (soundObjectCallbackFunction != null)
            {
                AudioClip audioClip = AudioObject.audioClips[clipNumber];
                _playingMonitoringCallbackCoroutine = MonitorCallback(audioClip.length, soundObjectCallbackFunction);
                StartCoroutine(_playingMonitoringCallbackCoroutine);
            }

            return clipNumber;
        }

        public void StopSound()
        {
            //Debug.Log("SoundObjectPlayer: StopSound " + this.gameObject.name);

            if (_audioSource != null)
            {
                _audioSource.Stop();
            }

            AudioObject.RemoveIndexFromCurrentSoundObjectsList();
            if (Application.isPlaying)
            {
                Destroy(_soundGameObject);
            }
            else
            {
                DestroyImmediate(_soundGameObject);
            }

        }


        public void FadeOutSound(float numberOfSeconds = 2.0f)
        {
            //Debug.Log("SoundObjectPlayer: FadeSound " + this.gameObject.name);

            if (_audioSource != null)
            {
                //_audioSource.Stop();

                StartCoroutine(StartFade(_audioSource, numberOfSeconds, 0.0f, StopSound));
            }


        }

        public void FadeInSound(float numberOfSeconds = 2.0f, float targetVolume = 1.0f)
        {
            if (_audioSource != null)
            {
                //_audioSource.Stop();

                StartCoroutine(StartFadeIn(_audioSource, numberOfSeconds, targetVolume));
            }
        }

        private IEnumerator MonitorAudioSource(float audioClipLength)
        {
            if (_audioSource.isPlaying)
            {
                yield return new WaitForSeconds(
                    audioClipLength + timeToWaitAfterSoundFinishesBeforeDestroyingGameObject);
                StopSound();
            }
        }

        private IEnumerator MonitorCallback(float audioClipLength,
            SoundObjectCallbackFunction soundObjectCallbackFunction)
        {
            if (_audioSource.isPlaying)
            {
                yield return new WaitForSeconds(audioClipLength - 0.01f);
                soundObjectCallbackFunction();
            }
        }

        public static IEnumerator StartFade(AudioSource audioSource, float duration, float targetVolume,
            UnityEngine.Events.UnityAction FadeReturn)
        {
            float currentTime = 0f;
            float currentVol;
            if (duration < 0.001f)
            {
                duration = 0.001f;
            }

            currentVol = audioSource.volume;
            currentVol = Mathf.Pow(10f, currentVol / 20f);
            float targetValue = Mathf.Clamp(targetVolume, 0.001f, 1f);

            while (currentTime < duration)
            {
                currentTime += Time.deltaTime;
                float newVol = Mathf.Lerp(currentVol, targetValue, currentTime / (duration * 10.0f));
                audioSource.volume = Mathf.Log10(newVol) * 20f;
                yield return null;
            }

            FadeReturn();

            yield break;
        }

        public static IEnumerator StartFadeIn(AudioSource audioSource, float duration, float targetVolume)
        {
            float currentTime = 0f;
            float currentVol = audioSource.volume;
            if (duration < 0.001f)
            {
                duration = 0.001f;
            }

            while (currentTime < duration)
            {
                currentTime += Time.deltaTime;
                audioSource.volume = Mathf.Lerp(currentVol, targetVolume, currentTime / duration);
                yield return null;
            }

            yield break;
        }
    }
}