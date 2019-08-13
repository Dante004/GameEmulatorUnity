using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Detector : MonoBehaviour
{
    private int _xOffset = -6;
    private const float CoolDownSlow = 0.3f;
    private const float CoolDownFast = 0.1f;
    private float _coolDownMove = 0.3f;
    private float _movingTime = 0;
    private float _fasterTime = 0;

    public List<string> tiles = new List<string>();
    public string[] files;
    public int menuPosition = 0;
    public Text title;

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
    }

    void Update()
    {
        title.text = tiles[menuPosition];

        var h = Input.GetAxisRaw("Horizontal");

        _movingTime -= Time.deltaTime;
        _fasterTime += Time.deltaTime;

        if ((int)h != 0 && _fasterTime > 0.5f)
        {
            _coolDownMove = CoolDownFast;
        }
        else if ((int)h == 0)
        {
            _coolDownMove = CoolDownSlow;
            _fasterTime = 0;
        }

        if ((int)h == 1 && _movingTime < 0)
        {
            menuPosition++;
            _movingTime = _coolDownMove;
            gameObject.GetComponent<RectTransform>().localPosition = new Vector3(gameObject.GetComponent<RectTransform>()
                                                                                     .localPosition.x - 290, 0, 0);
        }
        else if ((int)h == -1 && _movingTime < 0)
        {
            menuPosition--;
            _movingTime = _coolDownMove;
            gameObject.GetComponent<RectTransform>().localPosition = new Vector3(gameObject.GetComponent<RectTransform>()
                                                                                     .localPosition.x + 290, 0, 0);
        }

        if (menuPosition > tiles.Count - 1)
        {
            menuPosition = tiles.Count - 1;
            gameObject.GetComponent<RectTransform>().localPosition = new Vector3(gameObject.GetComponent<RectTransform>()
                                                                                     .localPosition.x + 290, 0, 0);
        }
        else if (menuPosition < 0)
        {
            menuPosition = 0;
            gameObject.GetComponent<RectTransform>().localPosition = new Vector3(gameObject.GetComponent<RectTransform>()
                                                                                     .localPosition.x - 290, 0, 0);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            PlayerPrefs.SetString("file", files[menuPosition]);
            SceneManager.LoadScene(1, LoadSceneMode.Single);
        }
    }
}
