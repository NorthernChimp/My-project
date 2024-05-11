using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainScript : MonoBehaviour
{
    // Start is called before the first frame update
    public static float brickWidth;
    public static Vector3 brickScale;
    GameObject tile;
    List<List<Tile>> tileLayout;
    List<RunnerSegment> segments;
    List<Actor> actors = new List<Actor>();
    int currentSegmentXRef = 0;//the segment reference for the last tile pushed to the furthest right in the game
    void Start()
    {
        SetupGame();
    }
    void SetupGame()
    {
        float screenHeight = Screen.height * 0.01f; //the height of hte entire screen in unity metres
        brickWidth = screenHeight / 24f;//have 24 bricks depending on height
        float scaleWidth = brickWidth/0.32f;
        tile = Resources.Load("Tile") as GameObject;
        brickScale = new Vector3(scaleWidth, scaleWidth, 1f);//the z value doesn't matter. this is the base scale for every object in teh game when its created. you may want an object to be bigger, but make it bigger based on this value, like brickscale * 1.5 for 50% bigger etc
        int totalBackgroundWidth = (int)((Screen.width * 0.01f) / brickWidth);
        totalBackgroundWidth += 5;//add 3 just to be safe
        Camera.main.orthographicSize = Screen.height * 0.005f;
        Vector3 origin = Vector3.left * (totalBackgroundWidth / 2) * brickWidth + Vector3.down * 11.5f * brickWidth;
        segments = new List<RunnerSegment>();
        int totalSegmentWidth = 0;
        while(totalSegmentWidth <= totalBackgroundWidth)  //you have to fill up the first screen with randomized segments
        {
            int randomWidth = (int)Random.Range(5f, 35f);//randomly select a width for the new segment (must be between 30 and 120, this can change later)
            segments.Add(new RunnerSegment().SetupSegment(randomWidth, origin + Vector3.right * totalSegmentWidth * brickWidth)); //setup the runner segment after creating it
            foreach(RunnerSegment r in segments){   ProcessGameEvents(r.GetSegmentEvents());} //go through the list of segments and get all the relevant game events they require
            totalSegmentWidth += segments[segments.Count - 1].GetSegmentWidth();  //update the total segment width of all the segments
        }
        tileLayout = new List<List<Tile>>();
        for (int y = 0; y < 24; y++)                        //for each block vertically
        {
            tileLayout.Add(new List<Tile>());               //create a new list of tiles
            for (int x = 0; x < totalBackgroundWidth; x++)  // go through every tile horizontally
            {
                Vector3 pos = origin + Vector3.right * x * brickWidth + Vector3.up * y * brickWidth; //this tiles positiong
                Tile t = CreateTile(pos);                                                           //create a tile at that positiong
                int tempX = x;                                              //presume the tiles x reference to be the same as x
                bool hasFoundProperSegment = false;                         //make a bool for confirming we have the x value for the relevant segment
                int segmentsRef = 0;                                        //start with segment ref 0, the first of the segments
                for(int o = 0; !hasFoundProperSegment; o++)
                {
                    segmentsRef = o;                                        //assume the current segment is the proper reference
                    if (segments[o].IsPointWithinSegmentWidth(tempX))       //if the x reference is within the current segment then we have found the proper reference
                    {
                        hasFoundProperSegment = true;                       //segments ref is the proper segment reference now (exit the for loop)
                    }
                    else
                    {
                        tempX -= segments[o].GetSegmentWidth();             //if the x reference is not inside of that segment then the x ref has to minus this segments width and start over on the next segment, minus the segment width and move on to the next segment (by repeating the for loop)
                    }
                }
                t.SetupTile(segments[segmentsRef].GetMapAtPoint(tempX,y));  //using segment ref to get the proper segment this tile belongs to and the temp x value which represents the x value of this tile in that segment and the y value
                tileLayout[y].Add(t);                                       //add it to the layout for later reference
            }
        }
        currentSegmentXRef = totalBackgroundWidth;                          //get the current segment xRef set it to the total backgroundwidth
        bool hasUpdatedXRef = false;                                        //make a bool for when it is the proper value for the current segment
        while (!hasUpdatedXRef)                     
        {
            if (segments[0].IsPointWithinSegmentWidth(currentSegmentXRef)) { hasUpdatedXRef = true; }    //if the total width is in the first segment then there is no other segments and the currentsegmentxref is accurate
            else { currentSegmentXRef -= segments[0].GetSegmentWidth();  segments.RemoveAt(0);  }       //if the segment doesn't contain this x value then remove that segments width and remove it from the list and repeat the while loop on the next segment.
        }
    }
    Tile CreateTile(Vector3 pos)
    {
        Transform t = Instantiate(tile, pos, Quaternion.identity).transform;
        t.localScale = brickScale;
        Tile til = t.GetComponent<Tile>();
        return til;
    }
    void CreateNextSegment()
    {
        segments.Add(new RunnerSegment().SetupSegment((int)Random.Range(5f, 30f), segments[0].GetNextOrigin()));
        currentSegmentXRef -= segments[0].GetSegmentWidth(); segments.RemoveAt(0);
        ProcessGameEvents(segments[0].GetSegmentEvents());
    }
    void ProcessGameEvents(List<GameEvent> events)
    {
        while(events.Count > 0)
        {
            GameEvent e = events[0];
            switch (e.GetEventType())
            {
                case GameEventType.createActor:CreateActor(e); break;
            }
            events.RemoveAt(0);
        }
    }
    void CreateActor(GameEvent e)
    {
        Vector3 pos = e.GetPos();
        string prefabName = e.GetPrefabName();
        GameObject prefab = Resources.Load(prefabName) as GameObject;
        Transform t = Instantiate(prefab, pos, Quaternion.identity).transform;
        Actor a = t.GetComponent<Actor>();
        List<GameEvent> temp = a.SetupActor();
        actors.Add(a);
        ProcessGameEvents(temp);
    }
    void CheckIfTilesHavePassedScreenLeft()
    {
        float xPos = tileLayout[0][0].transform.position.x;//only x pos matters so we only need to take one tile's xpos to deteremine if the rest in teh column need to be moved
        float leftDiff = transform.position.x - xPos;       //get the x difference from the camera
        if(Mathf.Abs(leftDiff) > Screen.width * 0.0055f)    //if its enough to be beyond the left side of the screen
        {
            //TileType typeOfTile = TileType.none;
            currentSegmentXRef++;
            for(int i =0; i < 24; i++)
            {
                if (segments[0].IsPointWithinSegmentWidth(currentSegmentXRef))  //if the current segment x ref is within the current segment then we dont need to do anything
                { }         
                else{ CreateNextSegment(); }                                    //otherwise we must create a new segment. the old segmen isn't needed any more
                TileType typeOfTile = segments[0].GetMapAtPoint(currentSegmentXRef, i);         //now that we know segments[0] is the proper segment get the type of tile at the current xsegment ref and the appropriate y value (i)
                Tile t = tileLayout[i][0];                                                  //makea variable for this tile
                tileLayout[i].RemoveAt(0);                                                  //remove it from the 0 positiong
                tileLayout[i].Add(t);                                                       //add it back on the top of the list
                t.SetupTile(typeOfTile);                                                    //set it up as the proper tile type
                t.transform.Translate(Vector3.right * brickWidth * tileLayout[0].Count,Space.World);    //and move it to where that tile should be in the real world
            }
        }
    }
    private void FixedUpdate()
    {
        transform.Translate(Vector3.right * Time.fixedDeltaTime * 0.75f, Space.World);
        CheckIfTilesHavePassedScreenLeft();

    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
