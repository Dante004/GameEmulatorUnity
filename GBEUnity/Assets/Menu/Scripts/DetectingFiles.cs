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
    
    void Start()

    {
        if (Directory.Exists(Path.GetFullPath("Roms")))
        {
    
             Dictionary<string, Sprite> Images = Resources.LoadAll<Sprite>("img").ToDictionary(x => x.name.ToUpper(), x => x);
            files = Directory.GetFileSystemEntries(Path.GetFullPath("Roms"));
            for (int i = 0; i < files.Length; i++)
            {
                getRom(files[i]);
                if (!files[i].EndsWith(".meta") && files[i].EndsWith(".gb"))
                {
                    numberOfFiles++;                    
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
                    prefabTitleTmp.GetComponentInChildren<Image>().sprite = Images[TMP];
                    prefabTitleTmp.transform.GetComponentInChildren<Image>().rectTransform.localPosition = new Vector2(40, -170);
                    prefabTitleTmp.transform.GetComponentInChildren<Image>().rectTransform.sizeDelta = new Vector2(250, 250);
                }                
                positionY -= 30;
                positionX -= 200;            
            }           
        }        
    }
    public void getRom(string path)
    {     
            if (!path.EndsWith(".meta") && path.EndsWith(".gb"))
            {
                numberOfFiles++;
                FileInfo Files = new FileInfo(path);
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
            }
    }
}
