using UnityEngine;

public class GridManager : MonoBehaviour
{
    public GameController gameController;

    public void OnCellClicked(int index)
    {
        gameController.OnCellClick(index);
    }
}
