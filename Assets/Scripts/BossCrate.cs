using System.Collections;
using Cinemachine;
using UnityEngine;

public class BossCrate : Grabbable {
    [SerializeField] private float rechargeTime = 3f;

    [SerializeField] private Sprite chargedSprite, rechargingSprite;

    [SerializeField] private GameObject enableWhileRecharging, enableWhileCharged;

    [SerializeField] private ParticleSystem chargedParticles, boostParticles;

    [SerializeField] private GameObject rechargeRing;

    [SerializeField] private float maxBoostTime = 0.75f;
    [SerializeField] private GameObject boostHitEffect;
    [SerializeField] private CinemachineImpulseSource boostHitImpulseSource;

    private Material ringRadialMaterial;


    public bool charged {get; private set;}
    public bool boosting {get; private set;}
    private SpriteRenderer sr;
    private Coroutine rechargeCoroutine;

    protected new void Awake() {
        base.Awake();
        sr = GetComponent<SpriteRenderer>();
        ringRadialMaterial = rechargeRing.GetComponent<SpriteRenderer>().material;
    }

    private void Start() {
        boostParticles.Stop();
        Charge();
    }

    // Called by PresidentBoss after it takes damage from this crate.
    public void Uncharge() {
        charged = false;
        sr.sprite = rechargingSprite;
        enableWhileCharged.SetActive(false);
        enableWhileRecharging.SetActive(true);
    }

    IEnumerator Recharge() {
        float time = 0;
        while (time < rechargeTime) {
            rechargeRing.transform.rotation = Quaternion.identity; // keep upright so fill end is always at top
            ringRadialMaterial.SetFloat("_FillAmount", time/rechargeTime);
            time += Time.deltaTime;
            yield return null;
        }

        Charge();
    }

    // Instantly charge this crate without cooldown.
    public void Charge() {
        if (rechargeCoroutine != null) StopCoroutine(rechargeCoroutine);

        sr.sprite = chargedSprite;
        SoundManager.PlaySound(audioSource, "boxcharged");
        enableWhileCharged.SetActive(true);
        enableWhileRecharging.SetActive(false);
        chargedParticles.Play();
        charged = true;
    }

    // Called by BossCrate
    public void Boost() {
        Physics2D.IgnoreCollision(MenuManager.player.capsuleCollider, GetComponent<Collider2D>(), true);
        StartCoroutine(ReenableCollisionAFterShortDelay());
        chargedParticles.Stop();
        boostParticles.Play();
        boosting = true;
        StartCoroutine(UnboostAfterDelay());
    }

    IEnumerator ReenableCollisionAFterShortDelay() {
        yield return new WaitForSeconds(0.2f);
        Physics2D.IgnoreCollision(MenuManager.player.capsuleCollider, GetComponent<Collider2D>(), false);
    }

    // Called when colliding with something while boosted. Removes boost particles and begins charge
    public void Unboost() {
        Uncharge();
        boostParticles.Stop();
        boosting = false;
        
        rechargeCoroutine = StartCoroutine(Recharge());
    }

    // This will clear boost if the crate flies in the air too long while boosting.
    IEnumerator UnboostAfterDelay() {
        yield return new WaitForSeconds(maxBoostTime);
        if (boosting) Unboost();
    }

    protected override void OnCollisionEnter2D(Collision2D other) {
        base.OnCollisionEnter2D(other);
        if (boosting) {
            Instantiate(boostHitEffect, transform.position, transform.rotation);
            boostHitImpulseSource.GenerateImpulseAt(transform.position, (other.relativeVelocity + Vector2.up*0.5f).normalized * 0.05f);
            Unboost();
        }
    }
}