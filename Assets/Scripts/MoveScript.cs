using UnityEngine;

/// <summary>
/// Simply moves the current game object
/// </summary>
public class MoveScript : MonoBehaviour
{

    public float force;
    private Vector3 randomDirection;
    private Vector3 movement;


    void Start()
    {
        randomDirection = new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f));

        //Drag is how fast object slowing down 
        GetComponent<Rigidbody>().drag = 0;

        //ForceMode.VelocityChange will ignore mass of object and apply velocity instantly
        GetComponent<Rigidbody>().AddForce(randomDirection * force, ForceMode.VelocityChange);
        
    }

    void OnCollisionEnter(Collision other)
    {
        //Check if you have to be specific to an object to bounce
        this.transform.GetComponent<Rigidbody>().velocity = Vector3.Reflect(other.relativeVelocity * -1, other.contacts[0].normal);

    }
}