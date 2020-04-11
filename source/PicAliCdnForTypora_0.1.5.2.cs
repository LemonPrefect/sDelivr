﻿/*
* PicAliCdnForTypora Ver 0.1.5.2 .Net Framework 4.6+
* Author: LemonPrefect
* E-mail: jingzhuokwok@qq.com
* Github: @LemonPrefect
* Website: https://LemonPrefect.cn
* Updated: 202004092256 Asia/Shanghai                                
*/
using System;
using System.IO;
using System.Net;
using System.Text;
using Flurl.Http;
using System.Threading;
using Newtonsoft.Json.Linq;

namespace PicAlicdnForTypora {
    internal class Program {
        const int REST_PER_REQUEST = 300;
        const int RETRY_PER_FAILED_REQUEST = 5;
        
        public class EquivocalUtf8EncodingProvider : EncodingProvider { //Make the utf8 from response Content-Type into utf-8
            public override Encoding GetEncoding(string name){
                return name == "utf8" ? Encoding.UTF8 : null;
            }
            public override Encoding GetEncoding(int codepage){
                return null;
            }
        }
        
        public static void Main(string[] args){
            Random randomNum = new Random();
            EncodingProvider provider = new EquivocalUtf8EncodingProvider();
            Encoding.RegisterProvider(provider);
            int imageQuantity = args.Length;
            string uploadUrl = "http://api.uomg.com/api/image.ali";
            string[] fetchedUrl = new string[imageQuantity];
            int flagRetry = 0;
            string[] userAgents = {
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/73.0.3683.103 Safari/537.36",
                "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_7_0) AppleWebKit/535.11 (KHTML, like Gecko) Chrome/17.0.963.56 Safari/535.11",
                "Opera/9.80 (Macintosh; Intel Mac OS X 10.6.8; U; en) Presto/2.8.131 Version/11.11",
                "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1; Trident/4.0; SE 2.X MetaSr 1.0; SE 2.X MetaSr 1.0; .NET CLR 2.0.50727; SE 2.X MetaSr 1.0)",
                "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1; TencentTraveler 4.0)"
            };
            for (int i = 0; i < imageQuantity; i++){
                
                //For those images already on the website which needn't to be uploaded
                if (args[i].Contains("http://") || args[i].Contains("https://") || args[i].Contains("ftp://")){
                    fetchedUrl[i] = args[i];
                    continue;
                }
                
                //For inserting image with 'file:///' which will be strip into '/'
                if ('/' == args[i][0]){
                    args[i] = args[i].Substring(1);
                }

                if (File.Exists(args[i]) == false){
                    Console.WriteLine("Error:File  File to upload is not exist");
                    Thread.CurrentThread.Abort();
                }

                //Read the file into stream for upload to prevent the unsupported characters from making the upload fail
                FileStream uploadImg = new FileStream(args[i],FileMode.Open,FileAccess.Read, FileShare.Read);
                byte[] bytesImg = new byte[uploadImg.Length];
                uploadImg.Read(bytesImg, 0, bytesImg.Length);
                uploadImg.Close();
                Stream uploadImgStream = new MemoryStream(bytesImg);
                
                var uploadRespose = uploadUrl.WithHeaders(new {
                    User_Agent = userAgents[randomNum.Next(4)],
                    Client_IP = randomNum.Next(192) + "." + randomNum.Next(255) + "." + randomNum.Next(255) + "." + randomNum.Next(255)
                }).PostMultipartAsync(data => 
                    data.AddString("file","multipart")
                        .AddFile("Filedata",uploadImgStream,WebUtility.UrlEncode(args[i]))
                ).Result;
                
                uploadImgStream.Dispose();
                
                //Try to catch some exceptions when uploaded failed and abort the thread
                if (uploadRespose.StatusCode != 200){
                    ExceptionController(ref flagRetry, ref i, "Error:Device.Network  Failed to upload file for 5 times");
                    continue;
                }
                string responseData = uploadRespose.ResponseMessage.Content.ReadAsStringAsync().Result;
                if (responseData.Contains("不符合文件上传的标准")){
                    ExceptionController(ref flagRetry, ref i, "Error:API-BtPanel  Failed to upload file for 5 times as the file is forbidden");
                    continue;
                }
                
                //Convert the response data into a Json Object
                JObject responseDataParsed = JObject.Parse(responseData);
                if ((string) responseDataParsed["code"] != "1"){
                    ExceptionController(ref flagRetry, ref i, "Error:API  Failed to upload file for 5 times");
                    continue;
                }
                fetchedUrl[i] = (string)responseDataParsed["imgurl"];
                flagRetry = 0;
                Thread.Sleep(REST_PER_REQUEST);
            }
            Console.WriteLine("Upload Success:");
            for (int i = 0; i < imageQuantity; i++){ //Output Urls as demanded
                Console.WriteLine(fetchedUrl[i]);
            }
        }
        internal static void ExceptionController(ref int flagRetry,ref int i,string exceptionMessage){//Receive and process the exceptions
            if (flagRetry == RETRY_PER_FAILED_REQUEST){
                Console.WriteLine(exceptionMessage);
                Thread.CurrentThread.Abort();
            }
            i--;
            flagRetry++;
        }
    }
}