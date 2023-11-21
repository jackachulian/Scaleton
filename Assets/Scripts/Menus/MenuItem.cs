using UnityEngine;
using UnityEngine.EventSystems;

public class MenuItem : MonoBehaviour, IMoveHandler
{
    private Menu menu;
    // Index in the enclosing menu - set by menu on awake
    private int index = -1;

    private void Awake() {
        menu = GetComponentInParent<Menu>();   
    }

    public void SetIndex(int index) {
        this.index = index;
    }

    public void OnMove(AxisEventData eventData)
    {
        if (index == -1) {
            Debug.LogError(name + " index has not been set by its enclosing menu");
            return;
        }

        if (eventData.moveDir == MoveDirection.Down) {
            menu.ScrollToShowItem(index+1);
        } else if (eventData.moveDir == MoveDirection.Up) {
            menu.ScrollToShowItem(index-1);
        }
    }
}