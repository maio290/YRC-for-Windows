// File created on 14.10.2016 at 16:39
using System;
using System.IO;
using System.Net;
using System.Text;

namespace Yamaha_Remote_Console
{
	class Program
	{

		
		public static string power_on(string ip)
		{
			return postXMLData("http://"+ip+"/YamahaRemoteControl/ctrl","<YAMAHA_AV cmd=\"PUT\"><Main_Zone><Power_Control><Power>On</Power></Power_Control></Main_Zone></YAMAHA_AV>");
		}
		
		public static string power_off(string ip)
		{
			return postXMLData("http://"+ip+"/YamahaRemoteControl/ctrl","<YAMAHA_AV cmd=\"PUT\"><Main_Zone><Power_Control><Power>Standby</Power></Power_Control></Main_Zone></YAMAHA_AV>");
		}
		
		public static string set_volume(string ip, int level)
		{
			return postXMLData("http://"+ip+"/YamahaRemoteControl/ctrl","<YAMAHA_AV cmd=\"PUT\"><Main_Zone><Volume><Lvl><Val>-"+level+"</Val><Exp>1</Exp><Unit>dB</Unit></Lvl></Volume></Main_Zone></YAMAHA_AV>");
		}
		
		public static string set_input(string ip, string input)
		{
			return postXMLData("http://"+ip+"/YamahaRemoteControl/ctrl","<YAMAHA_AV cmd=\"PUT\"><Main_Zone><Input><Input_Sel>"+input+"</Input_Sel></Input></Main_Zone></YAMAHA_AV>");
		}
		
		public static string postXMLData(string destinationUrl, string requestXml)
		{
			// This function was provided by AlliterativeAlice on Stackoverflow.com
			// http://stackoverflow.com/questions/17535872/
		    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(destinationUrl);
		    byte[] bytes;
		    bytes = System.Text.Encoding.ASCII.GetBytes(requestXml);
		    request.ContentType = "text/xml; encoding='utf-8'";
		    request.ContentLength = bytes.Length;
		    request.Method = "POST";
		    Stream requestStream = request.GetRequestStream();
		    requestStream.Write(bytes, 0, bytes.Length);
		    requestStream.Close();
		    HttpWebResponse response;
		    response = (HttpWebResponse)request.GetResponse();
		    if (response.StatusCode == HttpStatusCode.OK)
		    {
		        Stream responseStream = response.GetResponseStream();
		        string responseStr = new StreamReader(responseStream).ReadToEnd();
		        return responseStr;
		    }
		    return null;
		}
			
		public static void Main(string[] args)
		{
	
			
			
			try
			{
				string ip = args[0];
				for(int i = 1; i<args.Length; i++)
				{
					switch(args[i])
					{
						case "-power_on":
							power_on(ip);
							break;
						case "-power_off":
							power_off(ip);
							break;
						case "-setvol":
							int level = Convert.ToInt32(args[i+1]);		
							if(level > 800)
							{level = 805;}
							set_volume(ip,level);
							i++;
							break;
						case "-input":
							set_input(ip,args[i+1]);
							i++;
							break;
						
							
					}
				}
			}catch(IndexOutOfRangeException e)
			{}
			
		}
	}
}