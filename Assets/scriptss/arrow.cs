using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class Arrow : MonoBehaviour
{
    [SerializeField] private Camera panelCamera;

    public List<GameObject> chairPrefabs = new List<GameObject>();

    public GameObject abstractChairPrefab;

    private GameObject currentGhostPlacer;
    private List<GameObject> placedChairs = new List<GameObject>();
    private int currentChairIndex = 0;

    public GameObject floor;

    public GameObject canvas1;

    private Camera mainCamera;
    public RectTransform panel;
    public ScrollRect scrollRect;

    public FirstPersonController Bob; // Assign this in the Inspector or find it dynamically
    private Rigidbody bobBody;
    private int selectedIndex = 0;
    private GameObject[] items;
    private float maxDistance = 100f;

    private int prev = -1;
    private int curr = -1;
    //    public GameObject Selected;
    public float moveSpeed = 5f;

    private Asset asset = new Asset();
    MeshClass meshClass;

    void Start()
    {
        panelCamera = GameObject.FindGameObjectWithTag("PanelCamera")?.GetComponent<Camera>();
        // Find Bob if not assigned in the Inspector
        if (Bob == null)
        {
            Bob = FindObjectOfType<FirstPersonController>();
        }

        // Get the Rigidbody component from Bob
        bobBody = Bob.GetComponent<Rigidbody>();
        if (bobBody == null)
        {
            Debug.LogError("Bob does not have a Rigidbody component!");
        }

        // Disable the canvas at the start
        if (canvas1 != null)
        {
            canvas1.gameObject.SetActive(false);
        }

        if (panel != null)
        {
            items = new GameObject[panel.childCount];
            for (int i = 0; i < panel.childCount; i++)
            {
                items[i] = panel.GetChild(i).gameObject;
            }
        }
    }

    void Update()
    {
        // Ensure all required references are valid
        DoPanelThing();
        HandleCancel();
    }

    void PlacePanelInFrontOfCamera()
    {
        // Get the camera's position and forward direction
        Vector3 cameraPosition = panelCamera.transform.position;
        Vector3 cameraForward = panelCamera.transform.forward;
        Vector3 cameraRight = panelCamera.transform.right;

        // Calculate the position in front of the camera and slightly to the right
        float distanceFromCamera = 1f; // Distance from the camera
        float rightOffset = 1f; // Offset to the right
        Vector3 panelPosition = cameraPosition + cameraForward * distanceFromCamera + cameraRight * rightOffset;

        // Convert the world position to screen space
        Vector3 screenPosition = panelCamera.WorldToScreenPoint(panelPosition);

        // Set the panel's position
        panel.position = screenPosition;
    }

    void UpdateSelection()
    {
        // Highlight the selected item
        for (int i = 0; i < items.Length; i++)
        {
            if (i == selectedIndex)
            {
                items[i].GetComponent<Image>().color = Color.yellow; // Highlight selected item
            }
            else
            {
                items[i].GetComponent<Image>().color = Color.white; // Reset other items
            }
        }

        // Ensure the selected item is visible in the scroll view
        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 1 - (selectedIndex / (float)(items.Length - 1));
        }
    }
    
    /*
        Vector3 GetCrosshairWorldPosition()
        {
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));  // Ray from center of screen
            Vector3 groundPos = floor.transform.position + Vector3.up;
            groundPos.y += 1f;
            Plane groundPlane = new Plane(Vector3.up, groundPos);
            float distance;
            Vector3 floorPoint = Vector3.zero;

            if (groundPlane.Raycast(ray, out distance) && distance <= maxDistance)
            {
                floorPoint = ray.GetPoint(distance);
            }
            if (floorPoint != Vector3.zero)
            {
                return floorPoint;
            }
            return Vector3.zero;

        }
    */

    Vector3 GetCrosshairWorldPosition()
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)); // Ray from the center of the screen
        RaycastHit hit;
    
        // Perform a raycast to detect objects
        if (asset.assetType == Asset.AssetType.TableLamps || asset.assetType == Asset.AssetType.PhotoFrame || asset.assetType == Asset.AssetType.Fans)
        {
            if( Physics.Raycast(ray, out hit, maxDistance) && (asset.assetType == Asset.AssetType.TableLamps || asset.assetType == Asset.AssetType.Walls))
            {
                Vector3 placementPosition = hit.point + Vector3.up * 0.05f; // Adjust the height offset as needed
                return placementPosition;
            }
            

            else if (Physics.Raycast(ray, out hit, maxDistance) && asset.assetType == Asset.AssetType.PhotoFrame)
            {
                if (hit.collider.CompareTag("Wall"))
                {
                    Vector3 placementPosition = hit.point; //+ Vector3.down * 0.1f;
                    return placementPosition;
                }

                return Vector3.zero;
            }
            else if (Physics.Raycast(ray, out hit, 120f) && asset.assetType == Asset.AssetType.Fans)
            {
                if (hit.collider.CompareTag("Roof"))
                {
                    Vector3 placementPosition = hit.point;// + hit.normal * 0.1f;
                    return placementPosition;
                }

                return Vector3.zero;
            }
            //return hit.point;
        }

        Vector3 groundPos = floor.transform.position + Vector3.up;
        Plane groundPlane = new Plane(Vector3.up, groundPos);
        float distance;
        Vector3 floorPoint = Vector3.zero;

        if (groundPlane.Raycast(ray, out distance) && distance <= maxDistance)
        {
            floorPoint = ray.GetPoint(distance);
            /*
            float overlapRadius = 0.5f; // Adjust based on object size
            Collider[] colliders = Physics.OverlapSphere(floorPoint, overlapRadius);
            if (colliders.Length == 0)
            {
                return floorPoint;
            }
            */
        }
        if (floorPoint != Vector3.zero && IsPositionValid(floorPoint))
        {
            return floorPoint;
        }
        return Vector3.zero;
    
    }

    bool IsPositionValid(Vector3 position)
    {
        Collider[] colliders = Physics.OverlapSphere(position, 0.3f);
        return colliders.All(c => c.isTrigger || c.gameObject == currentGhostPlacer );
    }

    void GetAssetType()
    {
        switch (selectedIndex)
        {
            case 0:
                asset.assetType = Asset.AssetType.Chairs;
                curr = 0;
                break;
            case 1:
                asset.assetType = Asset.AssetType.Sofas;
                curr = 1;
                break;
            case 2:
                asset.assetType = Asset.AssetType.Tables;
                curr = 2;
                break;
            case 3:
                asset.assetType = Asset.AssetType.Beds;
                curr = 3;
                break;
            case 4:
                asset.assetType = Asset.AssetType.TableLamps;
                curr = 4;
                break;
            case 5:
                asset.assetType = Asset.AssetType.PhotoFrame;
                curr = 5;
                break;
            case 6:
                asset.assetType = Asset.AssetType.Fans;
                curr = 6;
                break;
            case 7:
                asset.assetType = Asset.AssetType.Walls;
                curr = 7;
                break;
            case 8:
                asset.assetType = Asset.AssetType.Ground;
                curr = 8;
                break;
        }
        PermissibleLocation permissibleLocation = new PermissibleLocation();
        permissibleLocation.permissibleType = PermissibleLocation.PermissibleType.Floor;
        bool isLuminated = false;
        if (asset.assetType == Asset.AssetType.TableLamps)
        {
            isLuminated = true;
        }
        meshClass = new MeshClass(asset, isLuminated, permissibleLocation);
        chairPrefabs = meshClass.prefabs;

        if (prev != curr && currentGhostPlacer != null)
        {
            prev = curr;
            currentChairIndex = 0;
            ReplaceGhostMesh();
            // UpdateGhostPlacer();
        }
        UpdateGhostPlacer();
    }

    void DoPanelThing()
    {
        if (canvas1 != null && panel != null && Camera.main != null)
        {
            // Toggle the canvas when pressing the 'I' key
            if (Input.GetKeyDown(KeyCode.I))
            {
                canvas1.gameObject.SetActive(!canvas1.activeSelf); // Toggle UI

                if (canvas1.activeSelf)
                {
                    // Calculate the position for the panel
                    PlacePanelInFrontOfCamera();

                    // Select the first item by default
                    UpdateSelection();
                    CreateGhostPlacer();
                }
            }
            if (canvas1.activeSelf)
            {
                // Scroll using the mouse wheel
                float scrollInput = Input.GetAxis("Mouse ScrollWheel");
                if (scrollInput > 0) // Scrolling up
                {
                    selectedIndex = Mathf.Max(0, selectedIndex - 1);
                    UpdateSelection();
                }
                else if (scrollInput < 0) // Scrolling down
                {
                    selectedIndex = Mathf.Min(items.Length - 1, selectedIndex + 1);
                    UpdateSelection();
                }
                GetAssetType();
                UpdateGhostPlacer();
                ConfirmSelection();
                HandleNextPrefab();

                // Confirm selection with the Enter key
            }

            if (!canvas1.activeSelf)
            {
                DestroyGhostPlacer();
            }

        }
        else
        {
            Debug.LogError("One or more required references are missing!");
        }
    }

    void CreateGhostPlacer()
    {
        currentGhostPlacer = new GameObject("GhostPlacer");
        ReplaceGhostMesh();
    }

    void DestroyGhostPlacer()
    {
        if (currentGhostPlacer != null)
        {
            foreach (Transform child in currentGhostPlacer.transform)
            {
                Destroy(child.gameObject);
            }
        }
    }

    void HandleNextPrefab()
    {
        if (currentGhostPlacer != null)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                currentChairIndex++;
                currentChairIndex = (currentChairIndex + chairPrefabs.Count) % chairPrefabs.Count;
                ReplaceGhostMesh();
            }
            if (Input.GetKeyDown(KeyCode.Q))
            {
                currentChairIndex--;
                currentChairIndex = (currentChairIndex + chairPrefabs.Count) % chairPrefabs.Count;
                ReplaceGhostMesh();
            }
        }
    }

    void ReplaceGhostMesh()
    {
        if (chairPrefabs == null || chairPrefabs.Count == 0)
        {
            Debug.LogError("Chair prefabs list is empty or null!");
            return;
        }
        foreach (Transform child in currentGhostPlacer.transform)
        {
            Destroy(child.gameObject);
        }


        GameObject ghostMesh = Instantiate(chairPrefabs[currentChairIndex], currentGhostPlacer.transform);
        ghostMesh.transform.localPosition = Vector3.zero;
    }

    void ConfirmSelection()
    {
        //Right-click
        if (Input.GetMouseButtonDown(1))  
        {
            Vector3 spawnPosition = currentGhostPlacer.transform.position;
            Quaternion spawnRotation = currentGhostPlacer.transform.rotation;
            if (spawnPosition == Vector3.zero) return; 
            GameObject newChair = Instantiate(abstractChairPrefab, spawnPosition, spawnRotation);
            placedChairs.Add(newChair);
            InstantiateChairMesh(newChair);  
        }

    }

    void InstantiateChairMesh(GameObject parent)
    {
        GameObject chairMesh = Instantiate(chairPrefabs[currentChairIndex], parent.transform);

        if (meshClass.isIlluminated)
        {
            ApplyIlluminationEffect(chairMesh);
        }
        chairMesh.transform.localPosition = Vector3.zero;
    }

    void HandleCancel()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, maxDistance) && hit.distance <= maxDistance)
            {
                Transform hitParent = hit.collider.transform.root;
                if (placedChairs.Contains(hitParent.gameObject))
                {
                    placedChairs.Remove(hitParent.gameObject);
                    Destroy(hitParent.gameObject);
                    Debug.Log("Deleted: " + hitParent.name);
                }

                else
                {
                    Debug.LogWarning("Hit object not in placedChairs list: " + hitParent.name);
                }

            }
        }
    }

    /*
        void UpdateGhostPlacer()
        {
            if (currentGhostPlacer != null)
            {
                GameObject newObj = Instantiate(prefab, position, rotation);
                Renderer objRenderer = newObj.GetComponent<Renderer>();

                if (objRenderer != null)
                {
                    float height = objRenderer.bounds.size.y;
                    Debug.Log("Object height: " + height);

                    // Example: Place the object on the ground
                    newObj.transform.position = new Vector3(
                        newObj.transform.position.x,
                        height / 2,  // Adjust if pivot is at center
                        newObj.transform.position.z
                    );
                }
                // Get the target position from the crosshair raycast
                Vector3 targetPosition = GetCrosshairWorldPosition();

                // Activate/deactivate the ghost based on validity
                currentGhostPlacer.SetActive(targetPosition != Vector3.zero);

                if (targetPosition != Vector3.zero)
                {
                    // Adjust position for the Box Collider (if present)
                    BoxCollider boxCollider = currentGhostPlacer.GetComponent<BoxCollider>();
                    if (boxCollider != null)
                    {
                        // Offset the Y position so the bottom of the collider sits on the hit point
                        targetPosition.y += boxCollider.size.y * 0.5f * currentGhostPlacer.transform.lossyScale.y;
                    }

                    // Get rotation aligned with the surface normal

                    // Update the ghost's position and rotation
                    currentGhostPlacer.transform.position = targetPosition;
                    Quaternion targetRotation = GetCrosshairRotation();

                    if (asset.assetType == Asset.AssetType.PhotoFrame)
                    {
                        Vector3 euler = targetRotation.eulerAngles;
                        euler.y = Mathf.Round(euler.y / 90) * 90;
                        targetRotation = Quaternion.Euler(euler);
                    }
                    currentGhostPlacer.transform.rotation = targetRotation;

                }
            }
        }
    */
    void UpdateGhostPlacer()
    {
        if (currentGhostPlacer == null) return;

        Vector3 targetPosition = GetCrosshairWorldPosition();

        //currentGhostPlacer.SetActive(targetPosition != Vector3.zero);

        if (targetPosition != Vector3.zero)
        {
            // Calculate the object's height using its Renderer
            Renderer objRenderer = currentGhostPlacer.GetComponent<Renderer>();
            BoxCollider boxCollider = currentGhostPlacer.GetComponentInChildren<BoxCollider>();
            float heightOffset = 0f;

            if (boxCollider != null)
            {
                heightOffset = boxCollider.size.y * 0.5f * currentGhostPlacer.transform.lossyScale.y;
            }
            else if (objRenderer != null)
            {
                heightOffset = objRenderer.bounds.extents.y;
            }
            targetPosition.y += heightOffset;
            
            // Get rotation aligned with the surface normal
            Quaternion targetRotation = GetCrosshairRotation();
            // Snap to 90-degree increments for Photo Frames
            if (asset.assetType == Asset.AssetType.PhotoFrame || asset.assetType == Asset.AssetType.Walls || asset.assetType == Asset.AssetType.Ground)
            {
                Vector3 euler = targetRotation.eulerAngles;
                euler.y = Mathf.Round(euler.y / 90) * 90;
                targetRotation = Quaternion.Euler(euler);
            }
            // Update the ghost's position and rotation
            currentGhostPlacer.transform.position = targetPosition;
            currentGhostPlacer.transform.rotation = targetRotation;
        }
    }

    Quaternion GetCrosshairRotation()
    {
        Quaternion angle = Quaternion.LookRotation(Camera.main.transform.forward);
        return Quaternion.Euler(0, angle.eulerAngles.y - 180, 0);
    }
    void ApplyIlluminationEffect(GameObject obj)
    {
        Light light = obj.AddComponent<Light>();
        light.type = LightType.Point;
        light.range = 20f;
        light.intensity = 20f;
        light.color = Color.yellow;
        light.transform.localPosition = new Vector3(0, 1, 0);
    }

    
}