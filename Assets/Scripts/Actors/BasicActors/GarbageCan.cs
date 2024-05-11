using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GarbageCan : MonoBehaviour,Actor
{
    public List<GameEvent> SetupActor()
    {
        List<GameEvent> temp = new List<GameEvent>();

        return temp;
    }
    public Transform GetTransform()
    {
        transform.localScale = MainScript.brickScale;
        return transform;
    }

}
