using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using System;
public class DetectingFiles : MonoBehaviour
{
    public GameObject textPrefab;
    public int numberOfFiles = 0;
    public string[] files;
    public int positionY = 10;
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
                    Debug.Log(romGame.ToString());
                }
                positionY -= 30;
            }
        }
    }

}
