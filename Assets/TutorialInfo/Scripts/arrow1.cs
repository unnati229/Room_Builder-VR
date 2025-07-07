using UnityEngine;
using UnityEngine.UI; 
public class Arrow1 : MonoBehaviour
{
    public GameObject chairPrefab;
    public GameObject sofaPrefab;
    public GameObject tablePrefab;
    public GameObject bedPrefab;
    public GameObject sidetblPrefab;

    public GameObject canvas1;
    public RectTransform panel;
    public ScrollRect scrollRect;

    public FirstPersonController Bob;
    public float moveSpeed = 5f;

    private ArrowUsecase _usecase;
    private ArrowImpl _impl;
    private ArrowData _data;

    void Start()
    {
        if (chairPrefab == null || sofaPrefab == null || tablePrefab == null || bedPrefab == null || sidetblPrefab == null)
    {
        Debug.LogError("One or more prefabs are not assigned!");
        return;
    }

    if (canvas1 == null || panel == null || scrollRect == null)
    {
        Debug.LogError("One or more UI elements are not assigned!");
        return;
    }

    if (Bob == null)
    {
        Bob = FindObjectOfType<FirstPersonController>();
    }

    Rigidbody bobRigidbody = Bob.GetComponent<Rigidbody>();
    if (bobRigidbody == null)
    {
        Debug.LogError("Bob does not have a Rigidbody component!");
        return;
    }
        // Initialize layers
        GameObject[] prefabs = { chairPrefab, sofaPrefab, tablePrefab, bedPrefab, sidetblPrefab };
        _data = new ArrowData(canvas1, panel, scrollRect, Bob.GetComponent<Rigidbody>(), prefabs, moveSpeed);
        _impl = new ArrowImpl(_data);
        _usecase = new ArrowUsecase(_impl);

        // Disable the canvas at the start
        _usecase.ToggleUI(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            _usecase.ToggleUI(!canvas1.activeSelf);
        }

        if (canvas1.activeSelf)
        {
            float scrollInput = Input.GetAxis("Mouse ScrollWheel");
            _usecase.HandleScrollInput(scrollInput);

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                _usecase.ConfirmSelection();
            }
        }

        if (_data.ObjSelected && Input.GetKeyDown(KeyCode.Space))
        {
            _usecase.MoveSelectedObject(Bob.transform.forward);
        }
    }
}