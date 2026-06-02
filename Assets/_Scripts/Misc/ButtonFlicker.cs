using UnityEngine;
using System.Collections;

public class ButtonFlicker : MonoBehaviour
{
    [SerializeField, Range(0.1f, 2f)] private float flickerInterval;
    [SerializeField] private GameObject buttonImage;
    private bool isFlickering = false;
    
    private void Awake()
    {
        StartCoroutine(FlickerRoutine());
    }

    private IEnumerator FlickerRoutine()
    {
        if(!isFlickering)
        {
            isFlickering = true;

            while (true)
            {
                yield return new WaitForSeconds(flickerInterval);
                buttonImage.SetActive(true);
                yield return new WaitForSeconds(flickerInterval);
                buttonImage.SetActive(false);
            }
        }
        else
        {
            yield break;
        }
    }
}
