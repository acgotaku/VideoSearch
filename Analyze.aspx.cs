using System;
using System.Collections.Generic;

using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using System.Collections;

namespace VideoSearch
{
    public partial class Analyze1 : System.Web.UI.Page
    {
        private data[] sub = null;
        private string IP = @"http://210.45.193.47/";
        private film movie = new film();
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request["code"] == null)
                return;
            string code = Request["code"].Trim();
            if (code == "")
            {
                return;
            }
            try
            {
                Function fun = new Function();
               movie = fun.getFilmbyUrl(IP + "mov/" + code + "/film.xml");
                cata CATAS = fun.getCatasByCode(code);
                fun.getAdress(CATAS.catasCode);
                ArrayList result = fun.result;
                sub = new data[result.Count];
                int count = (CATAS.catasCode.Length.ToString().Length - 1);
                data m = new data();
                this.Title = CATAS.name;
                string n = "";
                for (int i = 0; i < result.Count; i++)
                {
                    m = ((data)result[i]);
                    sub[(m.key - 1)] = (data)result[i];
                    n = CATAS.name;
                    if (CATAS.isDSJ) n += " 第" + fun.addZero(sub[(m.key - 1)].key, count) + "集";
                    sub[(m.key - 1)].name = n;
                }


            }
            catch (Exception ex)
            {
                Page.ClientScript.RegisterClientScriptBlock(Page.GetType(), "", string.Format("<script type='text/javascript'>alert('{0}');</script>", " 发生错误 " + ex.Message));
                return;
            }
            for (int i = 0; i < sub.Length; i++)
            {

                result.InnerHtml += "<li>" + "<a href=" + sub[i].infoOrAdress + ">" + sub[i].name + "</a>  </li>";
            }
            coverimg.Src = IP + "mov/" + code + @"/1.jpg";
            sum.InnerHtml = "作品:" + movie.name + "<br />" + "导演:" + movie.director + "<br />" + "主演:" + movie.actor +
                "<br />" + "影片类型:" + movie.filmtype + "<br />" + "出产地区:" + movie.region + "<br />" + "上映日期:" + movie.publishTime +
             "<br />" + "更新日期:" + movie.adddate + "<br />" + "介绍:" + movie.brief;

        }
    }
}