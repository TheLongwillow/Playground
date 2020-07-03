﻿using System.Collections;
using System.Collections.Generic;
using BezierSolution;
using UnityEngine;
using System.Linq;
using UnityEngine.EventSystems;
public class Rail : InteractibleBase
{
    [SerializeField]SplineManager splineManager;
    RailManager railManager;
    ObjectPlacementManager placementManager;
    ObjectChooser objectChooser;

    int currentRailWayOption;// active way

    bool isSearching;
    public bool isFirst, collidingWithInteractible;
    // Bir sonraki rayların bağlanabileceği noktaların serisi
    [SerializeField]RailConnectionPoint[] connectionPoints;
    [SerializeField] GameObject[] railTracks;
    void Start()
    {
        railManager = FindObjectOfType<RailManager>();
        placementManager = FindObjectOfType<ObjectPlacementManager>();
        objectChooser = FindObjectOfType<ObjectChooser>();

        currentRailWayOption = 1;
        //Eğer static değilse
        if(!isStatic){
            //ilk raymı ona bak
            isFirst = railManager.IsFirstRail();
        }
    }
    public void OnCollisionCallBack( CollidableBase collidedObject)
    {
        if( lastCollided == null || (collidedObject.GetHashCode() != lastCollided.GetHashCode()) )
        {
            lastCollided =  collidedObject;
            GetComponent<Animator>().Play("InteractibleCollision");
            if(!this.isStatic) // çarpıştığım obje statik ve ben değilsem
            {
                if(railManager.GetLastEditedRail() == null || railManager.GetLastEditedRail() != this) // kıpırdadım mı
                {   
                    // hayır
                    if(this.creationTime > collidedObject.creationTime) // ben yeni mi yerleştim
                    {
                        //siliniyorum
                        Destroy();
                    }
                }
                else if(railManager.GetLastEditedRail() != null && railManager.GetLastEditedRail() == this)
                {
                    // kıpırdamışım
                    // geri yeri me dönüyorum
                    railManager.GetRailBackToOldPosition();
                }      
            }   
        }     
    }
    public Rail GetNextRail()
    {
        return connectionPoints[currentRailWayOption].connectedPoint.rail;
    }
    public bool HasNextRail()
    {
        if(connectionPoints[currentRailWayOption].connectedPoint != null)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    // This will increment currentWayOption 
    // And will call setSpline
    public void ChangeCurrentOption()
    {
        currentRailWayOption++;
        if(currentRailWayOption >= connectionPoints.Length )
        {
            currentRailWayOption = 1;
        }
        else if (currentRailWayOption < 0){
            currentRailWayOption = connectionPoints.Length - 1;
        }

        ShowActiveTrack();

        splineManager.SetSpline(currentRailWayOption-1);
    }
    // shows current selected way track
    public void ShowActiveTrack()
    {
        // hangi yolun seçili olduğunu gösteren tracki aktif et
        for (int i = 0; i < railTracks.Length; i++)
        {
            if(i == currentRailWayOption -1)
                railTracks[i].SetActive(true);
            else
                railTracks[i].SetActive(false);
        }
    }
    // hides current selected way track
    public void HideTracks()
    {
        for (int i = 0; i < railTracks.Length; i++)
        {
            railTracks[i].SetActive(false);
        }
    }
    /// <summary>
    /// Delete rail properly
    /// </summary>
    public override void Destroy()
    {
        // If this rail is static you cant delete it 
        if(isStatic)
            return;

        CleanConnections();

        // Remove from list
        railManager.RemoveRail(this);

        Destroy(gameObject);    
    }
    // nulls all connections
    public void CleanConnections()
    {
        //Clean connectionPoints, connectedPoints 
        foreach (RailConnectionPoint item in connectionPoints)
        {
            if(item.connectedPoint != null)
            {
                item.connectedPoint.connectedPoint = null;
                item.connectedPoint = null;
            }
        }
    }
    public void Search()
    {
        isSearching = true;
    }

    public override void  Glow( bool b)
    {
        if(b)
        {
            mesh.material.SetInt("Vector1_114B864B", 3);
        }
        else{
            mesh.material.SetInt("Vector1_114B864B", 0);
        }
    }
    
    // gets selected connectionPoint
    public RailConnectionPoint GetCurrentConnectionPoint()
    {
        return connectionPoints[currentRailWayOption];
    }
    //All Connection points
    public RailConnectionPoint[] GetConnectionPoints()
    {
        return connectionPoints;
    }
    // Conection Points with no connection
    public RailConnectionPoint[] GetFreeConnectionPoints()
    {
        return connectionPoints.Where(s => s.connectedPoint == null).ToArray();
    }
    // Highlights all points
    public int HighlightConnectionPoints()
    {
        int i = 0;
        // highlight only available point ( doesnt have next or previousRail)
        foreach (RailConnectionPoint item in GetFreeConnectionPoints())
        {
            i++;
            item.Highlight();
        }
        return i;
        // rs listesini highlight et //
    }
    // highlights given points
    public int HighlightConnectionPoints(RailConnectionPoint[] rs)
    {
        int i = 0;
        // highlight only available point ( doesnt have next or previousRail)
        foreach (RailConnectionPoint item in rs)
        {
            i++;
            item.Highlight();
        }
        return i;   
    }
    //Downlight points
    public void DownlightConnectionPoints()
    {
        // Downlight highlighted points
        foreach (RailConnectionPoint item in GetConnectionPoints().Where(h => h.isHighlighted))
        {
            item.Downlight();
        }    
    }
}