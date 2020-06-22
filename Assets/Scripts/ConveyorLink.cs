using UnityEngine;

public class ConveyorLink : MonoBehaviour
{
    public Directions direction;

    public void Initialize()
    {
        Quaternion rotation;
        switch (direction)
        {
            case Directions.North:
                rotation = Quaternion.identity;
                break;
            case Directions.East:
                rotation = Quaternion.AngleAxis(90f, Vector3.up);
                break;
            case Directions.South:
                rotation = Quaternion.AngleAxis(180f, Vector3.up);
                break;
            case Directions.West:
                rotation = Quaternion.AngleAxis(270f, Vector3.up);
                break;
            default:
                rotation = Quaternion.identity;
                break;
        }
        transform.rotation = rotation;
    }

    public void Recycle()
    {
        ObjectPooler.instance.Recycle(this);
    }
}
