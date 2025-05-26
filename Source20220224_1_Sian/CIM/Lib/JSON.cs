using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Strong;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CIM
{
	public class JSON
	{
		#region Static Function
		public static string Serialize<T>(T Obj_, bool Indented_ = false)
		{
			try
			{
				//return JsonConvert.SerializeObject(Obj_, Indented_ ? Formatting.Indented : Formatting.None, new IsoDateTimeConverter() { DateTimeFormat = "yyyy/MM/dd HH:mm:ss" });   // 自動轉換DateTime格式，但有NULL值時還是會出現在JSON格式中
				return JsonConvert.SerializeObject(Obj_, Indented_ ? Formatting.Indented : Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }); // 不需要NULL值也出現																																								  //return JsonConvert.SerializeObject(Obj_, Indented_ ? Formatting.Indented : Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }); // 不需要NULL值也出現
			}
			catch (Exception ex)
			{
				LogWriter.LogException(ex);
				return "";
			}
		}

		public static T DeSerialize<T>(string Str_) where T : new()
		{
			string json = Str_;
			T DeSerializedObject;
			try
			{
				DeSerializedObject = JsonConvert.DeserializeObject<T>(json);
				return DeSerializedObject;
			}
			catch (Exception ex)
			{
				LogWriter.LogException(ex);
				return new T();
			}
		}
		#endregion
	}
}