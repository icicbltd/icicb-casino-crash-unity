using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HistoryHandler : MonoBehaviour
{
    public Image img;
    public Text mul;
    public Sprite[] sps;

    // Start is called before the first frame update
    void Start()
    {
        img.enabled = false;
        mul.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void SetVal(float val)
    {
        img.enabled = true;
        mul.enabled = true;
        if (val > 2f)
        {
            img.sprite = sps[2];
            mul.text = val.ToString() + "x";
        }

        if (val < 2f && val > 1.2f)
        {
            img.sprite = sps[1];
            mul.text = val.ToString() + "x";
        }

        if (val < 1.2f)
        {
            img.sprite = sps[0];
            mul.text = val.ToString() + "x";
        }

        if (val < 0.5f)
        {
            img.enabled = false;
            mul.enabled = false;
        }
    }


}
