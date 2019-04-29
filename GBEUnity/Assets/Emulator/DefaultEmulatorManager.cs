using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;


namespace Emulator
{
    public class DefaultEmulatorManager : MonoBehaviour
    {
        public string fileName;
        public Renderer screenRenderer;

        public ConsoleBase Emulator
        {
            get;
            private set;
        }

        private Dictionary<KeyCode, ConsoleBase.Button> _keyMapping;

        // Use this for initialization
        void Start()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;

            // Init Keyboard mapping
            _keyMapping = new Dictionary<KeyCode, ConsoleBase.Button>
            {
                {KeyCode.UpArrow, ConsoleBase.Button.Up},
                {KeyCode.DownArrow, ConsoleBase.Button.Down},
                {KeyCode.LeftArrow, ConsoleBase.Button.Left},
                {KeyCode.RightArrow, ConsoleBase.Button.Right},
                {KeyCode.Z, ConsoleBase.Button.A},
                {KeyCode.X, ConsoleBase.Button.B},
                {KeyCode.Space, ConsoleBase.Button.Start},
                {KeyCode.LeftShift, ConsoleBase.Button.Select}
            };

            // Load emulator
            IVideoOutput drawable = new DefaultVideoOutput();
            IAudioOutput audio = GetComponent<AudioManager>();
            Emulator = new Console(drawable,audio);
            screenRenderer.material.mainTexture = ((DefaultVideoOutput)Emulator.Video).Texture;
            gameObject.GetComponent<AudioSource>().enabled = false;
            StartCoroutine(LoadRom(fileName));
        }

        void Update()
        {
            // Input
            foreach (var entry in _keyMapping)
            {
                if (Input.GetKeyDown(entry.Key))
                    Emulator.SetInput(entry.Value, true);
                else if (Input.GetKeyUp(entry.Key))
                    Emulator.SetInput(entry.Value, false);
            }

            if (!Input.GetKeyDown(KeyCode.T)) return;
            var screenshot = ((DefaultVideoOutput)Emulator.Video).Texture.EncodeToPNG();
            File.WriteAllBytes("./screenshot.png", screenshot);
            Debug.Log("Screenshot saved.");
        }



        private IEnumerator LoadRom(string filename)
        {
            var path = filename;
            Debug.Log("Loading ROM from " + path + ".");

            if (!File.Exists(path))
            {
                Debug.LogError($"File couldn't be found. {path}");
                yield break;
            }
            Emulator.LoadRom(path);
            StartCoroutine(Run());
        }

        private IEnumerator Run()
        {
            gameObject.GetComponent<AudioSource>().enabled = true;
            while (true)
            {
                Emulator.RunNextStep();

                yield return null;
            }
        }
    }


}
