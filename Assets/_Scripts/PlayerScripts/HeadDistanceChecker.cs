using System;
using UnityEngine;

public class HeadDistanceChecker : MonoBehaviour
{
    [SerializeField] private Transform headTransform;

    private float currentDistFromHead;

    [SerializeField, Range(0f, 20f)] private float phase1Distance = 7.5f;
    [SerializeField, Range(0f, 20f)] private float phase2Distance = 14f;
    [SerializeField, Range(0f, 20f)] private float phase3Distance = 20f;

    [SerializeField, Range(0f, 50f)] private float maxDistFromHead = 21f;

    void Update()
    {
        if(headTransform == null)
        {
            Debug.LogError("Reference to headTransform is missing");
            return;
        }

        currentDistFromHead = Vector3.Distance(transform.position, headTransform.position);

        switch (currentDistFromHead)
        {
            case float dist when dist < phase1Distance:
                // Phase 1 logic // Standard film grain and vignette
                break;

            case float dist when dist >= phase1Distance && dist < phase2Distance:
                // Phase 2 logic // Lightly increased film grain and vignette
                break;

            case float dist when dist >= phase2Distance && dist < phase3Distance:
                // Phase 3 logic //  Further increased film grain and vignette
                break;

            case float dist when dist >= phase3Distance && dist < maxDistFromHead:
                // Phase 4 logic // Largely increased film grain and vignette
                break;

            case float dist when dist >= maxDistFromHead:
                // Phase 5 logic  // Player lost signal, restart level or something
                break;
        }
    }
}
