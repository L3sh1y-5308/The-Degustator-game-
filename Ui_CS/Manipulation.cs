using UnityEngine;
using UnityEngine.InputSystem;

public class Manipulation : MonoBehaviour
{
    
    [SerializeField] GameObject Camera;
    [SerializeField] GameObject ObjectToManipulate;


    public bool ManipulationEnabled = false;
    public bool ChosingMode = false;
    private GameInput controls;

    void Awake()
    {
        controls = new GameInput();
    }

void OnEnable()
{
    ChosingMode = !ManipulationEnabled;
    if (ManipulationEnabled)
        controls.ControllOf3dObj.Enable();
    else
        controls.ControllOf3dObj.Disable();
}


    public float speed = 5f;
    public void ManipulationEnable()
    {
        Vector2 scrollVector = controls.ControllOf3dObj.Scroll.ReadValue<Vector2>();
        if (scrollVector.y != 0) 
        { 
            float direction = Mathf.Sign(scrollVector.y);

            transform.Translate(Vector3.forward * direction * speed * Time.deltaTime);

        }


    }

    void Update()
    {
      ManipulationEnable();

    }



}
