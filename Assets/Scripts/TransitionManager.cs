using System;
using UnityEngine;
using UnityEngine.UI;

public class TransitionManager : MonoBehaviour {

    private static TransitionManager Instance;

    private void Awake() {
        if (Instance != null) Destroy(gameObject);
        Instance = this;
    }

    [SerializeField] private Image image;

    private Color fadeColor = Color.black;

    // Thing to happen in the middle of the transition while screen is covered
    private Action action;

    private bool inTransition;
    private bool transitioningIn;

    private float alpha;
    private float fadeInTime = 0.375f;
    private float fadeOutTime = 0.375f;

    private string fadeType = "linear";

    private void Update() {
        if (!inTransition) return;

        if (transitioningIn) {
            if (alpha >= 1f) {
                action.Invoke();
                transitioningIn = false;
            } else {
                alpha = Mathf.MoveTowards(alpha, 1f, Time.deltaTime/fadeInTime);
            }
            
        }
        else {
            alpha = Mathf.MoveTowards(alpha, 0f, Time.deltaTime/fadeOutTime);
            if (alpha <= 0f) {
                gameObject.SetActive(false);
                inTransition = false;
            }
        }

        float displayedAlpha = (fadeType == "easeIn") ? alpha*alpha : alpha;
        image.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, displayedAlpha);
    }

    public static void Transition(Action action, Color fadeColor, float fadeInTime = 0.375f, float fadeOutTime = 0.375f, string fadeType = "linear") {
        Instance.SelfTransition(action, fadeColor, fadeInTime, fadeOutTime, fadeType);
    }

    private void SelfTransition(Action action, Color fadeColor, float fadeInTime, float fadeOutTime, string fadeType) {
        this.action = action;
        this.fadeColor = fadeColor;
        this.fadeInTime = fadeInTime;
        this.fadeOutTime = fadeOutTime;
        this.fadeType = fadeType;
        inTransition = true;
        transitioningIn = true;
        gameObject.SetActive(true);
    }
 }