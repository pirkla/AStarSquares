 using UnityEngine;
 using UnityEngine.UI;
 
 [RequireComponent(typeof(Button))]
  public class UIKey : MonoBehaviour {
    public KeyCode key;

    public Button button;
    private void Start() {
        button = GetComponent<Button>();
    }

    private void Update() {
        if (Input.GetKeyDown(key)) {
            button.onClick.Invoke();
        } 
    }
 }