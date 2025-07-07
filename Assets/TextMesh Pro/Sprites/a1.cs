using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class a1 : MonoBehaviour
{
    // Public variables for chair prefabs, ghost material, and default placement distance
    // public GetMeshes gm;
    public List<GameObject> chairPrefabs = new List<GameObject>();
    public GameObject abstractChairPrefab;
    public Material ghostMaterial;

    private GameObject currentGhostPlacer;         
    private List<GameObject> placedChairs = new List<GameObject>();
    private int currentChairIndex = 0;
    private float maxDistance = 5f;
    void Start()
    {
        CreateGhostPlacer();
    }

    void Update()
    {
        UpdateGhostPlacer();   // Update ghost position and rotation
        HandlePlacement();     // Handle placement of chairs on right-click
        HandleScroll();        // Switch between chair prefabs with scroll wheel
        HandleCancel();        // Cancel placement with left-click and destroy chair
    }


    // Initializes the ghost placer object that will follow the mouse
    void CreateGhostPlacer()
    {
        currentGhostPlacer = new GameObject("GhostPlacer");
        ReplaceGhostMesh();  // Attach the selected chair mesh to the ghost placer
    }

    // Updates the ghost placer position and rotation based on the crosshair's intersection with the ground
    void UpdateGhostPlacer()
    {
        if (currentGhostPlacer != null)
        {
            Vector3 targetPosition = GetCrosshairWorldPosition();
            currentGhostPlacer.SetActive(targetPosition != Vector3.zero);  // Enable/disable ghost placer based on position validity
            Quaternion targetRotation = GetCrosshairRotation();
            currentGhostPlacer.transform.position = targetPosition;
            currentGhostPlacer.transform.rotation = targetRotation;
        }
    }

    // Places the chair when right-click is pressed
    void HandlePlacement()
    {
        if (Input.GetMouseButtonDown(1))  // Right-click
        {
            Vector3 spawnPosition = currentGhostPlacer.transform.position;
            Quaternion spawnRotation = currentGhostPlacer.transform.rotation;
            if (spawnPosition == Vector3.zero) return;  // Do not place if position is invalid
            GameObject newChair = Instantiate(abstractChairPrefab, spawnPosition, spawnRotation);
            placedChairs.Add(newChair);
            InstantiateChairMesh(newChair);  // Instantiate mesh for the placed chair
        }
    }

    // Scrolls to change the selected chair prefab
    void HandleScroll()
    {
        if (currentGhostPlacer != null)
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0)
            {
                currentChairIndex += (scroll > 0) ? 1 : -1;
                currentChairIndex = (currentChairIndex + chairPrefabs.Count) % chairPrefabs.Count;
                ReplaceGhostMesh();  // Update ghost mesh when scrolling
            }
        }
    }

    // Cancels placement by destroying the object hit by the raycast if within max distance
    void HandleCancel()
    {
        if (Input.GetMouseButtonDown(0))  // Left-click
        {
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));  // Ray from crosshair
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit) && hit.distance <= maxDistance)
            {
                Transform hitTransform = hit.collider.transform;
                if (placedChairs.Contains(hitTransform.gameObject))
                {
                    placedChairs.Remove(hitTransform.gameObject);
                    Destroy(hitTransform.gameObject);
                }
            }
        }
    }

    // Returns the world position where the crosshair intersects the ground plane
    Vector3 GetCrosshairWorldPosition()
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));  // Ray from center of screen
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);  // Ground plane at y = 0
        
        float distance;
        if (groundPlane.Raycast(ray, out distance) && distance <= maxDistance)
        {
            return ray.GetPoint(distance);
        }
        return Vector3.zero;  // Return zero if no valid position
    }

    // Returns the desired rotation for the ghost placer, aligned with the camera's forward direction
    Quaternion GetCrosshairRotation()
    {
        Quaternion angle = Quaternion.LookRotation(Camera.main.transform.forward);
        return Quaternion.Euler(0, angle.eulerAngles.y - 180, 0);  // Lock rotation to the Y-axis only
    }

    // Instantiates the actual chair mesh at the placed position
    void InstantiateChairMesh(GameObject parent)
    {
        GameObject chairMesh = Instantiate(chairPrefabs[currentChairIndex], parent.transform);
        chairMesh.transform.localPosition = Vector3.zero;  // Place the mesh at the parent’s origin
    }

    // Replaces the ghost mesh with the selected chair prefab
    void ReplaceGhostMesh()
    {
        // Remove any existing ghost meshes
        foreach (Transform child in currentGhostPlacer.transform)
        {
            Destroy(child.gameObject);
        }

        // Instantiate a new ghost mesh from the selected prefab
        GameObject ghostMesh = Instantiate(chairPrefabs[currentChairIndex], currentGhostPlacer.transform);
        ghostMesh.transform.localPosition = Vector3.zero;
        // ApplyGhostMaterial(ghostMesh);  // Apply transparency effect to the ghost mesh
    }

    // Applies the ghost material to all renderers of the given object to make it semi-transparent
    void ApplyGhostMaterial(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            renderer.material = ghostMaterial;
        }
    }
}