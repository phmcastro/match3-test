using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class TheGrid : MonoBehaviour
{
    private Gem[,] theMatrix = new Gem[6, 6];
    public GameObject firstGem;
    private Vector2 firstPos;
    private Vector2 gemPos;
    private Vector2 gemSize;
    public Transform board;
    private UIManager myUIManager;
    private int[] gemClicked1 = new int[2];
    private int[] gemClicked2 = new int[2];
    private bool possibleToMove = false;
    private Gem mainGemMatched;
    private List<Gem> gemsToPop = new List<Gem>();
    public GameObject blockInteractionsPanel;
    private int matchedGemType = -1;

    void Start()
    {

        myUIManager = transform.root.GetComponent<UIManager>();

        firstPos = firstGem.GetComponent<RectTransform>().anchoredPosition;

        gemSize = firstGem.GetComponent<RectTransform>().sizeDelta;
        gemPos = firstPos;

        GenerateGrid(true);

        firstGem.SetActive(false);

        ResetClicks();
        RelaseInteractions();

        // first check if there are possible moves
        possibleToMove = PossibleToMove();
        Debug.Log("Are there any possible moves?  " + possibleToMove);

        if(!possibleToMove) { ShuffleAndCheckGrid(); }
    }

    public void GenerateGrid(bool createNewObjs = false)
    {
        int currentGemType = 0;

        List<int> referenceIndexes = new List<int>();
        List<int> possibleGemTypes = new List<int>();

        // getting the indexes from sprite list
        int iter = 0;
        foreach(var tp in UIManager.gemSprites)
        {
            referenceIndexes.Add(iter);
            iter++;
        }

        int upperGem = -1;
        int leftGem = -1;


        // generate grid with no sequential gem types
        for(int i = 0; i < theMatrix.GetLength(0); i++)
        {
            for(int j = 0; j < theMatrix.GetLength(1); j++)
            {
                possibleGemTypes.AddRange(referenceIndexes);

                if(i == 0)
                {
                    leftGem = j > 0 ? theMatrix[i, j-1].myGemType : -1;

                    possibleGemTypes.Remove(leftGem);

                    currentGemType = possibleGemTypes[Random.Range(0, possibleGemTypes.Count)];
                }
                else if(i > 0 && j == 0)
                {
                    upperGem = theMatrix[(i-1), j].myGemType;

                    possibleGemTypes.Remove(upperGem);

                    currentGemType = possibleGemTypes[Random.Range(0, possibleGemTypes.Count)];
                }
                else if(i > 0 && j > 0)
                {
                    upperGem = theMatrix[i-1, j].myGemType;
                    leftGem = theMatrix[i, j-1].myGemType;

                    possibleGemTypes.Remove(leftGem);
                    possibleGemTypes.Remove(upperGem);

                    currentGemType = possibleGemTypes[Random.Range(0, possibleGemTypes.Count)];
                }

                // when called at the Start function we pass true to create new objects
                if(createNewObjs)
                {
                    theMatrix[i,j] = new Gem(Instantiate(firstGem, board), currentGemType, gemPos, i, j);
                    theMatrix[i,j].myGameObject.GetComponent<GemController>().LoadGemController(this, theMatrix[i, j]);
                }
                else
                {
                    theMatrix[i, j].SetGemType(currentGemType);
                }
                
                
                gemPos.x += gemSize.x;

                possibleGemTypes.Clear();
            }
            gemPos.y -= gemSize.y;
            gemPos.x = firstPos.x;
        }
    }

    public void ResetClicks()
    {
        gemClicked1[0] = -1;
        gemClicked1[1] = -1;

        gemClicked2[0] = -1;
        gemClicked2[1] = -1;

    }

    // activates an invisible UI panel that blocks any clicks interactions
    public void BlockInteractions()
    {
        blockInteractionsPanel.SetActive(true);
    }

    public void RelaseInteractions()
    {
        blockInteractionsPanel.SetActive(false);
    }


    // called from GemController
    public void GotClicked(int gemRow, int gemColumn)
    {
        if(gemClicked1[0] == -1 && gemClicked1[1] == -1)
        {
            gemClicked1[0] = gemRow;
            gemClicked1[1] = gemColumn;

            theMatrix[gemRow, gemColumn].myImage.color = Color.grey;
            myUIManager.PlaySoundFX(1);

            return;
        }

        // checking if the second clicked gem was a valid move option (one up, one down, one right and one left)
        bool possibleRowChoice = gemRow == (gemClicked1[0] - 1) || gemRow == (gemClicked1[0] + 1) || gemRow == gemClicked1[0];
        bool possibleColumnChoice = gemColumn == (gemClicked1[1] - 1) || gemColumn == (gemClicked1[1] + 1) || gemColumn == gemClicked1[1];

        // ruling out diagonal move tries
        bool diag1 = gemRow == (gemClicked1[0] - 1) && gemColumn == (gemClicked1[1] - 1);
        bool diag2 = gemRow == (gemClicked1[0] - 1) && gemColumn == (gemClicked1[1] + 1);
        bool diag3 = gemRow == (gemClicked1[0] + 1) && gemColumn == (gemClicked1[1] - 1);
        bool diag4 = gemRow == (gemClicked1[0] + 1) && gemColumn == (gemClicked1[1] + 1);

        if(diag1 || diag2 || diag3 || diag4)
        {
            return;
        }


        if(possibleRowChoice)
        {
            if(possibleColumnChoice)
            {
                gemClicked2[0] = gemRow;
                gemClicked2[1] = gemColumn;

                theMatrix[gemRow, gemColumn].myImage.color = Color.grey;
            }
            else { return; }
        }
        else { return; }


        myUIManager.PlaySoundFX(2);

        BlockInteractions();
        
        // getting move direction: x(1) = right, x(-1) = left, y(1) = up and y(-1) = down (if x or y equal to 0 it means there was no move on that axis)
        if(CheckMoveForCombinations((gemClicked2[1] - gemClicked1[1]), (gemClicked1[0] - gemClicked2[0])))
        {
            MatchGems();
        }
        else
        {
            MatchGems(false);
        }

    }

    // gets the sprite index from above gem and changes to it
    private void ChangeToAboveGemType(int iRef, int jRef)
    {
        int aboveGemType = -1;

        if(iRef > 0)
        {
            aboveGemType = theMatrix[(iRef-1), jRef].myGemType;

            theMatrix[iRef, jRef].SetGemType(aboveGemType);
        }
        else if(iRef == 0)
        {
            theMatrix[iRef, jRef].SetRandomGemType();
        }


    }


    // safe way to add to a list when we don't want repeated elements inside
    private void CheckAndAddToGemList(List<Gem> l, Gem g)
    {
        if(!l.Contains(g))
        {
            l.Add(g);
        }
    }



    // takes care of match gem dynamics or not match 
    private void MatchGems(bool isMatch = true)
    {
        Debug.Log("Gems to match or not -> Gem1: " + gemClicked1[0] + ", " + gemClicked1[1] + "  ##  Gem2: " + gemClicked2[0] + ", " + gemClicked2[1]);


        int gem1Type = theMatrix[gemClicked1[0], gemClicked1[1]].myGemType;
        int gem2Type = theMatrix[gemClicked2[0], gemClicked2[1]].myGemType;


        if(!isMatch)
        {
            mainGemMatched = null;
            StartCoroutine(WrongMatch(gem1Type, gem2Type));
            return;
        }

        

        theMatrix[gemClicked1[0], gemClicked1[1]].SetGemType(gem2Type);
        theMatrix[gemClicked2[0], gemClicked2[1]].SetGemType(gem1Type);


        myUIManager.PlaySoundFX(0);


        // "pops" matched gems, looks for matches on a "cross" pattern and pops them too
        ChainMatch(matchedGemType, mainGemMatched);

        FillGemHoles();

        gemsToPop.Clear();
        CheckForSubsequentMatches();

        // when the list of gems to "pop" is empty it means there are no subsequent matches
        while(gemsToPop.Count != 0)
        {
            Debug.Log("subsequent matches found!!!");

            foreach(Gem g in gemsToPop)
            {
                // "poping" subsequent matches gems
                g.SetGemType(-1);
            }
            
            FillGemHoles();

            gemsToPop.Clear();
            // checks for subsequent matches again and updates gems to pop list
            CheckForSubsequentMatches();
        }


        mainGemMatched = null;


        ResetClicks();
        RelaseInteractions();



        // after every match check again if there are possible moves

        possibleToMove = PossibleToMove();

        // if there are no possible moves shuffle the grid
        if(!possibleToMove)
        {
            ShuffleAndCheckGrid();
        }

    }


    // look for and add gems from other matches generated with current move to a list
    private void CheckForSubsequentMatches()
    {
        int gemTypeToFind = -1;
        //int goLook = 1;
        
        for(int i = 0; i < theMatrix.GetLength(0); i++)
        {
            for(int j = 0; j < theMatrix.GetLength(1); j++)
            {
                gemTypeToFind = theMatrix[i, j].myGemType;

                for(int goLookX = 1; j+goLookX < theMatrix.GetLength(1); goLookX++)
                {
                    if(theMatrix[i, j+goLookX].myGemType == gemTypeToFind)
                    {
                        if(goLookX == 2)
                        {
                            CheckAndAddToGemList(gemsToPop, theMatrix[i, j]);
                            CheckAndAddToGemList(gemsToPop, theMatrix[i, j+1]);
                            CheckAndAddToGemList(gemsToPop, theMatrix[i, j+2]);
                        }
                        else if(goLookX > 2)
                        {
                            CheckAndAddToGemList(gemsToPop, theMatrix[i, j+goLookX]);
                        }

                    }
                    else { break; }
                }

                for(int goLookY = 1; i+goLookY < theMatrix.GetLength(0); goLookY++)
                {
                    if(theMatrix[i+goLookY, j].myGemType == gemTypeToFind)
                    {
                        if(goLookY == 2)
                        {
                            CheckAndAddToGemList(gemsToPop, theMatrix[i, j]);
                            CheckAndAddToGemList(gemsToPop, theMatrix[i+1, j]);
                            CheckAndAddToGemList(gemsToPop, theMatrix[i+2, j]);
                        }
                        else if(goLookY > 2)
                        {
                            CheckAndAddToGemList(gemsToPop, theMatrix[i+goLookY, j]);
                        }

                    }
                    else { break; }
                }

            }
            
        }
    }


    private void FillGemHoles()
    {
        // iterates through whole matrix and simulates gems dropping
        for(int i = 0; i < theMatrix.GetLength(0); i++)
        {
            for(int j = 0; j < theMatrix.GetLength(1); j++)
            {
                if(theMatrix[i, j].myGemType == -1)
                {
                    for(int k = i; k >= 0; k--)
                    {
                        ChangeToAboveGemType(k, j);
                    }
                    
                }
            }
            
        }
    }

    // when we have a "wrong" move we show both gems as red and them bring them back to their places
    private IEnumerator WrongMatch(int gemT1, int gemT2)
    {
        WaitForSeconds delay = new WaitForSeconds(.3f);

        theMatrix[gemClicked1[0], gemClicked1[1]].SetGemType(gemT2);
        theMatrix[gemClicked2[0], gemClicked2[1]].SetGemType(gemT1);

        yield return delay;

        theMatrix[gemClicked1[0], gemClicked1[1]].myImage.color = Color.red;
        theMatrix[gemClicked2[0], gemClicked2[1]].myImage.color = Color.red;

        yield return delay;

        theMatrix[gemClicked1[0], gemClicked1[1]].SetGemType(gemT1);
        theMatrix[gemClicked2[0], gemClicked2[1]].SetGemType(gemT2);

        theMatrix[gemClicked1[0], gemClicked1[1]].myImage.color = Color.white;
        theMatrix[gemClicked2[0], gemClicked2[1]].myImage.color = Color.white;

        ResetClicks();
        RelaseInteractions();
    }



    private void ChainMatch(int matchedType, Gem mainGem)
    {

        int nRows = theMatrix.GetLength(0);
        int nColumns = theMatrix.GetLength(1);

        Debug.Log("main gem :  " + mainGem.myRow + ", " + mainGem.myColumn + " of the type: " + matchedType);

        mainGem.SetGemType(-1);
        myUIManager.AddScore(1);


        for(int down = mainGem.myRow+1; down < nRows; down++)
        {
            if(theMatrix[down, mainGem.myColumn].myGemType == matchedGemType)
            {
                theMatrix[down, mainGem.myColumn].SetGemType(-1);
                myUIManager.AddScore(1);
                Debug.Log("down chain!");
            }
            else { break; }
        }
        for(int up = mainGem.myRow-1; up >= 0; up--)
        {
            if(theMatrix[up, mainGem.myColumn].myGemType == matchedGemType)
            {
                theMatrix[up, mainGem.myColumn].SetGemType(-1);
                myUIManager.AddScore(1);
                Debug.Log("up chain!");
            }
            else { break; }
        }

        for(int right = mainGem.myColumn+1; right < nColumns; right++)
        {
            if(theMatrix[mainGem.myRow, right].myGemType == matchedGemType)
            {
                theMatrix[mainGem.myRow, right].SetGemType(-1);
                myUIManager.AddScore(1);
                Debug.Log("right chain!");
            }
            else { break; }
        }
        for(int left = mainGem.myColumn-1; left >= 0; left--)
        {
            if(theMatrix[mainGem.myRow, left].myGemType == matchedGemType)
            {
                theMatrix[mainGem.myRow, left].SetGemType(-1);
                myUIManager.AddScore(1);
                Debug.Log("left chain!");
            }
            else { break; }
        }

        
    }



    private bool CheckMoveForCombinations(int xDirection, int yDirection)
    {
        // if any of these directions is equal to zero it means there were no moves in that direction
        // for moves on the X axis 1 means right and -1 means left
        // for moves on the Y axis 1 means up and -1 means down

        mainGemMatched = null;

        if(xDirection != 0)
        {
            return CheckMoveOnTheX(gemClicked1[0], gemClicked1[1], xDirection) || CheckMoveOnTheX(gemClicked2[0], gemClicked2[1], xDirection*-1);
        }
        else
        {
            return CheckMoveOnTheY(gemClicked1[0], gemClicked1[1], yDirection) || CheckMoveOnTheY(gemClicked2[0], gemClicked2[1], yDirection*-1);
        }

    }


    private bool CheckMoveOnTheX(int i, int j, int dir)
    {
        int columnsToTheRight = (theMatrix.GetLength(1) - j) - 1;
        int columnsToTheLeft = j;
        int rowsAbove = i;
        int rowsBelow = (theMatrix.GetLength(0) - i) - 1;
        int currentGemtype = theMatrix[i, j].myGemType;
        matchedGemType = currentGemtype;
        bool combinationFound = false;

        Debug.Log("Gem1 indexes:  " + i + ", " + j + "  ##  " + "Gem2 indexes:  " + gemClicked2[0] + ", " + gemClicked2[1]);

        // getting the main gem for match testing
        if(columnsToTheLeft > 0 && columnsToTheRight > 0)
        {
            mainGemMatched = theMatrix[i, (j+dir)];
            Debug.Log("Gem future spot :  " + mainGemMatched.myRow + ", " + mainGemMatched.myColumn);
        }
        else if(j == 0)
        {
            mainGemMatched = theMatrix[i, j+1];
            Debug.Log("Gem future spot :  " + mainGemMatched.myRow + ", " + mainGemMatched.myColumn);
        }
        else
        {
            mainGemMatched = theMatrix[i, j-1];
            Debug.Log("Gem future spot :  " + mainGemMatched.myRow + ", " + mainGemMatched.myColumn);
        }
        
        
        if(dir == 1)
        {

            // checking for combinations to the right (Ex: # 0 # #)
            if(columnsToTheRight > 2)
            {
                if((theMatrix[i, (j+2)].myGemType == theMatrix[i, (j+3)].myGemType) && theMatrix[i, (j+2)].myGemType == currentGemtype)
                {
                    combinationFound = true;

                    Debug.Log("X Combination r-type 1 with cell: " + i + ", " + j + " of gem type: " + currentGemtype);

                    return combinationFound;
                }
            }

            // checking for middle combinations to the right
            // (Ex:   #  )
            //      # 0
            //        #
            if(columnsToTheRight >= 1 && rowsAbove >= 1 && rowsBelow >= 1)
            {
                if((theMatrix[(i-1), (j+1)].myGemType == theMatrix[(i+1), (j+1)].myGemType) && theMatrix[(i-1), (j+1)].myGemType == currentGemtype)
                {
                    combinationFound = true;

                    Debug.Log("X Combination r-type 2 with cell: " + i + ", " + j + " of gem type: " + currentGemtype);

                    return combinationFound;
                }
            }


            // checking for combinations to the right above
            // (Ex:   # )
            //        #
            //      # 0
            if(columnsToTheRight >= 1 && rowsAbove >= 2)
            {
                //Debug.Log("mainGem: " + currentGemtype + " first gem: " + theMatrix[(row-1), (column+1)].myGemType + " second gem: " + theMatrix[(row-2), (column+1)].myGemType);
                if((theMatrix[(i-1), (j+1)].myGemType == theMatrix[(i-2), (j+1)].myGemType) && theMatrix[(i-1), (j+1)].myGemType == currentGemtype)
                {
                    combinationFound = true;

                    Debug.Log("X Combination r-type 3 with cell: " + i + ", " + j + " of gem type: " + currentGemtype);
                    return combinationFound;
                }
            }


            // checking for combinations to the right below
            // (Ex: # 0  )
            //        #
            //        #
            if(columnsToTheRight >= 1 && rowsBelow >= 2)
            {
                if((theMatrix[(i+1), (j+1)].myGemType == theMatrix[(i+2), (j+1)].myGemType) && theMatrix[(i+1), (j+1)].myGemType == currentGemtype)
                {
                    combinationFound = true;

                    Debug.Log("X Combination r-type 4 with cell: " + i + ", " + j + " of gem type: " + currentGemtype);
                    return combinationFound;
                }
            }
        }
        else if(dir == -1)
        {
            // checking for combinations to the left (Ex: # # 0 #)
            // first check if we have columns to the left
            if(columnsToTheLeft > 2)
            {
                if((theMatrix[i, (j-2)].myGemType == theMatrix[i, (j-3)].myGemType) && theMatrix[i, (j-2)].myGemType == currentGemtype)
                {
                    combinationFound = true;

                    Debug.Log("X Combination l-type 1 with cell: " + i + ", " + j + " of gem type: " + currentGemtype);
                    return combinationFound;
                }
            }

            // checking for middle combinations to the left
            // (Ex:   #    )
            //        0 #
            //        #
            if(columnsToTheLeft >= 1 && rowsAbove >= 1 && rowsBelow >= 1)
            {
                if((theMatrix[(i-1), (j-1)].myGemType == theMatrix[(i+1), (j-1)].myGemType) && theMatrix[(i-1), (j-1)].myGemType == currentGemtype)
                {
                    combinationFound = true;

                    Debug.Log("X Combination l-type 2 with cell: " + i + ", " + j + " of gem type: " + currentGemtype);
                    return combinationFound;
                }
            }


            // checking for combinations to the left above
            // (Ex: #   )
            //      #
            //      0 #
            if(columnsToTheLeft >= 1 && rowsAbove >= 2)
            {
                if((theMatrix[(i-1), (j-1)].myGemType == theMatrix[(i-2), (j-1)].myGemType) && theMatrix[(i-1), (j-1)].myGemType == currentGemtype)
                {
                    combinationFound = true;

                    Debug.Log("X Combination l-type 3 with cell: " + i + ", " + j + " of gem type: " + currentGemtype);
                    return combinationFound;
                }
            }


            // checking for combinations to the left below
            // (Ex: 0 #  )
            //      #
            //      #
            if(columnsToTheLeft >= 1 && rowsBelow >= 2)
            {
                if((theMatrix[(i+1), (j-1)].myGemType == theMatrix[(i+2), (j-1)].myGemType) && theMatrix[(i+1), (j-1)].myGemType == currentGemtype)
                {
                    combinationFound = true;

                    Debug.Log("X Combination l-type 4 with cell: " + i + ", " + j + " of gem type: " + currentGemtype);
                    return combinationFound;
                }
            }
        }

        mainGemMatched= null;
        return combinationFound;
    }



    private bool CheckMoveOnTheY(int i, int j, int dir)
    {
        int columnsToTheRight = (theMatrix.GetLength(1) - j) - 1;
        int columnsToTheLeft = j;
        int rowsAbove = i;
        int rowsBelow = (theMatrix.GetLength(0) - i) - 1;
        int currentGemtype = theMatrix[i, j].myGemType;
        matchedGemType = currentGemtype;
        bool combinationFound = false;

        Debug.Log("Gem1 indexes:  " + i + ", " + j + "  ##  " + "Gem2 indexes:  " + gemClicked2[0] + ", " + gemClicked2[1]);

        // getting the main gem for match testing
        if(rowsAbove > 0 && rowsBelow > 0)
        {
            mainGemMatched = theMatrix[(i+(dir*-1)), j];
            Debug.Log("Gem future spot :  " + mainGemMatched.myRow + ", " + mainGemMatched.myColumn);
        }
        else if(i == 0)
        {
            mainGemMatched = theMatrix[i+1, j];
            Debug.Log("Gem future spot :  " + mainGemMatched.myRow + ", " + mainGemMatched.myColumn);
        }
        else
        {
            mainGemMatched = theMatrix[i-1, j];
            Debug.Log("Gem future spot :  " + mainGemMatched.myRow + ", " + mainGemMatched.myColumn);
        }
        

        if(dir == 1)
        {
            // checking for combinations above
            //(Ex: # )
            //     #
            //     0
            //     #
            // first check if we have rows above
            if(rowsAbove > 2)
            {
                if((theMatrix[(i-2), j].myGemType == theMatrix[(i-3), j].myGemType) && theMatrix[(i-2), j].myGemType == currentGemtype)
                {
                    combinationFound = true;

                    Debug.Log("Y Combination up-type 1 with cell: " + i + ", " + j + " of gem type: " + currentGemtype);
                    return combinationFound;
                }
            }


            // checking for middle combinations above
            // (Ex: # 0 # )
            //        #
            if(rowsAbove >= 1 && columnsToTheRight >= 1 && columnsToTheLeft >= 1)
            {
                if((theMatrix[(i-1), (j-1)].myGemType == theMatrix[(i-1), (j+1)].myGemType) && theMatrix[(i-1), (j-1)].myGemType == currentGemtype)
                {
                    combinationFound = true;

                    Debug.Log("Y Combination up-type 2 with cell: " + i + ", " + j + " of gem type: " + currentGemtype);
                    return combinationFound;
                }
            }


            // checking for combinations above to the left
            // (Ex: # # 0 )
            //          #
            if(rowsAbove >= 1 && columnsToTheLeft >= 2)
            {
                if((theMatrix[(i-1), (j-1)].myGemType == theMatrix[(i-1), (j-2)].myGemType) && theMatrix[(i-1), (j-1)].myGemType == currentGemtype)
                {
                    combinationFound = true;

                    Debug.Log("Y Combination up-type 3 with cell: " + i + ", " + j + " of gem type: " + currentGemtype);
                    return combinationFound;
                }
            }


            // checking for combinations above to the right
            // (Ex: 0 # # )
            //      #
            if(rowsAbove >= 1 && columnsToTheRight >= 2)
            {
                if((theMatrix[(i-1), (j+1)].myGemType == theMatrix[(i-1), (j+2)].myGemType) && theMatrix[(i-1), (j+1)].myGemType == currentGemtype)
                {
                    combinationFound = true;

                    Debug.Log("Y Combination up-type 4 with cell: " + i + ", " + j + " of gem type: " + currentGemtype);
                    return combinationFound;
                }
            }
        }
        else if(dir == -1)
        {
            // checking for combinations below
            //(Ex: # )
            //     0
            //     #
            //     #
            // first check if we have rows below
            if(rowsBelow > 2)
            {
                if((theMatrix[(i+2), j].myGemType == theMatrix[(i+3), j].myGemType) && theMatrix[(i+2), j].myGemType == currentGemtype)
                {
                    combinationFound = true;

                    Debug.Log("Y Combination down-type 1 with cell: " + i + ", " + j + " of gem type: " + currentGemtype);
                    return combinationFound;
                }
            }


            // checking for middle combinations below
            // (Ex:   #   )
            //      # 0 #
            if(rowsBelow >= 1 && columnsToTheRight >= 1 && columnsToTheLeft >= 1)
            {
                if((theMatrix[(i+1), (j-1)].myGemType == theMatrix[(i+1), (j+1)].myGemType) && theMatrix[(i+1), (j-1)].myGemType == currentGemtype)
                {
                    combinationFound = true;

                    Debug.Log("Y Combination down-type 2 with cell: " + i + ", " + j + " of gem type: " + currentGemtype);
                    return combinationFound;
                }
            }


            // checking for combinations below to the left
            // (Ex:     # )
            //      # # 0
            if(rowsBelow >= 1 && columnsToTheLeft >= 2)
            {
                if((theMatrix[(i+1), (j-1)].myGemType == theMatrix[(i+1), (j-2)].myGemType) && theMatrix[(i+1), (j-1)].myGemType == currentGemtype)
                {
                    combinationFound = true;

                    Debug.Log("Y Combination down-type 3 with cell: " + i + ", " + j + " of gem type: " + currentGemtype);
                    return combinationFound;
                }
            }


            // checking for combinations below to the right
            // (Ex: #     )
            //      0 # #
            if(rowsBelow >= 1 && columnsToTheRight >= 2)
            {
                if((theMatrix[(i+1), (j+1)].myGemType == theMatrix[(i+1), (j+2)].myGemType) && theMatrix[(i+1), (j+1)].myGemType == currentGemtype)
                {
                    combinationFound = true;

                    Debug.Log("Y Combination down-type 4 with cell: " + i + ", " + j + " of gem type: " + currentGemtype);
                    return combinationFound;
                }
            }
        }

        mainGemMatched = null;
        return combinationFound;
    }




    // method to check if there are any possible moves to match 3 and score
    private bool PossibleToMove()
    {
        int nRows = theMatrix.GetLength(0);
        int nColumns = theMatrix.GetLength(1);
        // 4 integers to keep track of the surroundings of each element
        // this way we can check if there are possible moves above, below, to the right and to the left of the element
        int rowsAbove = 0;
        int rowsBelow = 0;
        int columnsToTheRight = 0;
        int columnsToTheLeft = 0;

        int currentGemtype = -1;

        
        for (int i = 0; i < nRows; i++)
        {
            // updating element surrounding rows on each iteration
            rowsAbove = i;
            rowsBelow = (nRows-1) - i;

            for (int j = 0; j < nColumns; j++)
            {
                // updating element surrounding columns on each iteration
                columnsToTheLeft = j;
                columnsToTheRight = (nColumns-1) - j;

                currentGemtype = theMatrix[i, j].myGemType;


                // COMBINATIONS CHECKS

                // TOTAL OF 16 CASES
                // 4 per type of movement (there are 4 types of movement, thus 4x4 = 16 cases)



                // checking for combinations to the right (Ex: # 0 # #)
                // first check if we have columns to the right
                if(columnsToTheRight > 2)
                {
                    if((theMatrix[i, (j+2)].myGemType == theMatrix[i, (j+3)].myGemType) && theMatrix[i, (j+2)].myGemType == currentGemtype)
                    {
                        Debug.Log("Case 1 found! at cell: " + i + ", " + j + " of type: " + currentGemtype);
                        return true;
                    }
                }

                // checking for combinations to the left (Ex: # # 0 #)
                // first check if we have columns to the left
                if(columnsToTheLeft > 2)
                {
                    if((theMatrix[i, (j-2)].myGemType == theMatrix[i, (j-3)].myGemType) && theMatrix[i, (j-2)].myGemType == currentGemtype)
                    {
                        Debug.Log("Case 2 found! at cell: " + i + ", " + j + " of type: " + currentGemtype);
                        return true;
                    }
                }

                // checking for combinations above
                //(Ex: # )
                //     #
                //     0
                //     #
                // first check if we have rows above
                if(rowsAbove > 2)
                {
                    if((theMatrix[(i-2), j].myGemType == theMatrix[(i-3), j].myGemType) && theMatrix[(i-2), j].myGemType == currentGemtype)
                    {
                        Debug.Log("Case 3 found! at cell: " + i + ", " + j + " of type: " + currentGemtype);
                        return true;
                    }
                }

                // checking for combinations below
                //(Ex: # )
                //     0
                //     #
                //     #
                // first check if we have rows below
                if(rowsBelow > 2)
                {
                    if((theMatrix[(i+2), j].myGemType == theMatrix[(i+3), j].myGemType) && theMatrix[(i+2), j].myGemType == currentGemtype)
                    {
                        Debug.Log("Case 4 found! at cell: " + i + ", " + j + " of type: " + currentGemtype);
                        return true;
                    }
                }


                // checking for middle combinations to the right
                // (Ex:   #  )
                //      # 0
                //        #
                if(columnsToTheRight >= 1 && rowsAbove >= 1 && rowsBelow >= 1)
                {
                    if((theMatrix[(i-1), (j+1)].myGemType == theMatrix[(i+1), (j+1)].myGemType) && theMatrix[(i-1), (j+1)].myGemType == currentGemtype)
                    {
                        Debug.Log("Case 5 found! at cell: " + i + ", " + j + " of type: " + currentGemtype);
                        return true;
                    }
                }


                // checking for middle combinations to the left
                // (Ex:   #    )
                //        0 #
                //        #
                if(columnsToTheLeft >= 1 && rowsAbove >= 1 && rowsBelow >= 1)
                {
                    if((theMatrix[(i-1), (j-1)].myGemType == theMatrix[(i+1), (j-1)].myGemType) && theMatrix[(i-1), (j-1)].myGemType == currentGemtype)
                    {
                        Debug.Log("Case 6 found! at cell: " + i + ", " + j + " of type: " + currentGemtype);
                        return true;
                    }
                }


                // checking for middle combinations above
                // (Ex: # 0 # )
                //        #
                if(rowsAbove >= 1 && columnsToTheRight >= 1 && columnsToTheLeft >= 1)
                {
                    if((theMatrix[(i-1), (j-1)].myGemType == theMatrix[(i-1), (j+1)].myGemType) && theMatrix[(i-1), (j-1)].myGemType == currentGemtype)
                    {
                        Debug.Log("Case 7 found! at cell: " + i + ", " + j + " of type: " + currentGemtype);
                        return true;
                    }
                }


                // checking for middle combinations below
                // (Ex:   #   )
                //      # 0 #
                if(rowsBelow >= 1 && columnsToTheRight >= 1 && columnsToTheLeft >= 1)
                {
                    if((theMatrix[(i+1), (j-1)].myGemType == theMatrix[(i+1), (j+1)].myGemType) && theMatrix[(i+1), (j-1)].myGemType == currentGemtype)
                    {
                        Debug.Log("Case 8 found! at cell: " + i + ", " + j + " of type: " + currentGemtype);
                        return true;
                    }
                }


                // checking for combinations above to the left
                // (Ex: # # 0 )
                //          #
                if(rowsAbove >= 1 && columnsToTheLeft >= 2)
                {
                    if((theMatrix[(i-1), (j-1)].myGemType == theMatrix[(i-1), (j-2)].myGemType) && theMatrix[(i-1), (j-1)].myGemType == currentGemtype)
                    {
                        Debug.Log("Case 9 found! at cell: " + i + ", " + j + " of type: " + currentGemtype);
                        return true;
                    }
                }


                // checking for combinations above to the right
                // (Ex: 0 # # )
                //      #
                if(rowsAbove >= 1 && columnsToTheRight >= 2)
                {
                    if((theMatrix[(i-1), (j+1)].myGemType == theMatrix[(i-1), (j+2)].myGemType) && theMatrix[(i-1), (j+1)].myGemType == currentGemtype)
                    {
                        Debug.Log("Case 10 found! at cell: " + i + ", " + j + " of type: " + currentGemtype);
                        return true;
                    }
                }


                // checking for combinations below to the left
                // (Ex:     # )
                //      # # 0
                if(rowsBelow >= 1 && columnsToTheLeft >= 2)
                {
                    if((theMatrix[(i+1), (j-1)].myGemType == theMatrix[(i+1), (j-2)].myGemType) && theMatrix[(i+1), (j-1)].myGemType == currentGemtype)
                    {
                        Debug.Log("Case 11 found! at cell: " + i + ", " + j + " of type: " + currentGemtype);
                        return true;
                    }
                }


                // checking for combinations below to the right
                // (Ex: #     )
                //      0 # #
                if(rowsBelow >= 1 && columnsToTheRight >= 2)
                {
                    if((theMatrix[(i+1), (j+1)].myGemType == theMatrix[(i+1), (j+2)].myGemType) && theMatrix[(i+1), (j+1)].myGemType == currentGemtype)
                    {
                        Debug.Log("Case 12 found! at cell: " + i + ", " + j + " of type: " + currentGemtype);
                        return true;
                    }
                }


                // checking for combinations to the left above
                // (Ex: #   )
                //      #
                //      0 #
                if(columnsToTheLeft >= 1 && rowsAbove >= 2)
                {
                    if((theMatrix[(i-1), (j-1)].myGemType == theMatrix[(i-2), (j-1)].myGemType) && theMatrix[(i-1), (j-1)].myGemType == currentGemtype)
                    {
                        Debug.Log("Case 13 found at cell: " + i + ", " + j + " of type: " + currentGemtype);
                        return true;
                    }
                }



                // checking for combinations to the right above
                // (Ex:   # )
                //        #
                //      # 0
                if(columnsToTheRight >= 1 && rowsAbove >= 2)
                {
                    if((theMatrix[(i-1), (j+1)].myGemType == theMatrix[(i-2), (j+1)].myGemType) && theMatrix[(i-1), (j+1)].myGemType == currentGemtype)
                    {
                        Debug.Log("Case 14 found! at cell: " + i + ", " + j + " of type: " + currentGemtype);
                        return true;
                    }
                }


                // checking for combinations to the left below
                // (Ex: 0 #  )
                //      #
                //      #
                if(columnsToTheLeft >= 1 && rowsBelow >= 2)
                {
                    if((theMatrix[(i+1), (j-1)].myGemType == theMatrix[(i+2), (j-1)].myGemType) && theMatrix[(i+1), (j-1)].myGemType == currentGemtype)
                    {
                        Debug.Log("Case 15 found! at cell: " + i + ", " + j + " of type: " + currentGemtype);
                        return true;
                    }
                }


                // checking for combinations to the right below
                // (Ex: # 0  )
                //        #
                //        #
                if(columnsToTheRight >= 1 && rowsBelow >= 2)
                {
                    if((theMatrix[(i+1), (j+1)].myGemType == theMatrix[(i+2), (j+1)].myGemType) && theMatrix[(i+1), (j+1)].myGemType == currentGemtype)
                    {
                        Debug.Log("Case 16 found! at cell: " + i + ", " + j + " of type: " + currentGemtype);
                        return true;
                    }
                }
                
                // if any of these checks are satisfied we can return true right away
            }
        }

        return false;
    }

    public void ShuffleAndCheckGrid()
    {
        // reset the possibleToMove value for it to shuffle at least one time
        possibleToMove = false;

        while(!possibleToMove)
        {
            GenerateGrid();

            possibleToMove = PossibleToMove();
            Debug.Log("Are there any possible moves?  " + possibleToMove);
        }
    }

}



