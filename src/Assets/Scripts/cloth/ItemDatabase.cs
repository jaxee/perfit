using System.Collections.Generic;
using UnityEngine;

public class ItemDatabase : MonoBehaviour
{
	public List<ClothingItem> clothesList = new List<ClothingItem>();
    public static ItemDatabase instance; 


    private void Awake()
    {
        //create singleton
		//Debug.Log ("Happen");
        instance = this; 

		//1.Naked (Empty/No dress/0)
		clothesList.Add(new ClothingItem(0, "", "", "naked", "Dress"));

		//2.Dresses
									//ItemID //Name //Description //FileName //ItemType //GameObjectInstance

		clothesList.Add(new ClothingItem(1, "", "", "dress01_s", "Dress", (GameObject)Resources.Load("Dresses/small/dress01_s")));
		clothesList.Add(new ClothingItem(2, "", "", "dress02_s", "Dress", (GameObject)Resources.Load("Dresses/small/dress02_s")));
		clothesList.Add(new ClothingItem(3, "", "", "dress03_s", "Dress", (GameObject)Resources.Load("Dresses/small/dress03_s")));
		clothesList.Add(new ClothingItem(4, "", "", "dress04_s", "Dress", (GameObject)Resources.Load("Dresses/small/dress04_s")));
		clothesList.Add(new ClothingItem(5, "", "", "dress05_s", "Dress", (GameObject)Resources.Load("Dresses/small/dress05_s")));
		clothesList.Add(new ClothingItem(6, "", "", "dress06_s", "Dress", (GameObject)Resources.Load("Dresses/small/dress06_s")));
		clothesList.Add(new ClothingItem(7, "", "", "dress07_s", "Dress", (GameObject)Resources.Load("Dresses/small/dress07_s")));
		clothesList.Add(new ClothingItem(8, "", "", "dress08_s", "Dress", (GameObject)Resources.Load("Dresses/small/dress08_s")));

		clothesList.Add(new ClothingItem(9, "", "", "dress01_m", "Dress", (GameObject)Resources.Load("Dresses/medium/dress01_m")));
		clothesList.Add(new ClothingItem(10, "", "", "dress02_m", "Dress", (GameObject)Resources.Load("Dresses/medium/dress02_m")));
		clothesList.Add(new ClothingItem(11, "", "", "dress03_m", "Dress", (GameObject)Resources.Load("Dresses/medium/dress03_m")));
		clothesList.Add(new ClothingItem(12, "", "", "dress04_m", "Dress", (GameObject)Resources.Load("Dresses/medium/dress04_m")));
		clothesList.Add(new ClothingItem(13, "", "", "dress05_m", "Dress", (GameObject)Resources.Load("Dresses/medium/dress05_m")));
		clothesList.Add(new ClothingItem(14, "", "", "dress06_m", "Dress", (GameObject)Resources.Load("Dresses/medium/dress06_m")));
		clothesList.Add(new ClothingItem(15, "", "", "dress07_m", "Dress", (GameObject)Resources.Load("Dresses/medium/dress07_m")));
		clothesList.Add(new ClothingItem(16, "", "", "dress08_m", "Dress", (GameObject)Resources.Load("Dresses/medium/dress08_m")));

		clothesList.Add(new ClothingItem(17, "", "", "dress01_l", "Dress", (GameObject)Resources.Load("Dresses/large/dress01_l")));
		clothesList.Add(new ClothingItem(18, "", "", "dress02_l", "Dress", (GameObject)Resources.Load("Dresses/large/dress02_l")));
		clothesList.Add(new ClothingItem(19, "", "", "dress03_l", "Dress", (GameObject)Resources.Load("Dresses/large/dress03_l")));
		clothesList.Add(new ClothingItem(20, "", "", "dress04_l", "Dress", (GameObject)Resources.Load("Dresses/large/dress04_l")));
		clothesList.Add(new ClothingItem(21, "", "", "dress05_l", "Dress", (GameObject)Resources.Load("Dresses/large/dress05_l")));
		clothesList.Add(new ClothingItem(22, "", "", "dress06_l", "Dress", (GameObject)Resources.Load("Dresses/large/dress06_l")));
		clothesList.Add(new ClothingItem(23, "", "", "dress07_l", "Dress", (GameObject)Resources.Load("Dresses/large/dress07_l")));
		clothesList.Add(new ClothingItem(24, "", "", "dress08_l", "Dress", (GameObject)Resources.Load("Dresses/large/dress08_l")));



	}

	public ClothingItem FetchItemByID(int id)
    {
		for (int i = 0; i < clothesList.Count; i++)
        {
			if (clothesList[i].ItemID == id)
            {
				return clothesList[i];
            }
        }
        return null;
    }

	public ClothingItem FetchItemByFileName(string file)
    {
		for (int i = 0; i < clothesList.Count; i++)
        {

			if (clothesList[i].FileName == file)
            {
				return clothesList[i];
            }
        }
        return null;
    }


}
