// Usecase Layer
using UnityEngine;
using UnityEngine.UI; 
public class ArrowUsecase
{
    private ArrowImpl _impl;

    public ArrowUsecase(ArrowImpl impl)
    {
        _impl = impl;
    }

    public void ToggleUI(bool isActive)
    {
        _impl.ToggleUI(isActive);
    }

    public void HandleScrollInput(float scrollInput)
    {
        _impl.HandleScrollInput(scrollInput);
    }

    public void ConfirmSelection()
    {
        _impl.ConfirmSelection();
    }

    public void MoveSelectedObject(Vector3 direction)
    {
        _impl.MoveSelectedObject(direction);
    }
}