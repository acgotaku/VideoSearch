<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="VideoSearch._Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>安师大校园网视频解析服务器</title>
    <link href="css/base.css" rel="stylesheet" type="text/css" />
    <script src="Scripts/jquery-1.7.1.js" type="text/javascript"></script>
    <script type="text/javascript">
        $(function () {
            $("#click").click(function () {
                $.post("Search.ashx", { "key": $("#key").val() }, function (data, status) {
                    if (status == "success") {
                        if (data.toString().substring(0, 5) != "error") {
                            var episodes = $.parseJSON(data);
                            $("#result").empty();
                            for (var i = 0; i < episodes.length; i++) {
                                var episode = episodes[i];
                                var li = $("<li>" + episode.name + ":" + episode.infoOrAdress + "</li>");
                                $("#result").append(li);

                            }
                        }
                        else {

                            alert("搜索失败");
                        }

                    }
                    else {
                        alert("与数据库连接失败！");
                    }

                });


            });

            $("#key").keydown(function (e) {
                var curKey = e.which;
                if (curKey == 13) {
                    $("#click").trigger("click");
                }

            });
        });
    
    </script>
</head>
<body>
    <form id="form1" runat="server">
    <div id="content">
        <div id="logo">
        </div>
        <div id="search">
            <input type="text" id="key" /><input type="button" id="click" value="搜 索" />
            <ul id="result">
            </ul>
        </div>
    </div>
    </form>
</body>
</html>
