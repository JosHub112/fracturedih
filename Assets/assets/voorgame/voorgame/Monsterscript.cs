using UnityEngine;
using System; 
using System.Collections;

public class Monsterscript : MonoBehaviour
{
    public Material _mat; 
    public float timer = 30; 
    private float alphachan;

    void Start()
    {
        StartCoroutine(Lerp());
    }


    void Update()
    {
        
    }

    
IEnumerator Lerp()
    {
        float timeElapsed = 0;

        while (timeElapsed < timer)
        {
            alphachan = Mathf.Lerp(0, 1, timeElapsed / timer);
            _mat.SetColor("_BaseColor", new Color(1,1,1,alphachan));
            timeElapsed += Time.deltaTime;

            yield return null;
        }

        alphachan = 1;
    }


}
