using System;
using System.Collections.Generic;
using System.Web;
using System.Collections;
using Newtonsoft.Json;
using System.IO;
using System.Text;

namespace VideoSearch
{
    /// <summary>
    /// Search 的摘要说明
    /// </summary>
    public class Search : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            string key = context.Request["key"].Trim();
            if (key.StartsWith("http") == true)
            {

                string url = key;
                try
                {
                    Function fun = new Function();
                    string code = fun.getCodeByurl(url);
                    cata CATAS = fun.getCatasByCode(code);
                    fun.getAdress(CATAS.catasCode);
                    ArrayList result = fun.result;
                    data [] sub=new data[result.Count];
                    int count = (CATAS.catasCode.Length.ToString().Length - 1);
                    data m=new data();
                    string n="";
                    for (int i = 0; i < result.Count; i++)
                    {
                        m = ((data)result[i]);
                        sub[(m.key-1)] = (data)result[i];
                        n = CATAS.name;
                        if (CATAS.isDSJ) n += " 第" + fun.addZero(sub[(m.key - 1)].key, count) + "集";
                        sub[(m.key-1)].name = n;
                        sub[(m.key - 1)].code = "";
                    }
                    StringBuilder sb = new StringBuilder();
                    JsonSerializer serializer = new JsonSerializer();
                    using (StringWriter sw = new StringWriter(sb))
                    using (JsonWriter writer = new JsonTextWriter(sw))
                    {
                        serializer.Serialize(writer, sub);
                    }
                    context.Response.Write(sb);
                }
                catch(Exception ex)
                {
                    context.Response.Write("error"+ex.Message);
                }
               

            }
            else
            {
                Function fun = new Function();
                string sv = key;
                if (fun.searchMovCode(sv))
                {
                    ArrayList result = fun.searchResults;
                    data[] sub = new data[result.Count];
                    for (int i = 0; i < result.Count; i++)
                    {
                        sub[i] = (data)result[i]; 
                    }
                    StringBuilder sb = new StringBuilder();
                    JsonSerializer serializer = new JsonSerializer();
                    using (StringWriter sw = new StringWriter(sb))
                    using (JsonWriter writer = new JsonTextWriter(sw))
                    {
                        serializer.Serialize(writer, sub);
                    }
                    context.Response.Write(sb);
                }
                else {

                    context.Response.Write("error" + "没有搜索到!");
                }
                
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}