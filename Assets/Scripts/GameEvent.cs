using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEvent 
{
    GameEventType ty; public GameEventType GetEventType() { return ty; }
    int amt; public int GetAmt() { return amt; }
    int dmg; public int GetDmg() { return dmg; }
    int reference; public int GetReference() { return reference; }
    float f; public float GetFloat() { return f; }
    string prefabName; public string GetPrefabName() { return prefabName; }
    Vector3 pos; public Vector3 GetPos() { return pos; }
    GameEvent(GameEventType pe,int amou)
    {
        ty = pe;
        amt = amou;
    }
    public static GameEvent CreateActorEvent(string prefab,Vector3 pos) { return new GameEvent(GameEventType.createActor, prefab,pos); }
    GameEvent(GameEventType pe,string s,Vector3 p)
    {
        pos = p;
        prefabName = s;
        ty = pe;
    }
}
public enum GameEventType { none,createActor,createEffect,removeActor}
