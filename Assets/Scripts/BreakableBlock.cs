using UnityEngine;

public class BreakableBlock : Respawnable
{   
    [SerializeField] private float breakForce = 5f;

    [SerializeField] private GameObject breakShatterPrefab;

    [SerializeField] private bool respawns;

    private GameObject breakShatter;

    void OnCollisionEnter2D(Collision2D c){
        if (!c.gameObject.GetComponent<Grabbable>()) return;

        Debug.Log("relvel: "+c.relativeVelocity + " - "+c.relativeVelocity.magnitude);
        Vector2 force = c.relativeVelocity * c.rigidbody.mass;
        Debug.Log("force: "+force+" (magnitude: "+force.magnitude+")");
        float centerForce = Vector2.Dot(force, transform.position - c.transform.position);
        Debug.Log("center force: "+centerForce);

        if(centerForce > breakForce) {
            Break(force);
        }
    }

    void Break(Vector2 force) {
        if (!breakShatterPrefab) {
            gameObject.SetActive(false);
            return;
        }
        
        breakShatter = Instantiate(breakShatterPrefab, transform.position, Quaternion.identity);
        gameObject.SetActive(false);

        Vector2 impulseForce = force.normalized * (force.magnitude - breakForce);
        
        foreach (Rigidbody2D rb in breakShatter.GetComponentsInChildren<Rigidbody2D>()) {
            rb.AddForce(impulseForce * 0.5f * rb.mass, ForceMode2D.Impulse);
            rb.AddForce(Random.insideUnitCircle * 2f * rb.mass, ForceMode2D.Impulse);
            rb.AddTorque(Random.Range(-8f, 8f) * rb.mass);
            Destroy(rb.gameObject, 30f);
        }
    }

    public override void Respawn()
    {
        if (respawns) {
            base.Respawn();
            Destroy(breakShatter);
            gameObject.SetActive(true);
        }
    }
}
