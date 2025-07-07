using UnityEngine;
using UnityEngine.UI; 
// Impl Layer
public class ArrowImpl
{
    private ArrowData _data;

    public ArrowImpl(ArrowData data)
    {
        _data = data;
    }

    public void ToggleUI(bool isActive)
    {
        _data.Canvas.gameObject.SetActive(isActive);
        if (isActive)
        {
            _data.PlacePanelInFrontOfCamera();
            _data.UpdateSelection(0); // Select the first item by default
        }
    }

    public void HandleScrollInput(float scrollInput)
    {
        if (scrollInput > 0) // Scrolling up
        {
            _data.SelectedIndex = Mathf.Max(0, _data.SelectedIndex - 1);
        }
        else if (scrollInput < 0) // Scrolling down
        {
            _data.SelectedIndex = Mathf.Min(_data.Items.Length - 1, _data.SelectedIndex + 1);
        }
        _data.UpdateSelection(_data.SelectedIndex);
    }

/*
    public void ConfirmSelection()
    {
        _data.ConfirmSelection();
    }
    */
    public GameObject ConfirmSelection()
    {
        _data.ObjSelected = true;
        return _data.GetSelectedPrefab();
    }


    public void MoveSelectedObject(Vector3 direction)
    {
        if (_data.ObjSelected)
        {
            _data.BobBody.position += direction * _data.MoveSpeed * Time.deltaTime;
        }
    }
}