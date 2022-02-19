
using SimpleJSON;
using UnityEngine;
using UnitySocketIO;
using UnitySocketIO.Events;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TMPro;

public class CrashController : MonoBehaviour
{

    public SocketIOController io;
    public TMP_Text txt_multipier;
    public Text txt_bettimetitle;
    public Text txt_bettimeprogess;
    public GameObject obj_bettimeprogress;
    public Image img_bettimeprogess;
    public Sprite sp_bg_green;
    public Sprite sp_bg_red;
    public Text txt_betbutton;
    public GameObject obj_rotate;
    public Image img_betplay;
    public Image img_check;
    public Button btn_check;
    public Button btn_plus;
    public Button btn_minus;
    public Text txt_autocash;
    public Text txt_atribalance;
    public InputField input_amount;
    public GameObject prefab_userinfo;
    public Transform transform_content;
    public GameObject btn_reconnect;
    public Image img_network;
    public GameObject[] historys;
    public GameObject prefab_tooltip;
    public GameObject tooltip_parent;

    const int BET = 0;
    const int READY = 1;
    const int PLAYING = 2;
    const int GAMEEND = 3;
    public float f_autocash = 1.01f;
    public bool b_isAutoCash = false;
    public bool isEnteredRoom = false;
    BetPlayer _player = new BetPlayer();
    public float f_multipier = 0f;
    public enum GameState { 
        BET,
        READY,
        PLAYING,
        GAMEEND
    }

    public GameState myState = GameState.BET;
    public bool isCashingout = false;
    public bool isBetting = false;
    public bool isCrashed = false;
    public Users m_users;

    // GameReadyStatus Send
    [DllImport("__Internal")]
    private static extern void GameReady (string msg);

    // Start is called before the first frame update
    void Start()
    {
        obj_rotate.SetActive(false);
        input_amount.text = "10.0";
        img_check.enabled = b_isAutoCash;
        btn_minus.interactable = b_isAutoCash;
        btn_plus.interactable = b_isAutoCash;
        txt_autocash.text = f_autocash.ToString();

        io.Connect();
        
        #if UNITY_WEBGL == true && UNITY_EDITOR == false
            GameReady("Ready");
        #endif

        StartCoroutine(iStart(0.5f));
    }

    public void setNewSocket (string data) {
        JSONNode socketInfo = JSON.Parse(data);
        // io.Close();
        Debug.Log(socketInfo["IP"]);
        Debug.Log(socketInfo["port"]);
        Debug.Log(socketInfo["sslEnabled"]);
        io.SetSocketURL(socketInfo["IP"], int.Parse(socketInfo["port"]) ,socketInfo["sslEnabled"]);
        // io.SetSocketURL(newURL, port, sslEnabled);
        io.Connect();

        StartCoroutine(iStart(0.5f));
    }

    public void RequestToken (string data) {
        JSONNode usersInfo = JSON.Parse(data);
        Debug.Log(usersInfo["token"]);
        Debug.Log(usersInfo["amount"]);
        _player.token = usersInfo["token"];
        _player.name = usersInfo["userName"];

        float i_balance = int.Parse(usersInfo["amount"]);
        txt_atribalance.text = i_balance.ToString();
    }

   
    IEnumerator iStart(float t)
    {
        yield return new WaitForSeconds(t);
        
        io.Emit("enterroom", JsonUtility.ToJson(_player), (string res) =>
        {
       
            CallResponse cres = JsonUtility.FromJson<CallResponse>(res);
            if (cres.status == 1)
            {
                isEnteredRoom = true;
            }
            else
            {
                isEnteredRoom = false;
            }
        });

        io.On("crash", (SocketIOEvent e) => {

            if (!isEnteredRoom)
                return;

            Response res = JsonUtility.FromJson<Response>(e.data);
            switch (res.gamestate)
            {
                case BET:
                    isCrashed = false;
                    SpaceCruizerController.Instance.SetStarted(false);
                    txt_bettimetitle.enabled = true;
                    txt_bettimeprogess.enabled = true;
                    txt_bettimetitle.text = "Place your bet!";
                    txt_multipier.text = "1.00x";
                    txt_multipier.color = Color.white;
                    txt_betbutton.text = "BET";
                    obj_bettimeprogress.SetActive(true);
                    img_bettimeprogess.enabled = true;
                    img_bettimeprogess.fillAmount = 1f - res.progress / 10000f;
                    txt_bettimeprogess.text = (10 - (res.progress / 10000f) *10 ) .ToString("0.00");
                    img_betplay.sprite = sp_bg_red;                    
                    obj_rotate.SetActive(false);
                    img_betplay.GetComponent<Button>().interactable = true;
                    myState = GameState.BET;
                    _player.crashed = false;
                    break;
                case READY:
                    SpaceCruizerController.Instance.SetStarted(false);
                    txt_bettimeprogess.enabled = false;
                    txt_bettimetitle.enabled = true;
                    txt_bettimetitle.text = "Ready! Stop Betting.";
                    txt_betbutton.text = "BET";
                    img_bettimeprogess.enabled = false;
                    obj_bettimeprogress.SetActive(false);
                    obj_rotate.SetActive(true);
                    txt_betbutton.enabled = false;
                    img_betplay.sprite = sp_bg_red;
                    img_betplay.GetComponent<Button>().interactable = false;
                    myState = GameState.READY;
                    break;
                case PLAYING:
                    SpaceCruizerController.Instance.SetStarted(true);
                    txt_multipier.text = res.mul.ToString("0.00") + "x";
                    f_multipier = float.Parse(res.mul.ToString("0.00"));
                    txt_bettimeprogess.enabled = false;
                    txt_bettimetitle.enabled = false;
                    txt_betbutton.text = "CASHOUT";
                    txt_betbutton.enabled = true;
                    img_bettimeprogess.enabled = false;
                    obj_bettimeprogress.SetActive(false);
                    obj_rotate.SetActive(false);
                    img_betplay.sprite = sp_bg_green;
                    
                    if (b_isAutoCash)
                    {
                        img_betplay.GetComponent<Button>().interactable = false;
                        if (f_multipier == f_autocash)
                        {
                            StartCoroutine(iProcessAutoCashout());
                        }
                    }
                    else {
                        img_betplay.GetComponent<Button>().interactable = true;
                    }

                    myState = GameState.PLAYING;
                    break;
                case GAMEEND:
                    txt_multipier.color = Color.red;
                    txt_bettimetitle.enabled = false;
                    txt_bettimeprogess.enabled = false;
                    img_bettimeprogess.enabled = false;
                    obj_bettimeprogress.SetActive(false);
                    obj_rotate.SetActive(false);
                    img_betplay.sprite = sp_bg_red;
                    txt_betbutton.enabled = true;
                    txt_betbutton.text = "BET END";
                    img_betplay.sprite = sp_bg_red;
                    img_betplay.GetComponent<Button>().interactable = false;
                    myState = GameState.GAMEEND;
                    if (!isCrashed)
                    {
                        isCrashed = true;
                        SpaceCruizerController.Instance.Crash();
                    }

                    
                    if(!_player.crashed)
                    { 
                        if (_player.betted && !_player.cashouted)
                        {
                            _player.crashed = true;
                            f_multipier = float.Parse(res.mul.ToString("0.00"));
                            _player.multipier = f_multipier;

                            io.Emit("crashed", JsonUtility.ToJson(_player), (string sres) => {
                                CallResponse cres = JsonUtility.FromJson<CallResponse>(sres);
                                if (cres.status == 1)
                                {
                                    NotificationController.Instance.Show(cres.message);                                 
                                }
                            });
                       
                        }
                    }


                    _player.betted = false;
                    _player.cashouted = false;
                    break;
            }
            
        });

        io.On("userslist", (SocketIOEvent e) => {
         
            m_users = Users.CreateFromJson(e.data);

            JSONNode usersInfo = JSON.Parse(e.data);

            Debug.Log(usersInfo["users"]);

            if (transform_content.childCount != 0)
            {
                for (int i = 0; i < transform_content.childCount; i++)
                {
                    Destroy(transform_content.GetChild(i).gameObject);
                }
            }

            for (int i = 0; i < usersInfo["users"].Count; i++)
            {
                var cell = Instantiate(prefab_userinfo, transform_content);
                cell.GetComponent<UserInfoHandler>().SetValues(usersInfo["users"][i]["name"], usersInfo["users"][i]["multipier"], usersInfo["users"][i]["amount"], usersInfo["users"][i]["betted"], usersInfo["users"][i]["cashouted"]);             
            }

        });


        io.On("history", (SocketIOEvent e) => {
            Debug.Log(e.data);
            JSONNode history = JSON.Parse(e.data);
           
            for (int i = 0; i < history["history"].Count; i++)
            {

                historys[i].GetComponent<HistoryHandler>().SetVal(float.Parse(history["history"][i]));
            }


        });

        io.On("betted", (SocketIOEvent e) => {
            JSONNode betted = JSON.Parse(e.data);
            GameObject tooltip = Instantiate(prefab_tooltip, tooltip_parent.transform);
            tooltip.GetComponent<TooltipHandler>().SetVal(betted["username"] + " : " + float.Parse(betted["mul"]).ToString("0.00"));
            Debug.Log(betted);
        });

        io.On("error message", (SocketIOEvent e) => {
            CallResponse cres = JsonUtility.FromJson<CallResponse>(e.data);
            Debug.Log("-------------------" + cres.errMessage);
            NotificationController.Instance.Show(cres.errMessage);
        });

        io.On("bet response", (SocketIOEvent e) => {
            CallResponse cres = JsonUtility.FromJson<CallResponse>(e.data);
            NotificationController.Instance.Show(cres.message);
            isBetting = false;
            txt_atribalance.text = (float.Parse(string.IsNullOrEmpty(txt_atribalance.text) ? "0" : txt_atribalance.text) - cres.data).ToString("F2");
        });

        io.On("cashout result", (SocketIOEvent e) => {
            CallResponse cres = JsonUtility.FromJson<CallResponse>(e.data);
            NotificationController.Instance.Show(cres.message);
            txt_atribalance.text = (float.Parse(string.IsNullOrEmpty(txt_atribalance.text) ? "0" : txt_atribalance.text) + cres.earnAmount).ToString("F2");
            isCashingout = false;
        });
    }

    
    public void OnBtnClick_PlayerBet()
    {
        switch (myState)
        {
            case GameState.BET:
                if (isBetting)
                    return;

                if(_player.betted) {
                    NotificationController.Instance.Show("You already bet");
                    return;
                }
                if(float.Parse(string.IsNullOrEmpty(input_amount.text) ? "0" : input_amount.text) > float.Parse(string.IsNullOrEmpty(txt_atribalance.text) ? "0" : txt_atribalance.text)){   
                    NotificationController.Instance.Show("Your Balance is not enough");
                    return;
                }
                _player.amount = float.Parse(string.IsNullOrEmpty(input_amount.text) ? "0" : input_amount.text);
                _player.betted = true;
                isBetting = true;

                io.Emit("playerbet", JsonUtility.ToJson(_player));

                break;

            case GameState.READY:

                break;

            case GameState.PLAYING:
                if (!_player.betted)
                {
                    NotificationController.Instance.Show("You did not bet yet!");
                    return;

                }
                if (_player.cashouted)
                {
                    NotificationController.Instance.Show("You already cashouted!");
                    return;
                }
                if (isCashingout)
                {
                    NotificationController.Instance.Show("You are  cashing out!");
                    return;
                }
                isCashingout = true;
                _player.multipier = f_multipier;
                _player.cashouted = true;

                io.Emit("playercashout", JsonUtility.ToJson(_player));            
     
                break;

            case GameState.GAMEEND:
                break;
        }
 
    }

    public void OnBtnClick_MinAmount()
    {
        input_amount.text = "10.0";
    }

    public void OnChangeAmount(){
        if (float.Parse(input_amount.text) < 10f)
            input_amount.text = "10.0";
        else if (float.Parse(input_amount.text) > 100000f)
        {
            input_amount.text = "100000.0";
        }
    }

    public void OnBtnClick_MaxAmount()
    {
        float myTotalAmount = float.Parse(string.IsNullOrEmpty(txt_atribalance.text) ? "0" : txt_atribalance.text);
        if (myTotalAmount >= 100000f)
            input_amount.text = "100000.0";
        else if (myTotalAmount >= 10f && myTotalAmount < 100000f)
            input_amount.text = myTotalAmount.ToString("F2");
    }

    public void OnBtnClick_HalfAmount()
    {
        float amount = float.Parse(input_amount.text);
        if (amount <= 10f)
            input_amount.text = "10.0";
        else
            input_amount.text = (amount / 2.0f).ToString("F2");
    }

    public void OnBtnClick_DoubleAmount()
    {
        float amount = float.Parse(input_amount.text);
        if (amount >= 100000f)
            input_amount.text = "100000.0";
        else
        {
            float temp = amount * 2.0f;
            if (temp >= 100000f)
                input_amount.text = "100000.0";
            else
                input_amount.text = (amount * 2.0f).ToString("F2");
        }
            
    }

    public void OnBtnClick_Checkbox()
    {
        b_isAutoCash = !b_isAutoCash;
        img_check.enabled = b_isAutoCash;
        btn_minus.interactable = b_isAutoCash;
        btn_plus.interactable = b_isAutoCash;
    }

    IEnumerator iProcessAutoCashout()
    {
        yield return null;
        if (b_isAutoCash)
        {
            if (_player.betted)
            {

                _player.multipier = f_multipier;
                _player.cashouted = true;
                io.Emit("playercashout", JsonUtility.ToJson(_player), (string res) =>
                {
                    CallResponse cres = JsonUtility.FromJson<CallResponse>(res);
                    NotificationController.Instance.Show(cres.message);
                    txt_atribalance.text = (float.Parse(string.IsNullOrEmpty(txt_atribalance.text) ? "0" : txt_atribalance.text) + cres.earnAmount).ToString("F2");
                    isCashingout = false;
                });
            }
            else {
                NotificationController.Instance.Show("You did not bet yet.");
            }
        }

    }


    public void OnBtnClick_Plus()
    {
        f_autocash = Mathf.FloorToInt(f_autocash) + 1f;
        txt_autocash.text = f_autocash.ToString();
    }

    public void OnBtnClick_Minus()
    {
        f_autocash = Mathf.FloorToInt(f_autocash) - 1f;
        if (f_autocash < 1.5f)
            f_autocash = 1.01f;
        txt_autocash.text = f_autocash.ToString();
    }
    // Update is called once per frame
    void Update()
    {
        img_network.color = Application.internetReachability == NetworkReachability.NotReachable ? Color.red : Color.green;
        StartCoroutine(UpdateState(0.5f));
    }

    IEnumerator UpdateState(float t)
    {
        io.Emit("state update", JsonUtility.ToJson(_player));
        yield return new WaitForSeconds(t);
    }

    private void OnApplicationQuit()
    {
        io.Emit("disconnect");
        io.Close();        
    }

}


[Serializable]
public class Users
{
    public List<BetPlayer> users;
    public static Users CreateFromJson(string data)
    {
        return JsonUtility.FromJson<Users>(data);
    }
}


public class Response {
    public int gamestate;
    public float progress;
    public float mul;
}


public class BetPlayer {
    public string name;
    public float amount;
    public bool betted;
    public float multipier;
    public bool cashouted;
    public bool crashed;
    public string token;
}

public class CallResponse {
    public int status;
    public string message;
    public int data;
    public float earnAmount;
    public string errMessage;
}
