using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NotificationController : MonoBehaviour
{

    public Text txt_notification;
    private static NotificationController _instance;
    public bool isShowing = false;
    public static NotificationController Instance {
        get {
            return _instance;
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Image>().enabled = false;
        txt_notification.enabled = false;
        _instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void Show(string msg)
    {
        if (isShowing)
            return;

        isShowing = true;
        txt_notification.enabled = true;
        GetComponent<Image>().enabled = true;
        txt_notification.text = msg;
        StartCoroutine(iShow(2f));
    }


    IEnumerator iShow(float t)
    {
        

        yield return new WaitForSeconds(t);
        GetComponent<Image>().enabled = false;
        txt_notification.enabled = false;
        isShowing = false;

    }
}
