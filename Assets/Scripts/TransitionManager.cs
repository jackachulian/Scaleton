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

    // Thing to happen in the middle of the transition while screen is covered
    private Action action;

    private bool inTransition;
    private bool transitioningIn;

    private float alpha;
    private float fadeTime = 0.375f;

    private void Update() {
        if (!inTransition) return;

        if (transitioningIn) {
            if (alpha >= 1f) {
                action.Invoke();
                transitioningIn = false;
            } else {
                alpha = Mathf.MoveTowards(alpha, 1f, Time.deltaTime/fadeTime);
            }
            
        }
        else {
            alpha = Mathf.MoveTowards(alpha, 0f, Time.deltaTime/fadeTime);
            if (alpha <= 0f) {
                gameObject.SetActive(false);
                inTransition = false;
            }
        }

        image.color = new Color(0, 0, 0, alpha);
    }

    public static void Transition(Action action, float fadeTime = 0.375f) {
        Instance.SelfTransition(action, fadeTime);
    }

    private void SelfTransition(Action action, float fadeTime) {
        this.action = action;
        this.fadeTime = fadeTime;
        inTransition = true;
        transitioningIn = true;
        gameObject.SetActive(true);
    }
 }