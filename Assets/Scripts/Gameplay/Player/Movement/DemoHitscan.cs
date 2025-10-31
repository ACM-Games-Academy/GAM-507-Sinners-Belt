using UnityEngine;

public class DemoHitscan : MonoBehaviour
{
    //Hitscan demo script for testing purposes

    [SerializeField] private float range = 100f;
    [SerializeField] private Camera fpsCam;

    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        RaycastHit hit;
        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit, range))
        {
            Debug.Log("Hit: " + hit.transform.name);
            RaycastHit(hit.collider.gameObject.GetComponent<Rigidbody>());
        

            
           
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(fpsCam.transform.position, fpsCam.transform.forward * range);


    }
    public void RaycastHit(Rigidbody rb)
    {
        if (rb != null)
        {
            //Add force opposite to the hit normal
            Vector3 forceDirection = (rb.transform.position - transform.position).normalized;

            float forceMagnitude = 10f; // Adjust this value to change the force applied
            rb.AddForce(forceDirection * forceMagnitude, ForceMode.Impulse);
        }
    }
}
