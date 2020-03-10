let timeNow = new Date();
let timestampNow = timeNow.getTime();
if($.cookie('sunrise') === undefined || $.cookie('sunset') === undefined || returnCitySN["cip"] !== $.cookie('ip')) {
    console.log("IsNight? Run!");
    var details = {};
    details["TIME"] = parseInt(new Date().getTime() / 1000);
    details["IP_ADDRESS"] = returnCitySN["cip"];
    let data = JSON.stringify(details);
    data = BASE64.encode(data);
    data = "PLKOZ23476/" + data;

    $.ajax({
        type: "GET",
        async: false,
        url: "https://api.lemonprefect.cn/?" + data,
        success: function (data) {
            let isNigntArray = data.split(",");
            let timeExpire = new Date(timeNow.toLocaleDateString()).getTime() - 1;
            let timeLeftTillExpire = 24 * 60 * 60 * 1000 - (timestampNow - timeExpire);
            let cookieValidTime = new Date();
            cookieValidTime.setTime(timeLeftTillExpire + timestampNow);
            $.cookie('sunrise',isNigntArray[0],{expires: cookieValidTime,path: '/'});
            $.cookie('sunset',isNigntArray[1],{expires: cookieValidTime,path: '/'});
            $.cookie('ip',returnCitySN["cip"],{expires: cookieValidTime,path: '/'});
        },
        error: function () {
            console.log("Is Night?");
        }
    });
}
let timeNowCompare = parseInt(timestampNow / 1000);
if(timeNowCompare  > $.cookie('sunset') || timeNowCompare < $.cookie('sunrise')){
    //var shade = document.createElement('div');
    //shade.innerHTML = '<div style="position:fixed;background-color:#000;top:0;left:0;z-index:2147483647;pointer-events:none;opacity:0.33;width:100%;height:100%"></div>';
    //document.body.appendChild(shade);
    console.log("Good Evening!");
}else{
    console.log("Maybe it's daytime now.")
}
console.log("日出：" + $.cookie('sunrise'));
console.log("日落：" + $.cookie('sunset'));
console.log("咱不会写不用Pjax的局部刷新夜间模式~");