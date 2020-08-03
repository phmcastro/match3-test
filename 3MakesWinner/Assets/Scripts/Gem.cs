using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[System.Serializable]
public class Gem
{
    public Image myImage;
    public GameObject myGameObject;
    public RectTransform myRectTransform;
    public int myGemType;
    public Vector2 currentPos;
    public int myRow = -1;
    public int myColumn = -1;

    // called from TheGrid when creating a first grid
    public Gem(GameObject gO, int spriteType, Vector2 startPos, int row, int column)
    {
        myGameObject = gO;
        myGemType = spriteType;
        currentPos = startPos;
        myRectTransform = myGameObject.GetComponent<RectTransform>();
        myImage = myGameObject.GetComponent<Image>();

        myRow = row;
        myColumn = column;

        SetGemPosition(currentPos);
        SetGemType(spriteType);
    }


    // for better logging
    public override string ToString()
    {
        return $"GemIndex: {myRow}, {myColumn}; GemType: {myGemType}";
    }


    public void SetGemPosition(Vector2 newPos)
    {
        myRectTransform.anchoredPosition = newPos;
    }

    public void SetRandomGemType()
    {
        int randomGemType = Random.Range(0, UIManager.gemSprites.Count);

        myImage.overrideSprite = UIManager.gemSprites[randomGemType];
        myImage.color = Color.white;
        myGemType = randomGemType;
    }

    public void SetGemType(int gemT)
    {
        if(gemT == -1)
        {
            myImage.color = Color.clear;
            myGemType = gemT;
            return;
        }

        myImage.overrideSprite = UIManager.gemSprites[gemT];
        myImage.color = Color.white;
        myGemType = gemT;
    }
}
