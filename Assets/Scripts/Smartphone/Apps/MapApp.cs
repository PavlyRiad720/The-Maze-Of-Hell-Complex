using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapApp : SmartphoneBaseApp
{
    [System.Serializable]
    public class MapSector
    {
        public string sectorID;
        public Collider sectorTriggerZone;
        public Image sectorMapUIOverlay;
        public bool isDiscovered;
    }

    [Header("Map Grid Sector")]
    public List<MapSector> mapSectors = new List<MapSector>();

    [Header("Player Tracking")]
    public Transform playerTransform;
    public RectTransform playerIconOnMap;
    public Vector2 mapWorldBoundsMin;
    public Vector2 mapWorldBoundsMax;

    private void Update()
    {
        if (!IsOpen) return;

        UpdatePlayerMapPosition();
        CheckPlayerSectorDiscovery();
    }

    private void UpdatePlayerMapPosition()
    {
        if (playerTransform == null || playerIconOnMap == null) return;

        // Map world position to Normalized 0..1 Map UI space
        float normX = Mathf.InverseLerp(mapWorldBoundsMin.x, mapWorldBoundsMax.x, playerTransform.position.x);
        float normY = Mathf.InverseLerp(mapWorldBoundsMin.y, mapWorldBoundsMax.x, playerTransform.position.z);

        RectTransform parentRect = playerIconOnMap.parent.GetComponent<RectTransform>();
        if (parentRect != null)
        {
            float uiX = (normX * parentRect.rect.width) - (parentRect.rect.width * 0.5f);
            float uiY = (normY * parentRect.rect.height) - (parentRect.rect.height * 0.5f);
            playerIconOnMap.anchoredPosition = new Vector2(uiX, uiY);
        }
    }

    private void CheckPlayerSectorDiscovery()
    {
        foreach (var sector in mapSectors)
        {
            if (!sector.isDiscovered && sector.sectorTriggerZone != null)
            {
                if (sector.sectorTriggerZone.bounds.Contains(playerTransform.position))
                {
                    DiscoverSector(sector.sectorID);
                }
            }
        }
    }

    public void DiscoverSector(string sectorID)
    {
        MapSector sector = mapSectors.Find(s => s.sectorID == sectorID);
        if (sector != null && !sector.isDiscovered)
        {
            sector.isDiscovered = true;
            if (sector.sectorMapUIOverlay != null)
            {
                sector.sectorMapUIOverlay.gameObject.SetActive(true);
            }
        }
    }

    // --- Unlock full map via Item Pickups ---
    public void RevealFullMapLayout()
    {
        foreach (var sector in mapSectors)
        {
            DiscoverSector(sector.sectorID);
        }
    }
}
