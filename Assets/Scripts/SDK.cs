using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TapTap.Bootstrap; // 命名空间
using TapTap.Common; // 命名空间
public class SDK : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {


        var config = new TapConfig.Builder()
            .ClientID("9ctimknkvmigpidsdl") // 必须，开发者中心对应 Client ID
            .ClientToken("ggWk5aKlNJWPAap1yNvchsCuxuSde0aRdOXn6YTb") // 必须，开发者中心对应 Client Token
            .ServerURL("https://your_server_url") // 必须，开发者中心 > 你的游戏 > 游戏服务 > 基本信息 > 域名配置 > API
            .RegionType(RegionType.CN) // 非必须，CN 表示中国大陆，IO 表示其他国家或地区
            .ConfigBuilder();
        TapBootstrap.Init(config);

    }
    public async void taptap()
    {
        var currentUser = await TDSUser.GetCurrent();
        if (null == currentUser)
        {
            Debug.Log("当前未登录");
            // 开始登录
        }
        else
        {
            Debug.Log("已登录");
            // 进入游戏
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
