using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;

public class BossUI : MonoBehaviour {
    

    [SerializeField] private RectTransform healthBackground, healthFill, healthCatchupFill;

    static DamageableEntity boss;

    [SerializeField] private float healthCatchupDelay = 0.6f;

    [SerializeField] private float catchupDampedSpeed = 5f;

    private int displayedHp; 
    private float catchupHp;
    private float catchupTimer;

    private void Start() {
        healthCatchupFill.sizeDelta = new Vector2(healthBackground.sizeDelta.x * (boss.hp / boss.maxHp), healthFill.sizeDelta.y);
    }

    private void Update() {
        if (catchupTimer > 0) {
            catchupTimer -= Time.deltaTime;
            return;
        }

        while (displayedHp != catchupHp) {
            catchupHp = Mathf.MoveTowards(catchupHp, displayedHp, catchupDampedSpeed * Time.deltaTime);
            healthCatchupFill.sizeDelta = new Vector2(healthBackground.sizeDelta.x * (catchupHp / boss.maxHp), healthFill.sizeDelta.y);
        }
    }

    public void SetBoss(DamageableEntity entity) {
        boss = entity;
        HealthBarUpdate();
    }

    public void HealthBarUpdate() {
        displayedHp = boss.hp;
        catchupTimer = healthCatchupDelay;
        float hpPercentage = 1f * boss.hp / boss.maxHp;
        healthFill.sizeDelta = new Vector2(healthBackground.sizeDelta.x * hpPercentage, healthFill.sizeDelta.y);
        Debug.Log(healthBackground.sizeDelta);
        Debug.Log(healthFill.sizeDelta);
    }
}