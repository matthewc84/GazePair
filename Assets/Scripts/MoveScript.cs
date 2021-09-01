using UnityEngine;

/// <summary>
/// Simply moves the current game object
/// </summary>
public class MoveScript : MonoBehaviour
{

    private Vector3 speed = new Vector3(3, 3, 3);
    private float force = 3f;
    //public Vector3 randomDirection = new Vector3(Random.Range(-1, 2), 0.0f, Random.Range(-1, 2));
    private Vector3 randomDirection;
    private Vector3 movement;


    void Start()
    {
        randomDirection = new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), 0);

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

    /*void Update()
    {
        // 2 - Movement
        movement = new Vector3(
            speed.x * direction.x,
            speed.y * direction.y,
            speed.z * direction.z);
    }

    void FixedUpdate()
    {
        // Apply movement to the rigidbody
        GetComponent<Rigidbody>().velocity = movement;
    }*/
}