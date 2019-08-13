using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Detector : MonoBehaviour
{
    void Start()
    {
        var pathToRomes = Path.GetFullPath("Roms");

        var images = Resources.LoadAll<Sprite>("img").ToDictionary(x => x.name.ToUpper(), x => x);

        if (Directory.Exists(pathToRomes))
        {
            files = Directory.GetFileSystemEntries(pathToRomes);
            foreach (var file in files)
            {
                if (!file.EndsWith(".meta") && file.EndsWith(".gb") || file.EndsWith(".gbc"))
                {
                    var romGame = ROMLoader.Load(file);

                    var romObject = new GameObject($"{romGame.title} Root");
                    romObject.transform.SetParent(gameObject.transform, false);
                    romObject.transform.position = new Vector3(_xOffset, 0, 0);

                    _xOffset += 4;

                    tiles.Add(romGame.title);

                    var coverObject = new GameObject($"{romGame.title} Cover");
                    coverObject.transform.SetParent(romObject.transform, false);

                    var imageComp = coverObject.AddComponent<Image>();
                    imageComp.sprite = images.ContainsKey(romGame.title) ? images[romGame.title] : images["DEFAULT"];
                    imageComp.rectTransform.sizeDelta = new Vector2(240, 240);
                }
            }
        }
        else
        {
            Directory.CreateDirectory(pathToRomes);
        }
        // Update is called once per frame
        void Update()
        {

        }
    }
}
