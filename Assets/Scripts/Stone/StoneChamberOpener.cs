using UnityEngine;
using System.Collections;

public class StoneChamberOpener : MonoBehaviour
{
    [Header("--- 3D Model Parts ---")]
    public GameObject leftPart;
    public GameObject rightPart;
    public GameObject bottomPart;

    [Header("--- Animation Settings ---")]
    public float delayBeforeOpen = 5.0f; 
    public float vibrationIntensity = 0.02f; 
    public float vibrationDuration = 1.0f; 
    
    [Header("--- Movement Distance Control ---")]
    [Tooltip("How far the left model will go")]
    public float leftFlyDistance = 40f; 
    
    [Tooltip("How far the right model will go")]
    public float rightFlyDistance = 40f; 

    [Tooltip("How far back the models will fly while moving left/right")]
    public float backwardFlyDistance = 20f; // 🌟 NEW: Backward movement control
    
    [Tooltip("How far down the bottom model will go")]
    public float bottomFlyDistance = 40f; 

    [Header("--- Movement Speed ---")]
    public float flySpeed = 5f; 

    void Start()
    {
        StartCoroutine(OpenChamberRoutine());
    }

    IEnumerator OpenChamberRoutine()
    {
        yield return new WaitForSeconds(delayBeforeOpen);

        Vector3 leftStart = leftPart != null ? leftPart.transform.position : Vector3.zero;
        Vector3 rightStart = rightPart != null ? rightPart.transform.position : Vector3.zero;
        Vector3 bottomStart = bottomPart != null ? bottomPart.transform.position : Vector3.zero;

        float elapsed = 0f;
        while (elapsed < vibrationDuration)
        {
            if (leftPart != null) leftPart.transform.position = leftStart + new Vector3(Random.Range(-vibrationIntensity, vibrationIntensity), 0, 0);
            if (rightPart != null) rightPart.transform.position = rightStart + new Vector3(Random.Range(-vibrationIntensity, vibrationIntensity), 0, 0);
            if (bottomPart != null) bottomPart.transform.position = bottomStart + new Vector3(0, Random.Range(-vibrationIntensity, vibrationIntensity), 0);
            
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (leftPart != null) leftPart.transform.position = leftStart;
        if (rightPart != null) rightPart.transform.position = rightStart;
        if (bottomPart != null) bottomPart.transform.position = bottomStart;

        // 🌟 FIXED: Now Vector3.forward (backward) is added along with moving left/right
        Vector3 leftTarget = leftStart + (Vector3.left * leftFlyDistance) + (Vector3.forward * backwardFlyDistance);   
        Vector3 rightTarget = rightStart + (Vector3.right * rightFlyDistance) + (Vector3.forward * backwardFlyDistance); 
        Vector3 bottomTarget = bottomStart + (Vector3.down * bottomFlyDistance); // The bottom part will go down just like before

        float moveElapsed = 0f;
        while (moveElapsed < 3f) 
        {
            if (leftPart != null) leftPart.transform.position = Vector3.Lerp(leftPart.transform.position, leftTarget, Time.deltaTime * flySpeed);
            if (rightPart != null) rightPart.transform.position = Vector3.Lerp(rightPart.transform.position, rightTarget, Time.deltaTime * flySpeed);
            if (bottomPart != null) bottomPart.transform.position = Vector3.Lerp(bottomPart.transform.position, bottomTarget, Time.deltaTime * flySpeed);
            
            moveElapsed += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(10f);
        
        if (leftPart != null) Destroy(leftPart);
        if (rightPart != null) Destroy(rightPart);
        if (bottomPart != null) Destroy(bottomPart);
    }
}