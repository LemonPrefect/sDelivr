<?php
/*
* phpAPI_BingWallpaperFetch Ver 0.3.0.1
* Author: LemonPrefect
* E-mail: jingzhuokwok@qq.com
* Github: @LemonPrefect
* Website: https://LemonPrefect.cn
* Updated: 201908202325 Asia/Shanghai                                
*/

//define the default parameter for the GET request
$idx = isset($_GET["idx"])?htmlspecialchars($_GET["idx"]):"0";
$region = isset($_GET["region"])?htmlspecialchars($_GET["region"]):"cn";
$resolution = isset($_GET["resolution"])?htmlspecialchars($_GET["resolution"]):"1920x1080";
$swiftSwap = isset($_GET["ss"])?htmlspecialchars($_GET["ss"]):"0";
//execute the function the fetch the information needed
InternalFunctions::FetchWallpaper($idx,$region,$resolution,$wallpaperDetails,$wallpaperUrl);
//check the existence of wallpaper
$urlCheckObj = curl_init();
curl_setopt($urlCheckObj, CURLOPT_URL, $wallpaperUrl);
curl_setopt($urlCheckObj, CURLOPT_HEADER, 1);
curl_setopt($urlCheckObj,CURLOPT_NOBODY,true);
curl_setopt($urlCheckObj, CURLOPT_RETURNTRANSFER, 1);
$data = curl_exec($urlCheckObj);
$_httpCode = curl_getinfo($urlCheckObj,CURLINFO_HTTP_CODE);
curl_close($urlCheckObj);
if($_httpCode == 500){
    $wrongMessage = array();
    $wrongMessage['code'] = "001";
    $wrongMessage['msg'] = "Wallpaper may be not exist.";
    $wrongMessageEcho = json_encode($wrongMessage);
    echo $wrongMessageEcho;
//sort out the request by SwiftSwap
}else if($swiftSwap == 1){
    //output only the Url of the wallpaper
    echo $wallpaperUrl;
}else if($swiftSwap == 2){
    //check if the wallpaper exist in the defined resolution using httpcode
    header("Location:".$wallpaperUrl);
}else{
    //output a detailed json text
    echo $wallpaperDetails;
}
class InternalFunctions{
    public static function FetchWallpaper($idx,$region,$resolution,&$wallpaperDetails,&$wallpaperUrl){
        try{
            $objCurl = curl_init();
            //seperate the Chinese version and the international version
            if($region == "cn"){
                curl_setopt($objCurl,CURLOPT_URL,"http://cn.bing.com/HPImageArchive.aspx?format=js&idx=".$idx."&n=1&ensearch=0");
                $header = array("X-Forwarded-For:14.215.177.39");
                curl_setopt($objCurl,CURLOPT_HTTPHEADER,$header);
            }else{
                curl_setopt($objCurl,CURLOPT_URL,"http://cn.bing.com/HPImageArchive.aspx?format=js&idx=".$idx."&n=1&cc=".$region);
                $header = array("X-Forwarded-For:64.233.161.2");
                curl_setopt($objCurl,CURLOPT_HTTPHEADER,$header);
            }
            $cookie = "ENSEARCH=BENVER=1";
            curl_setopt($objCurl,CURLOPT_COOKIE,$cookie);
            curl_setopt($objCurl, CURLOPT_RETURNTRANSFER, 1);
            curl_setopt($objCurl, CURLOPT_HEADER, 0);
            $response = curl_exec($objCurl);
            if($response == NULL){
                throw new Exception("CURL Error:" . curl_error($objCurl));
            }
            curl_close($objCurl);
        }catch (Exception $error){
            echo $error->getMessage();
        }
        //resolve the original json text to simplify it
        $jsonResponse = json_decode($response,TRUE);
        foreach($jsonResponse['images'] as $imageArray){
            $imageTitle = $imageArray['title'];
            $imageStartDate = $imageArray['startdate'];
            $imageEndDate = $imageArray['enddate'];
            $imageUrlbase = $imageArray['urlbase'];
            $imageCopyright = $imageArray['copyright'];
            $imagehsh = $imageArray['hsh'];
        }
        //reconstruct a json text
        $imageNew = array();
        $imageNew['code'] = "000";
        $imageNew['Title'] = $imageTitle;
        $imageNew['StartDate'] = $imageStartDate;
        $imageNew['EndDate'] = $imageEndDate;
        $imageNew['Url'] = "https://cn.bing.com".$imageUrlbase."_".$resolution.".jpg";
        $imageNew['Copyright'] = $imageCopyright;
        $imageNew['hsh'] = $imagehsh;
        $wallpaperDetails = json_encode($imageNew);
        $wallpaperUrl =  $imageNew['Url'];
        return 0;
    }
}