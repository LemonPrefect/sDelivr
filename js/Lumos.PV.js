var details = {};
details["OPERATION"] = "AddPV";
details["TIME"] = parseInt(new Date().getTime()/1000);
details["PASSAGE_TITLE"] = document.title;
details["IP_ADDRESS"] = returnCitySN["cip"];
details["LOCATION"] = returnCitySN["cname"];
data = JSON.stringify(details);
console.log(data);
data = BASE64.encode(data);
console.log(data);
data = "23RY/" + data;


$.ajax({
    type : "GET",
    async : false,
    url: "http://api.lemonprefect.cn/?" + data,
    success : function(data) {
        console.log("PV Added");
    },
    error : function() {
        console.log("PV Addition Failed");
    }
});