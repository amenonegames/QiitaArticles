namespace Utils.Editor
{
    using UnityEngine;
    using UnityEditor;
    using UnityEngine.Audio;

    public class GenerateCodeWindow : EditorWindow
    {
        private AudioMixer audioMixer;

        [MenuItem("Tools/GenerateCode")]
        private static void OpenWindow()
        {
            GetWindow<GenerateCodeWindow>("Generate Code");
        }

        private void OnGUI()
        {
            GUILayout.Label("Attach Audio Mixer", EditorStyles.boldLabel);
            audioMixer = (AudioMixer)EditorGUILayout.ObjectField("Audio Mixer", audioMixer, typeof(AudioMixer), false);

            if (GUILayout.Button("Sync To Mixer") && audioMixer != null)
            {
                FetchMixerData fetchMixerData = new(audioMixer);
                
                if (fetchMixerData != null)
                {
                    var parameters =fetchMixerData.SyncToMixer();
                    
                    AudioMixerControllerGenerator generator = new(parameters);
                    generator.Generate();
                }
                else
                {
                    Debug.LogError("FetchMixerData component not found in the scene.");
                }
            }
        }
    }

}