using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;


namespace Emulator
{
    public class DefaultEmulatorManager : MonoBehaviour
    {
        public string Filename;
        public Renderer ScreenRenderer;

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
            _keyMapping = new Dictionary<KeyCode, ConsoleBase.Button>();
            _keyMapping.Add(KeyCode.UpArrow, ConsoleBase.Button.Up);
            _keyMapping.Add(KeyCode.DownArrow, ConsoleBase.Button.Down);
            _keyMapping.Add(KeyCode.LeftArrow, ConsoleBase.Button.Left);
            _keyMapping.Add(KeyCode.RightArrow, ConsoleBase.Button.Right);
            _keyMapping.Add(KeyCode.Z, ConsoleBase.Button.A);
            _keyMapping.Add(KeyCode.X, ConsoleBase.Button.B);
            _keyMapping.Add(KeyCode.Space, ConsoleBase.Button.Start);
            _keyMapping.Add(KeyCode.LeftShift, ConsoleBase.Button.Select);


            // Load emulator
            IVideoOutput drawable = new DefaultVideoOutput();
            IAudioOutput audio = GetComponent<AudioManager>();
            Emulator = new Console(drawable,audio);
            ScreenRenderer.material.mainTexture = ((DefaultVideoOutput)Emulator.Video).Texture;
            gameObject.GetComponent<AudioSource>().enabled = false;
            StartCoroutine(LoadRom(Filename));
        }

        void Update()
        {
            // Input
            foreach (KeyValuePair<KeyCode, ConsoleBase.Button> entry in _keyMapping)
            {
                if (Input.GetKeyDown(entry.Key))
                    Emulator.SetInput(entry.Value, true);
                else if (Input.GetKeyUp(entry.Key))
                    Emulator.SetInput(entry.Value, false);
            }

            if (Input.GetKeyDown(KeyCode.T))
            {
                byte[] screenshot = ((DefaultVideoOutput)Emulator.Video).Texture.EncodeToPNG();
                File.WriteAllBytes("./screenshot.png", screenshot);
                Debug.Log("Screenshot saved.");
            }
        }



        private IEnumerator LoadRom(string filename)
        {
            string path = filename;
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
                // Run
                Emulator.RunNextStep();

                yield return null;
            }
        }
    }


}
