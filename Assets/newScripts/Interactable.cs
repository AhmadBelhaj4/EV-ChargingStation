// Any object the player can interact with uses this interface
public interface IInteractable
{
    string GetHintText(); // What to show in the UI
    void Interact();      // What happens when E is pressed
}