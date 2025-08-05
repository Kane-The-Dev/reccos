using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectingEffect : MonoBehaviour
{
    PlayerMovement pm;
    ParticleSystem ps;
    public ParticleSystemForceField psff;

    void Start()
    {
        ps = GetComponent<ParticleSystem>();
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.CompareTag("Player"))
        {
            GetComponent<SphereCollider>().enabled = false;
            pm = collision.GetComponent<PlayerMovement>();
            Transform player = pm.gameObject.transform;
            Vector2 v = new Vector2(player.position.x - transform.position.x, player.position.z - transform.position.z);
            float angle = Vector2.Angle(v,new Vector2(0f,-1f));
            transform.rotation = Quaternion.Euler(-105f,angle,0f);
            ps.Play();
            Invoke("Enable",0.2f);
        }
    }

    public void Enable()
    {
        var externalForces = ps.externalForces;
        externalForces.AddInfluence(pm.psff);
        var main = ps.main;
        main.gravityModifierMultiplier = 0.15f;
    }
}
