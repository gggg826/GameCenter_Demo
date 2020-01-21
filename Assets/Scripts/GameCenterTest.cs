using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using System.Web;
using System.Collections;

public class GameCenterTest : MonoBehaviour
{
    public Text text;
    bool GameCenterState = false;
    StringBuilder sb = new StringBuilder();
    
    [DllImport("__Internal")]
    private static extern void CallFromUnity_GameCenterUserVerify();

    /// <summary>
    /// 初始化 GameCenter 结果回调函数
    /// </summary>
    /// <param name="success">If set to <c>true</c> success.</param>
    private void HandleAuthenticated(bool success)
    {
        PrintLog("*** HandleAuthenticated: result = " + success);
        ///初始化成功
        if (success)
        {
            GameCenterState = success;
            string userInfo = "Username: " + Social.localUser.userName +
            "\nUser ID: " + Social.localUser.id +
            "\nIsUnderage: " + Social.localUser.underage;

            PrintLog("success" + userInfo);
            
        if (GameCenterState)
        {
            CallFromUnity_GameCenterUserVerify();
        }
        }
        else
        {
            ///初始化失败
            PrintLog("fail");
        }
    }

    public void GameCenterLoginClick()
    {
        Social.localUser.Authenticate(HandleAuthenticated);
    }
    
    public void ClearLogClick()
    {
        if(sb != null)
        {
            sb.Clear();
            text.text = sb.ToString();
        }
    }

    //logincallback
    public void IOSGameGameCenterVerifyFail(string errorMsg)
    {
        
    }

    public void IOSGameGameCenterVerifySuccess(string result)
    {

        PrintLog("gamecenterlogincallback:" + result);
        Dictionary<string, object> dict = (Dictionary<string, object>)Jsontext.ReadeData(result);

        //string url = HttpUtility.UrlEncode("publicKeyUrl");

        //PrintLog($"encode后的url   {url}");

        //System.Text.StringBuilder stringbuilder = Jsontext.WriteData(dict);
        //stringbuilder.Append("\0");

        //PrintLog($"要发送的json  {stringbuilder.ToString()}");



        string sigEncode = HttpUtility.UrlEncode((string)dict["signature"]);
        dict["signature"] = sigEncode;

        System.Text.StringBuilder sb = Jsontext.WriteData(dict);
        sb.Append("\0");

        // TODO 向游戏服务器发送请求登录消息 [2020/1/18 14:40:41 BingLau]
        StartCoroutine(LoginGame(sb.ToString()));

    }


    public IEnumerator LoginGame(string access_token)
    {
        //string temp = $"http://api.tianyuonline.cn/api/extendaccount?sid={access_token}&passwd=wuditianyu&types=1&cv=1.0.1&udid={SystemInfo.deviceUniqueIdentifier}&mc=2000";
        string temp = string.Format("http://api.tianyuonline.cn/api/extendaccount?sid={0}&passwd=wuditianyu&types=1&cv=1.0.1&udid={1}&mc=2000"
                            , access_token, SystemInfo.deviceUniqueIdentifier);

        WWW  nwww = new WWW(temp);
            /******拉选服前*********/

            yield return nwww;

        if (nwww.isDone && string.Empty != nwww.text)
        {
            string text = nwww.text;
            object jsonobj = Jsontext.ReadeData(text);
            if (jsonobj != null)
            {
                Dictionary<string, object> aobj = (Dictionary<string, object>)jsonobj;
                if (aobj != null)
                {
                    Debug.Log("1111111");
                }
                else
                {
                    Debug.Log("22222");
                }
            }
        }
        else
        {
            Debug.Log("333333");
        }
    }


    private void PrintLog(string str)
    {
        Debug.Log(str);
        sb.Append("\n\n" + str);
        text.text = sb.ToString();
    }
}
