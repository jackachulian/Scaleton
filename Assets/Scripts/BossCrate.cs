using System.Collections;
using UnityEngine;

public class BossCrate : Grabbable {
    [SerializeField] private float rechargeTime = 3f;

    [SerializeField] private Sprite chargedSprite, rechargingSprite;

    [SerializeField] private GameObject disableWhileRecharging, enableWhileRecharging;

    [SerializeField] private GameObject rechargeRing;


    private Material ringRadialMaterial;


    public bool charged {get; private set;} = true;
    private SpriteRenderer sr;
    private Coroutine rechargeCoroutine;

    protected new void Awake() {
        base.Awake();
        sr = GetComponent<SpriteRenderer>();
        ringRadialMaterial = rechargeRing.GetComponent<SpriteRenderer>().material;
    }

    private void Start() {
        Charge();
    }

    // Called by PresidentBoss after it takes damage from this crate.
    public void DealDamage() {
        sr.sprite = rechargingSprite;
        disableWhileRecharging.SetActive(false);
        enableWhileRecharging.SetActive(true);
        charged = false;

        rechargeCoroutine = StartCoroutine(Recharge());
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
        disableWhileRecharging.SetActive(true);
        enableWhileRecharging.SetActive(false);
        charged = true;
    }
}