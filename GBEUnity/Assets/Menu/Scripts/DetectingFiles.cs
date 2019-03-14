using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using System;
using System.Linq;
public class DetectingFiles : MonoBehaviour
{
    public GameObject textPrefab;
    public GameObject titlePrefab;
    public int numberOfFiles = 0;
    [NonSerialized]
    public string[] files;
    public string[] posters;
    public int positionY = 10;
    public int positionX = 100;
    public int numberOfPosters = 0;
  public Sprite sprite;
    public string TMP;
    // Start is called before the first frame update

    void Start()
    {
        if (Directory.Exists(Path.GetFullPath("Roms")))
        {
            Dictionary<string, Sprite> obrazki = Resources.LoadAll<Sprite>("img").ToDictionary(x => x.name.ToUpper(), x => x);
            files = Directory.GetFileSystemEntries(Path.GetFullPath("Roms"));
            for (int i = 0; i < files.Length; i++)
            {
                if (!files[i].EndsWith(".meta") && files[i].EndsWith(".gb"))
                {
                    numberOfFiles++;
                    FileInfo Files = new FileInfo(files[i]);
                    GameObject prefabTextTmp = Instantiate(textPrefab) as GameObject;
                    Text textName = prefabTextTmp.GetComponentInChildren<Text>();
                    textName.text = "Files :" + Files.Name;
                    prefabTextTmp.transform.SetParent(gameObject.transform, false);
                    RectTransform rect = prefabTextTmp.GetComponent<RectTransform>();
                    rect.localPosition = new Vector2(prefabTextTmp.transform.position.x, positionY);
                    textName.fontSize = 50;
                    textName.horizontalOverflow = HorizontalWrapMode.Overflow;
                    textName.verticalOverflow = VerticalWrapMode.Overflow;
                    textName.alignment = TextAnchor.MiddleCenter;
                    RomGame romGame = ROMLoader.Load(files[i]);                   
                    GameObject prefabTitleTmp = Instantiate(titlePrefab) as GameObject;
                    Text title = prefabTitleTmp.GetComponentInChildren<Text>();
                    title.text = romGame.title.ToString();
                    TMP = romGame.title.ToString();
                    prefabTitleTmp.transform.SetParent(gameObject.transform, false);
                    RectTransform rectTitle = prefabTitleTmp.GetComponent<RectTransform>();
                    rectTitle.localPosition = new Vector2(positionX,100 );
                    title.fontSize = 50;
                    title.horizontalOverflow = HorizontalWrapMode.Overflow;
                    title.verticalOverflow = VerticalWrapMode.Overflow;
                    title.alignment = TextAnchor.MiddleCenter;
                    posters = Directory.GetFileSystemEntries("Assets\\Resources");
                    prefabTitleTmp.GetComponentInChildren<Image>().sprite = obrazki[TMP];
                }
                positionY -= 30;
                positionX -= 200;            
            }           
        }        
    }
}
