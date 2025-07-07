using UnityEngine;
using UnityEngine.UI; 

// Dataclass Layer
public class ArrowData
{
    public GameObject[] Items { get; private set; }
    public int SelectedIndex { get; set; }
    public bool ObjSelected { get; set; }
    public float MoveSpeed { get; private set; }
    public GameObject Canvas { get; private set; }
    public RectTransform Panel { get; private set; }
    public ScrollRect ScrollRect { get; private set; }
    public Rigidbody BobBody { get; private set; }
    public GameObject[] Prefabs { get; private set; }

    public ArrowData(GameObject canvas, RectTransform panel, ScrollRect scrollRect, Rigidbody bobBody, GameObject[] prefabs, float moveSpeed)
    {
        Canvas = canvas;
        Panel = panel;
        ScrollRect = scrollRect;
        BobBody = bobBody;
        Prefabs = prefabs;
        MoveSpeed = moveSpeed;

        InitializeItems();
    }

    private void InitializeItems()
    {
        Items = new GameObject[Panel.childCount];
        for (int i = 0; i < Panel.childCount; i++)
        {
            Items[i] = Panel.GetChild(i).gameObject;
        }
    }

    public void PlacePanelInFrontOfCamera()
    {
        Vector3 cameraPosition = Camera.main.transform.position;
        Vector3 cameraForward = Camera.main.transform.forward;
        Vector3 cameraRight = Camera.main.transform.right;

        float distanceFromCamera = 1f;
        float rightOffset = 1f;
        Vector3 panelPosition = cameraPosition + cameraForward * distanceFromCamera + cameraRight * rightOffset;

        Vector3 screenPosition = Camera.main.WorldToScreenPoint(panelPosition);
        Panel.position = screenPosition;
    }

    public void UpdateSelection(int index)
    {
        for (int i = 0; i < Items.Length; i++)
        {
            Items[i].GetComponent<Image>().color = (i == index) ? Color.yellow : Color.white;
        }

        if (ScrollRect != null)
        {
            ScrollRect.verticalNormalizedPosition = 1 - (index / (float)(Items.Length - 1));
        }
    }

/*
    public void ConfirmSelection()
    {
        Debug.Log("Selected Item: " + Items[SelectedIndex].name);

        Vector3 forward = BobBody.transform.forward;
        GameObject prefab = Prefabs[SelectedIndex];
        Instantiate(prefab, BobBody.position + forward * 5f, Quaternion.identity);
        ObjSelected = true;
    }
    */

    public GameObject GetSelectedPrefab()
    {
        return Prefabs[SelectedIndex];
    }
}