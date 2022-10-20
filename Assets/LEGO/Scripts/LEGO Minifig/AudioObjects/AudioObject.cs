using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Unity.LEGO.Minifig
{
    public enum AudioObjectType
    {
        SFX,
        Ambience,
        Dialog,
        Music
    };

    public enum Playback
    {
        Random,
        Sequential
    };

    [CreateAssetMenu(fileName = "AudioObject", menuName = "Audio Objects/Audio Object")]
    [ExecuteAlways]
    public class AudioObject : ScriptableObject
    {
        //static private Dictionary<string, string> currentAudioPlayingAudioObjects;
        public AudioClip[] audioClips;
        public AudioMixerGroup audioMixerGroup;
        public bool loop = false;
        public bool randomPitch = false;
        public float minPitch = 0.9f;
        public float maxPitch = 1.1f;
        public bool randomVolume = false;
        public float minVolume = 0.5f;
        public float maxVolume = 1.0f;
        public bool limitNumberOfVoices = false;
        public int maximumNumberOfVoices = 1;
        public bool fadesEnabled = false;
        public float fadeInTime = 0.0f;
        public float fadeOutTime = 0.0f;
        public int lastClipNumber = -1;

        public Playback playback;

        public AudioObjectType audioObjectType;
        public bool playNewestVoice = true;
        private AudioObjectPlayer _audioObjectPlayer;
        private GameObject _audioGameObject;
        private static List<AudioObject> _currentAudioObjects;

        public int AudioObjectPlay()
        {
            //Debug.Log("AudioObjectPlay");

            bool skipPlayback = false;
            //make sure that there are clips to play
            if (audioClips == null)
            {
                Debug.LogWarning("No clips in this audio object.");
                return -1;
            }

            if (audioClips.Length < 1)
            {
                Debug.LogWarning("No clips in this audio object.");
                return -1;
            }

            if (audioClips[0] == null)
            {
                Debug.LogWarning("No clips in this audio object.");
                return -1;
            }

            //if this is music - then we want to fade previous music tracks
            if (audioObjectType == AudioObjectType.Music)
            {
                FadeAllMusic();
            }

            //if we need to limit the voices, we check those first to make sure we aren't over the limit
            if (limitNumberOfVoices && _currentAudioObjects != null)
            {


                for (int i = _currentAudioObjects.Count - 1; i >= 0; i--)
                {
                    int numberOfCurrentAudioObjectInstances = 0;
                    if (_currentAudioObjects[i].name == this.name)
                    {
                        numberOfCurrentAudioObjectInstances++;
                        //Debug.Log("numberOfCurrentSoundObjectInstances: " + numberOfCurrentAudioObjectInstances);
                    }

                    if (numberOfCurrentAudioObjectInstances > maximumNumberOfVoices)
                    {
                        //Debug.Log("maximum voices reached: " + _currentSoundObjects[i].name);
                        if (playNewestVoice)
                        {

                            if (_currentAudioObjects[i].audioObjectType == AudioObjectType.Music)
                            {
                                _currentAudioObjects[i]._audioObjectPlayer.FadeOutSound();
                            }
                            else
                            {
                                _currentAudioObjects[i]._audioObjectPlayer.StopSound();
                            }
                        }
                        else
                        {
                            skipPlayback = true;
                        }
                    }
                }
            }

            if (skipPlayback)
            {
                return -1;
            }

            int clipNumber = lastClipNumber;

            if (playback == Playback.Random && audioClips.Length > 1)
            {
                while (clipNumber == lastClipNumber)
                {
                    clipNumber = Random.Range(0, audioClips.Length);
                }

            }
            else
            {
                clipNumber = lastClipNumber + 1;
                if (clipNumber >= audioClips.Length)
                {
                    clipNumber = 0;
                }
            }

            lastClipNumber = clipNumber;

            //check to see if clip exists
            if (audioClips[clipNumber] == null)
            {
                return -1;
            }


            //get ready to play by setting up a temporary game object, with a SoundObjectPlayer component
            _audioGameObject =
                new GameObject($"AudioObject - {audioClips[clipNumber].name} {Time.realtimeSinceStartup}");
            _audioObjectPlayer = _audioGameObject.AddComponent(typeof(AudioObjectPlayer)) as AudioObjectPlayer;
            _audioObjectPlayer.AudioObject = this;
            int whichAudioClip = -1;

            //play the clip, and return the number of the clip
            if (_audioObjectPlayer.AudioObject.audioObjectType == AudioObjectType.Music)
            {
                whichAudioClip = _audioObjectPlayer.PlaySound(clipNumber: clipNumber,
                    soundObjectCallbackFunction: delegate { MusicCallback(); });
            }
            else
            {
                whichAudioClip = _audioObjectPlayer.PlaySound(clipNumber: clipNumber);
            }

            if (_currentAudioObjects == null)
            {
                _currentAudioObjects = new List<AudioObject>();
            }

            _currentAudioObjects.Add(this);

            return whichAudioClip;
        }

        public void MusicCallback()
        {

            if (loop)
            {
                if (playback == Playback.Sequential)
                {
                    if (lastClipNumber < audioClips.Length - 1)
                    {
                        int whichAudioClip = AudioObjectPlay();
                    }
                    else
                    {
                        lastClipNumber = -1;
                        int whichAudioClip = AudioObjectPlay();
                    }
                }
                else
                {
                    //playback == Playback.Random
                    int whichAudioClip = AudioObjectPlay();

                }
            }
            else
            {
                //no loop
                if (lastClipNumber < audioClips.Length - 1)
                {
                    int whichAudioClip = AudioObjectPlay();
                }
            }

        }


        public void AudioObjectStop()
        {
            //Debug.Log("SoundObjectStop");
            if (audioClips == null)
            {
                Debug.LogWarning("No clips in this sound object.");
                return;
            }

            if (audioClips.Length < 1)
            {
                Debug.LogWarning("No clips in this sound object.");
                return;
            }

            _audioObjectPlayer.StopSound();
            RemoveIndexFromCurrentSoundObjectsList();
        }

        public void SoundObjectFadeOut(float numberOfSeconds)
        {
            //Debug.Log("SoundObjectFadeOut");
            if (audioClips == null)
            {
                Debug.LogWarning("No clips in this sound object.");
                return;
            }

            if (audioClips.Length < 1)
            {
                Debug.LogWarning("No clips in this sound object.");
                return;
            }

            _audioObjectPlayer.FadeOutSound(numberOfSeconds);

        }

        public static void StopAllSounds()
        {
            //Debug.Log("StopAllSounds");
            foreach (AudioObject audioObject in _currentAudioObjects)
            {
                audioObject._audioObjectPlayer.StopSound();
            }

            _currentAudioObjects = null;

        }


        public static void FadeAllMusic()
        {
            if (_currentAudioObjects == null)
            {
                return;
            }

            foreach (AudioObject audioObject in _currentAudioObjects)
            {
                if (audioObject.audioObjectType == AudioObjectType.Music)
                {
                    audioObject._audioObjectPlayer.FadeOutSound(0.5f);
                }
            }

        }


        public void RemoveIndexFromCurrentSoundObjectsList()
        {
            //remove the index if the voices were limited
            if (limitNumberOfVoices)
            {
                for (int i = 0; i < _currentAudioObjects.Count; i++)
                {
                    if (this.name == _currentAudioObjects[i].name)
                    {
                        _currentAudioObjects.RemoveAt(i);
                    }
                }
            }
        }

        public void ResetCurrentAudioObject()
        {
            // reset parameters on the current sound object
            if (_currentAudioObjects == null)
            {
                _currentAudioObjects = new List<AudioObject>();
            }

            _currentAudioObjects.Clear();
            this.randomVolume = false;
            this.minVolume = 0.5f;
            this.maxVolume = 1.0f;
            this.randomPitch = false;
            this.minPitch = 0.9f;
            this.maxPitch = 1.1f;
            this.loop = false;
            this.limitNumberOfVoices = false;
            this.maximumNumberOfVoices = 1;
            this.playNewestVoice = true;
            this.playback = Playback.Random;
            this.audioObjectType = AudioObjectType.SFX;

        }

        private void StopAudio()
        {
            _audioObjectPlayer.StopSound();
        }

//
// #if UNITY_EDITOR
//         /* Somatone testing code, might have use later */
//         public void EditorPlayClip(AudioClip editorAudioClip)
//         {
//             int startSample = 0;
//             System.Reflection.Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
//             System.Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
//             System.Reflection.MethodInfo method = audioUtilClass.GetMethod(
//                 "PlayClip",
//                 System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public,
//                 null,
//                 new System.Type[] { typeof(AudioClip), typeof(int), typeof(bool) },
//                 null
//             );
//             Debug.Log(method);
//             method?.Invoke(
//                 null,
//                 new object[] { editorAudioClip, startSample, loop }
//             );
//         }
//
//         /* Somatone testing code, might have use later */
//         public void EditorStopAllClips()
//         {
//
//             System.Reflection.Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
//             System.Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
//             System.Reflection.MethodInfo method = audioUtilClass.GetMethod(
//                 "StopAllClips",
//                 System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public,
//                 null,
//                 new System.Type[] { },
//                 null
//             );
//
//             Debug.Log(method);
//             method?.Invoke(
//                 null,
//                 new object[] { }
//             );
//         }
// #endif
    }
}