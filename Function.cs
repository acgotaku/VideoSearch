using System;
using System.Collections.Generic;
using System.Web;
using System.Collections;
using System.Xml;
using System.Threading;
using System.Text.RegularExpressions;
using System.Net;
using System.IO;
using System.Text;

namespace VideoSearch
{
    struct data
    {
        public int key;
        public string code;
        public string name;
        public string infoOrAdress;
    }
    struct cata
    {
        public bool isDSJ;
        public string name;
        public string[] catasCode;
    }
    struct film
    {
        public string name;
        public string director;
        public string actor;
        public string region;
        public string filmtype;
        public string publishTime;
        public string adddate;
        public string brief;
    }

    class Function
    {
        private   string IP = @"http://210.45.193.47/";

        private string searchinfopath = HttpContext.Current.Server.MapPath(".");
        public   ArrayList result = new ArrayList();

        //获得标志码
        public   string getCodeByurl(string url)
        {
            string[] datas = url.Split('?');
            string[] parm = datas[1].Split('&');
            string code = parm[0].Substring(5);
            return code;
        }
        //获得所有剧集的标志码
        public   cata getCatasByCode(string code)
        {
            cata c = new cata();
            string url = IP + @"mov/" + code + @"/url.xml";
            string xml = getXMLbyUrl(url);
            XmlDocument XML = new XmlDocument();
            XML.LoadXml(xml);
            string playurl = XML.SelectSingleNode("root/b").InnerText;
            c.name = XML.SelectSingleNode("root/a").InnerText;
            playurl = playurl.Replace("\n", "");
            playurl = playurl.Replace("\r", "");
            c.catasCode = playurl.Split(',');
            if (c.catasCode.Length > 2) c.isDSJ = true;
            return c;
        }
        //多线程解析下载地址
        public   void getAdress(string [] catas)
        {
            result.Clear();
            for (int i = 0; i < catas.Length - 1; i++)
            {
                data m = new data();
                m.key = i + 1;
                m.code = catas[i];
                ThreadPool.QueueUserWorkItem(new WaitCallback(getAdressbyCode), m);
            }
            waitAllStop(catas.Length-1);
        }
        //等待所有线程结束
        private   void waitAllStop(int count)
        {
            while (true)
            {
                if (result.Count == count) break;
                Thread.Sleep(50);
            }
        }
        //根据标志码获得下载地址
        public   void getAdressbyCode(object o)
        {
            data datas = (data)o;
            string adress = "";
            string url = IP + @"return.asp?info=" + datas.code;
            string xml = getXMLbyUrl(url);
            if (xml != "")
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xml);
                adress = xmlDoc.SelectSingleNode("root/url").InnerText;
                adress = Regex.Replace(adress, "http://(.*?)/", IP);
                if (adress == "") adress = "服务器没有本视频数据";
                datas.infoOrAdress = adress;
            }
            else
            {
                //MessageBox.Show(datas.code);
            }
            result.Add(datas);
        }

        //获得网络XML文件
        public   string getXMLbyUrl(string url)
        {
            string xml = "";
            try
            {
                HttpWebRequest httpReq = (HttpWebRequest)WebRequest.Create(url);
                httpReq.UserAgent = @"Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; .NET CLR 2.0.40607; .NET CLR 1.1.4322)";
                StreamReader sr = null;
                HttpWebResponse httpResponse = null;
                httpResponse = (HttpWebResponse)httpReq.GetResponse();
                sr = new StreamReader(httpResponse.GetResponseStream(), Encoding.GetEncoding("GB2312"));
                xml = sr.ReadToEnd();//返回的结果                
            }
            catch
            {
                xml = "";
            }
            return xml;
        }
       //解析文件详细信息
        public film getFilmbyUrl(string url)
        {
            string xml = getXMLbyUrl(url);
            film movie = new film();
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);
            if (xmlDoc == null) return movie;
            XmlNodeList nodelist = xmlDoc.SelectNodes("film");
            movie.name = nodelist[0].SelectSingleNode("name").InnerText;
            movie.director = nodelist[0].SelectSingleNode("director").InnerText;
            movie.actor = nodelist[0].SelectSingleNode("actor").InnerText;
            movie.region = nodelist[0].SelectSingleNode("region").InnerText;
            movie.filmtype = nodelist[0].SelectSingleNode("filmtype").InnerText;
            movie.publishTime = nodelist[0].SelectSingleNode("publishTime").InnerText;
            movie.adddate = nodelist[0].SelectSingleNode("adddate").InnerText;
            movie.brief = nodelist[0].SelectSingleNode("brief").InnerText.Remove(0, 14) ;
                return movie;
        }
        //搜索视频
        public   ArrayList searchResults = new ArrayList();
        public   bool searchMovCode(string key)
        {
            XmlDocument XML = openSearchInfo();
            if (XML == null) return false;
            XmlNodeList nodelist = XML.SelectNodes("root/film");
            for (int i = 0; i < nodelist.Count; i++)
            {
                if (nodelist[i].SelectSingleNode("a").InnerText.IndexOf(key) != -1 ||
                    nodelist[i].SelectSingleNode("c").InnerText.IndexOf(key) != -1 ||
                    nodelist[i].SelectSingleNode("d").InnerText.IndexOf(key) != -1 ||
                    nodelist[i].SelectSingleNode("g").InnerText.IndexOf(key) != -1)
                 {
                    data c = new data();
                    c.name = nodelist[i].SelectSingleNode("a").InnerText;
                    c.code = nodelist[i].SelectSingleNode("b").InnerText;
                    c.infoOrAdress = nodelist[i].SelectSingleNode("s").InnerText;
                    searchResults.Add(c);
                }
            }
            return true;
        }
        //获得搜索信息并保存到本地
        private   string getSearchInfoandSave()
        {
            string datas = getXMLbyUrl(@"http://210.45.193.47/mov/xml/Total.xml");
            datas = datas.Replace(@"</root>", "<updatetime>" + DateTime.Now.ToString("yyyy-MM-dd") + @"</updatetime></root>");
            StreamWriter sw;
            string f = searchinfopath + "serarchinfo.ahnu";
            if (File.Exists(f))
            {
                File.Delete(f);
            }
            sw = File.CreateText(f);
            sw.Write(datas);
            sw.Close();
            File.SetAttributes(f, FileAttributes.Hidden);
            return datas;
        }
        //打开搜索信息
        private   XmlDocument openSearchInfo()
        {
            string f = "";
            try
            {
                f = File.ReadAllText(searchinfopath + "serarchinfo.ahnu", Encoding.GetEncoding("UTF-8"));
            }
            catch
            {
                f=getSearchInfoandSave();
            }
            XmlDocument XML = new XmlDocument();
            if (f != "")
            {
                XML.LoadXml(f);
                DateTime today = DateTime.Now;
                DateTime date = DateTime.Parse(XML.SelectSingleNode("root/updatetime").InnerText);
                TimeSpan ts = today.Subtract(date);
                if (double.Parse(ts.TotalDays.ToString()) > 1.0)
                {
                    f = getSearchInfoandSave();
                    if (f != "") XML.LoadXml(f);
                    else XML = null;
                }
            }
            else
            {
                XML = null;
            }
            
            return XML;
        }
        //对数字格式化
        public    string addZero(int num,int length)
        {

            if (num >= Math.Pow(10, length))
            {
                return num.ToString();
            }
            else
            {
                return num.ToString().PadLeft(length + 1, '0');
            }
        }  
        //访问用户统计
        public   bool userCount()
        {
            try
            {
                System.Net.WebClient wb = new System.Net.WebClient();
                wb.DownloadData(@"http://www.icehoney.me/count.php?method=add");                
                return true;
            }
            catch
                {
                }
            return false;
        }
        //第一次运行程序
        public   void isFirstRun()
        {
            string f=searchinfopath + "AHNU";
            if (!File.Exists(f))
            {
                if (userCount())
                {
                    StreamWriter sw;
                    sw = File.CreateText(searchinfopath + "AHNU");
                    sw.Close();
                    File.SetAttributes(f, FileAttributes.ReadOnly);
                    File.SetAttributes(f, FileAttributes.Hidden);
                }                 
            }
        }
        //获得最新版本号
        private   string getVersion()
        {
            return getXMLbyUrl(@"http://www.icehoney.me/version.txt");
        }
       
        
     
        //检查网络连接
        public   bool isLink(string url)
        {
            try
            {
                HttpWebRequest httpReq = (HttpWebRequest)WebRequest.Create(url);
                httpReq.UserAgent = @"Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; .NET CLR 2.0.40607; .NET CLR 1.1.4322)";
                httpReq.Timeout = 1000;
                HttpWebResponse httpResponse = null;
                httpResponse = (HttpWebResponse)httpReq.GetResponse();
                if (httpResponse.StatusCode == HttpStatusCode.OK)
                {
                    return true;
                }
            }
            catch
            {                
            }
            return false;
        }

        //检查校园网
        public   bool LANlink()
        {
            return isLink(@"http://cse.ahnu.edu.cn/");
        }
        //检查外网
        public   bool NETlink()
        {
            return isLink(@"http://www.icehoney.me/");
        }
    }
}
