using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using System;
public class DetectingFiles : MonoBehaviour
{
    public GameObject textPrefab;
    public GameObject titlePrefab;
    public int numberOfFiles = 0;
    public string[] files;
    public int positionY = 10;
    public int positionX = 100;
    // Start is called before the first frame update
    
    void Start()
    {
        if (Directory.Exists(Path.GetFullPath("Roms")))
        {
           
            files = Directory.GetFileSystemEntries(Path.GetFullPath("Roms"));
            for (int i = 0; i < files.Length; i++)
            {
                if (!files[i].Contains(".meta") && files[i].Contains(".gb"))
                {
                    numberOfFiles++;
                    FileInfo File = new FileInfo(files[i]);
                    GameObject prefabTextTmp = Instantiate(textPrefab) as GameObject;
                    Text textName = prefabTextTmp.GetComponentInChildren<Text>();
                    textName.text = "Files :" + File.Name;
                    prefabTextTmp.transform.SetParent(gameObject.transform, false);
                    RectTransform rect = prefabTextTmp.GetComponent<RectTransform>();
                    rect.localPosition = new Vector2(prefabTextTmp.transform.position.x, positionY);
                    textName.fontSize = 50;
                    textName.horizontalOverflow = HorizontalWrapMode.Overflow;
                    textName.verticalOverflow = VerticalWrapMode.Overflow;
                    textName.alignment = TextAnchor.MiddleCenter;
                    RomGame romGame = ROMLoader.Load(files[i]);
                    //Debug.Log(romGame.title);
                    GameObject prefabTitleTmp = Instantiate(titlePrefab) as GameObject;
                    Text title = prefabTitleTmp.GetComponentInChildren<Text>();
                    title.text = romGame.title.ToString();
                    prefabTitleTmp.transform.SetParent(gameObject.transform, false);
                    RectTransform rectTitle = prefabTitleTmp.GetComponent<RectTransform>();
                    rectTitle.localPosition = new Vector2(positionX,100 );
                    title.fontSize = 50;
                    title.horizontalOverflow = HorizontalWrapMode.Overflow;
                    title.verticalOverflow = VerticalWrapMode.Overflow;
                    title.alignment = TextAnchor.MiddleCenter;
                     
                }
                positionY -= 30;
                positionX -= 200;
            }
        }
    }

}
