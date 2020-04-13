﻿/*
* PicAliCdnForTypora Ver 0.1.5.3 .Net Framework 4.6+ / .Net Core 3.1+ Build Swelling Sugar PreRelease
* Author: LemonPrefect
* E-mail: jingzhuokwok@qq.com
* Github: @LemonPrefect
* Website: https://LemonPrefect.cn
* Updated: 202004131731 Asia/Shanghai                                
*/
using System;
using System.IO;
using Flurl.Http;
using System.Threading;
using Newtonsoft.Json.Linq;

namespace PicAlicdnForTypora {
    internal class Program {
        const int REST_PER_REQUEST = 300;
        const int RETRY_PER_FAILED_REQUEST = 5;
        
        
        public static void Main(string[] args){
            Random randomNum = new Random();
            int imageQuantity = args.Length;
            string uploadUrl = "https://kfupload.alibaba.com/mupload";
            string[] fetchedUrl = new string[imageQuantity];
            int flagRetry = 0;
            
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
                    User_Agent = "iAliexpress/6.22.1 (iPhone; iOS 12.1.2; Scale/2.00)",
                    Client_IP = randomNum.Next(192) + "." + randomNum.Next(255) + "." + randomNum.Next(255) + "." + randomNum.Next(255)
                }).PostMultipartAsync(data => 
                    data.AddString("scene","aeMessageCenterV2ImageRule")
                        .AddString("name",randomNum.Next(65536) + Path.GetExtension(args[i]))
                        .AddFile("file",uploadImgStream,randomNum.Next(65536) + Path.GetExtension(args[i]),"image/" + Path.GetExtension(args[i]).Substring(1))
                ).Result;
                
                uploadImgStream.Dispose();
                
                //Try to catch some exceptions when uploaded failed and abort the thread
                if (uploadRespose.StatusCode != 200){
                    ExceptionController(ref flagRetry, ref i, "Error:Device.Network  Failed to upload file for 5 times");
                    continue;
                }
                
                string responseData = uploadRespose.ResponseMessage.Content.ReadAsStringAsync().Result;

                //Convert the response data into a Json Object
                JObject responseDataParsed = JObject.Parse(responseData);
                if ((string) responseDataParsed["code"] != "0"){
                    ExceptionController(ref flagRetry, ref i, "Error:API  Failed to upload file for 5 times");
                    continue;
                }
                fetchedUrl[i] = (string)responseDataParsed["url"];
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