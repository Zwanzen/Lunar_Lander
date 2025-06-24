using UnityEngine;

public class Rotate : MonoBehaviour
{

    [SerializeField] private float rotationSpeed = 50f;
    [SerializeField] private Axis rotationAxis = Axis.Y;

    private Vector3 rotationVector;

    enum Axis
    {
        X,
        Y,
        Z
    }

    private void Start()
    {
        rotationVector = Vector3.zero;
        switch (rotationAxis)
        {
            case Axis.X:
                rotationVector = Vector3.right;
                break;
            case Axis.Y:
                rotationVector = Vector3.up;
                break;
            case Axis.Z:
                rotationVector = Vector3.forward;
                break;
        }
    }

    private void Update()
    {

        transform.Rotate(rotationVector * rotationSpeed * Time.deltaTime);
    }

}
