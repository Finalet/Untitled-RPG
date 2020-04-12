using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CandleFlickering : MonoBehaviour
{
    Light lightSource;

    public float flickerSpeed = 1;
    public float flickerIntensity = 1;

    float baseIntensity;
    float newIntensity;
    float timer;

    float changeSpeed;
    void Start()
    {
        lightSource = GetComponent<Light>();
        baseIntensity = lightSource.intensity;
    }

    void Update()
    {
        if (timer >= 0) {
            timer -= Time.deltaTime;
            newIntensity = Random.Range(baseIntensity * (1 - flickerIntensity), baseIntensity * (1+flickerIntensity));
            changeSpeed = Mathf.Abs(newIntensity / lightSource.intensity) * 10;
        } else {
            lightSource.intensity = Mathf.MoveTowards(lightSource.intensity, newIntensity, Time.deltaTime  * changeSpeed);
            if (lightSource.intensity == newIntensity) 
                timer = 1 / flickerSpeed;
        }
    }
}
