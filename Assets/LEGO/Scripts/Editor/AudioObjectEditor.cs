using Unity.LEGO.Minifig;
using UnityEngine;
using UnityEditor;
using UnityEngine.Audio;

namespace Unity.LEGO.EditorExt
{
    [CustomEditor(typeof(AudioObject))]
    [CanEditMultipleObjects]
    public class AudioObjectEditor : UnityEditor.Editor
    {
        private static bool _overrideInspectorGUI = true;//enables/disables customEditor over AudioObject script
        private GUIStyle _guiStyle = new GUIStyle();
        private int _whichPreviewClip = -1;

        public override void OnInspectorGUI()
        {
            if (!_overrideInspectorGUI)
            {
                base.OnInspectorGUI();
                return;
            }

            _guiStyle.fontStyle = FontStyle.Bold;
            _guiStyle.fontSize = 12;

            AudioObject myTarget = (AudioObject)target;

            EditorUtility.SetDirty(myTarget);

            DrawObject(ref myTarget);
        }

        private void DrawObject(ref AudioObject audioObject)
        {

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("AudioObject Editor: " + audioObject.name, GUILayout.Width(400));
            EditorGUILayout.EndHorizontal();


            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Play", GUILayout.Width(50)))
            {
                _whichPreviewClip = audioObject.AudioObjectPlay();

            }

            if (GUILayout.Button("Stop", GUILayout.Width(50)))
            {
                audioObject.AudioObjectStop();
            }

            if (GUILayout.Button("Stop All Sounds", GUILayout.Width(100)))
            {
                audioObject.AudioObjectStop();
            }


            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            // audioClips
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("AudioClips", GUILayout.Width(200));
            EditorGUILayout.EndHorizontal();

            if (audioObject.audioClips == null)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Add Clip", GUILayout.Width(115)))
                {

                    audioObject.audioClips = new AudioClip[] { };

                    System.Array.Resize(ref audioObject.audioClips, audioObject.audioClips.Length + 1);

                }

                EditorGUILayout.EndHorizontal();
                return;
            }

            if (audioObject.audioClips.Length > 0)
            {

                for (int i = 0; i < audioObject.audioClips.Length; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    //EditorGUILayout.LabelField("AudioClip", GUILayout.Width(200));
                    audioObject.audioClips[i] = (AudioClip)EditorGUILayout.ObjectField("", audioObject.audioClips[i], typeof(AudioClip), true, GUILayout.Width(300));


                    if (_whichPreviewClip == i)
                    {
                        EditorGUILayout.LabelField("•   ", GUILayout.Width(20));
                    }
                    else
                    {
                        EditorGUILayout.LabelField("    ", GUILayout.Width(20));
                    }

                    if (i == audioObject.audioClips.Length - 1)
                    {
                        if (GUILayout.Button("Delete Clip", GUILayout.Width(115)))
                        {
                            System.Array.Resize(ref audioObject.audioClips, audioObject.audioClips.Length - 1);    //resize array
                        }
                    }

                    EditorGUILayout.EndHorizontal();
                }
            }


            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Clip", GUILayout.Width(115)))
            {

                System.Array.Resize(ref audioObject.audioClips, audioObject.audioClips.Length + 1);    //resize array

            }

            EditorGUILayout.EndHorizontal();

            /////////////// audiomixergroup
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);


            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Audio Mixer Group", GUILayout.Width(140));
            audioObject.audioMixerGroup = (AudioMixerGroup)EditorGUILayout.ObjectField("", audioObject.audioMixerGroup, typeof(AudioMixerGroup), true, GUILayout.Width(295));

            EditorGUILayout.EndHorizontal();

            /////////////// additional parameters
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Sound Type", GUILayout.Width(140));
            audioObject.audioObjectType = (AudioObjectType)EditorGUILayout.EnumPopup(audioObject.audioObjectType, GUILayout.Width(295));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Playback", GUILayout.Width(140));
            audioObject.playback = (Playback)EditorGUILayout.EnumPopup(audioObject.playback, GUILayout.Width(295));
            EditorGUILayout.EndHorizontal();


            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Randomize Pitch", GUILayout.Width(140));
            audioObject.randomPitch = EditorGUILayout.Toggle("", audioObject.randomPitch, GUILayout.Width(20));
            if (audioObject.randomPitch)
            {
                EditorGUILayout.LabelField("Min", GUILayout.Width(40));
                audioObject.minPitch = EditorGUILayout.FloatField("", audioObject.minPitch, GUILayout.Width(50));
                EditorGUILayout.LabelField("Max", GUILayout.Width(40));
                audioObject.maxPitch = EditorGUILayout.FloatField("", audioObject.maxPitch, GUILayout.Width(50));
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Randomize Volume", GUILayout.Width(140));
            audioObject.randomVolume = EditorGUILayout.Toggle("", audioObject.randomVolume, GUILayout.Width(20));
            if (audioObject.randomVolume)
            {
                EditorGUILayout.LabelField("Min", GUILayout.Width(40));
                audioObject.minVolume = EditorGUILayout.FloatField("", audioObject.minVolume, GUILayout.Width(50));
                EditorGUILayout.LabelField("Max", GUILayout.Width(40));
                audioObject.maxVolume = EditorGUILayout.FloatField("", audioObject.maxVolume, GUILayout.Width(50));
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Loop", GUILayout.Width(140));
            audioObject.loop = EditorGUILayout.Toggle("", audioObject.loop, GUILayout.Width(20));
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Enable Fades", GUILayout.Width(140));
            audioObject.fadesEnabled = EditorGUILayout.Toggle("", audioObject.fadesEnabled, GUILayout.Width(20));
            EditorGUILayout.EndHorizontal();


            if (audioObject.fadesEnabled)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Default Fade In Time", GUILayout.Width(140));
                audioObject.fadeInTime = EditorGUILayout.FloatField("", audioObject.fadeInTime, GUILayout.Width(100));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Default Fade Out Time", GUILayout.Width(140));
                audioObject.fadeOutTime = EditorGUILayout.FloatField("", audioObject.fadeOutTime, GUILayout.Width(100));
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Limit Sound Voices", GUILayout.Width(140));
            audioObject.limitNumberOfVoices = EditorGUILayout.Toggle("", audioObject.limitNumberOfVoices, GUILayout.Width(20));
            if (audioObject.limitNumberOfVoices)
            {
                EditorGUILayout.LabelField("Max Voices", GUILayout.Width(80));
                audioObject.maximumNumberOfVoices = EditorGUILayout.IntField("", audioObject.maximumNumberOfVoices, GUILayout.Width(20));
                EditorGUILayout.LabelField("Play Newest Request", GUILayout.Width(160));
                audioObject.playNewestVoice = EditorGUILayout.Toggle("", audioObject.playNewestVoice, GUILayout.Width(20));

            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Reset Parameters", GUILayout.Width(130)))
            {
                audioObject.ResetCurrentAudioObject();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);


        }
    }
}