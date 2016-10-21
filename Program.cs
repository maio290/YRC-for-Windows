// File created on 14.10.2016 at 16:39
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace Yamaha_Remote_Console
{
	class Program
	{

		static bool debug = false;
		static bool disable_sleep = false;
		
		public static string server_playback_stop(string ip)
		{
			return postXMLData("http://"+ip+"/YamahaRemoteControl/ctrl","<YAMAHA_AV cmd=\"PUT\"><SERVER><Play_Control><Playback>Stop</Playback></Play_Control></SERVER></YAMAHA_AV>");
		}
		
		public static string server_page_down(string ip)
		{
			return postXMLData("http://"+ip+"/YamahaRemoteControl/ctrl","<YAMAHA_AV cmd=\"PUT\"><SERVER><List_Control><Page>Down</Page></List_Control></SERVER></YAMAHA_AV>");
		}
		
		public static string server_page_up(string ip)
		{
			return postXMLData("http://"+ip+"/YamahaRemoteControl/ctrl","<YAMAHA_AV cmd=\"PUT\"><SERVER><List_Control><Page>Up</Page></List_Control></SERVER></YAMAHA_AV>");
		}
		
		public static string server_get_list(string ip)
		{
			return postXMLData("http://"+ip+"/YamahaRemoteControl/ctrl","<YAMAHA_AV cmd=\"GET\"><SERVER><List_Info>GetParam</List_Info></SERVER></YAMAHA_AV>");
		}
		
		public static string server_direct_sel(string ip, int listelement)
		{
			return postXMLData("http://"+ip+"/YamahaRemoteControl/ctrl","<YAMAHA_AV cmd=\"PUT\"><SERVER><List_Control><Direct_Sel>Line_"+listelement+"</Direct_Sel></List_Control></SERVER></YAMAHA_AV>");
		}
		
		public static int server_determine_list_pos(string ip, string reply_xml, string tofind)
		{
			// Actually, the receiver is using XML, but since we can handle it better as string, we do handle it as string. 
			// Since Yamaha is using paging, we have to watch out for multiple lists 
			
			int max_line_start = reply_xml.IndexOf("<Max_Line>")+"<Max_Line>".Length;
			int max_line_end = reply_xml.IndexOf("</Max_Line>",max_line_start);
			int max_line = Convert.ToInt32(reply_xml.Substring(max_line_start,max_line_end-max_line_start));
			int pages = 0;
			
			if((max_line%8) != 0)
			{
				pages = (max_line/8)+1;
			}
			else
			{
				pages = (max_line/8);
			}
			
			
			
			
			while(pages != 0)
			{
							
				while(reply_xml.IndexOf("<Menu_Status>Busy</Menu_Status>") != -1)
				{	if(!disable_sleep)
					{Thread.Sleep(125);}
					reply_xml = server_get_list(ip);}
				
				// check if the current element is contained in the list 
				if(reply_xml.IndexOf(tofind) != -1)
				{
					// extract the listpos of it
					int listpos_strpos = reply_xml.IndexOf(tofind)-7;
					try
					{
					int listpos = Convert.ToInt32(reply_xml.Substring(listpos_strpos,1));
					if(debug)
					{Console.WriteLine("Returning: " + listpos);}
					return listpos;
					}
					catch(FormatException f)
					{}
					
				}
		
				// update the reply_xml to the next page
				server_page_down(ip);
				pages--;
				reply_xml = server_get_list(ip);
			}
			
			return -1;
		}
		
		public static void set_server(string ip, string[] input_commands)
		{
			// Ensure that input is SERVER
			set_input(ip,"SERVER");
			// Mute the autoplay
			server_playback_stop(ip);
			
			
			
			for(int i = 0; i<input_commands.Length; i++)
			{
				if(debug)
				{Console.WriteLine("Currently looking for: " + input_commands[i]);}
				string reply_xml = server_get_list(ip);
				
				while(reply_xml.IndexOf("<Menu_Status>Busy") != -1)
				{
				reply_xml = server_get_list(ip);
				}
				
				int list_pos = server_determine_list_pos(ip,reply_xml,input_commands[i]);
				
				if(list_pos == -1)
				{
					Console.WriteLine("Failed to set to: " + input_commands[i]);
					continue;
				}
				
				server_direct_sel(ip,list_pos);
				
				// The sleep shall ensure that the receiver did perform our action since there is no success or error state
				if(!disable_sleep)
				{Thread.Sleep(1000);}
				
			}
			
			
			
		}
		
		public static void fade_volume(string ip, string ss, string fs, string sts, string sls)
		{
			int start;
			int final;
			int step;
			int sleep;
			
			try
			{start = Convert.ToInt32(ss);
			final = Convert.ToInt32(fs);
			step = Convert.ToInt32(sts);
			sleep = Convert.ToInt32(sls);
			}
			catch(FormatException e)
			{
				Console.Error.WriteLine("[CRITICAL] Invalid arguments for fade_volume!");
				return;
			}
			
			if(final > start)
			{
				for(int i = start; i<= final; i=i+step)
				{
					set_volume(ip,i);
					Thread.Sleep(sleep);
				}
			}
			else
			{
				for(int i = start; i>= final; i-=step)
				{
					set_volume(ip,i);
					Thread.Sleep(sleep);
				}			
			}
		}
		
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
						case "-fadevol":
							fade_volume(ip,args[++i],args[++i],args[++i],args[++i]);
							break;
						case "-directcommand":
							Console.WriteLine("Executing command directly: " + args[i+1]);
							try
							{
							Console.WriteLine(postXMLData("http://"+ip+"/YamahaRemoteControl/ctrl",args[++i]));
							}
							catch(WebException e)
							{
								Console.Error.WriteLine("[CRITICAL] The direct command has failed!");
								Console.Error.WriteLine(e.StackTrace);
							}
							break;
						case "-set_server" :
						string[] input_commands = args[++i].Split(';');
						set_server(ip,input_commands);
						break;
						case "-disable_sleep":
						// I do not recommend to do so!
						disable_sleep = true;
						break;
						case "-debug":
						debug = true;
						break;
						
						
							
					}
				}
			}catch(IndexOutOfRangeException e)
			{
				Console.Error.WriteLine("[CRITICAL] You have definied too less arguments for the functions you're using!");
			}
			
		}
	}
}