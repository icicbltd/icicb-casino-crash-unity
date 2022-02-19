using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class UserInfoHandler : MonoBehaviour
{
    public TMP_Text txt_username;
    public TMP_Text txt_multipier;
    public TMP_Text txt_amount;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetValues(string username, string multipier, string amount, string betted, string cashed)
    {
        txt_username.text = username;

        bool b_betted = bool.Parse(betted);
        bool b_cashed = bool.Parse(cashed);

        if (!b_betted && !b_cashed)
        {
            txt_multipier.text = "";
            txt_amount.text = (float.Parse(amount)).ToString("F1");
            txt_amount.color = Color.green;
            Debug.Log("called AAA");
        }

        if (b_betted && !b_cashed)
        {
            txt_multipier.text = "";
            txt_amount.text = (float.Parse(amount)).ToString("F1");
            txt_amount.color = Color.green;
            Debug.Log("called BBB");
        }


        if (b_betted && b_cashed)
        {
            txt_multipier.text = float.Parse(multipier).ToString("0.00") + "x";
            txt_multipier.color = Color.green;
            txt_amount.text = (float.Parse(amount)).ToString("F1");
            txt_amount.color = Color.green;
            Debug.Log("called CCC");
        }

    }

}
