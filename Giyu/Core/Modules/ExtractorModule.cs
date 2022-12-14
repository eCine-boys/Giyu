using Jint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace Giyu.Core.Modules
{
    public static class ExtractorModule
    {

		public static async Task<dynamic> GetInfo(string id)
        {
			await Task.Yield();

			var utf8 = new UTF8Encoding(false);
			var wc = new WebClient();
			wc.Encoding = utf8;

			var dats = await wc.DownloadStringTaskAsync(new Uri(string.Format("https://youtube.com/get_video_info?video_id={0}&el=detailpage", id)));
			var dat = dats.Split('&')
				.Where(xs => !string.IsNullOrWhiteSpace(xs))
				.Select(xs => xs.Split('='))
				.ToDictionary(xsa => xsa[0], xsa => HttpUtility.UrlDecode(xsa[1]));

			if (dat.ContainsKey("reason"))
				throw new Exception(dat["reason"]);

			return dat;
			// You can use those to get some video info
			//var thumb = dat["thumbnail_url"];
			//var title = dat["title"];
			//var authr = dat["author"];
		}
		public static async Task<string> GetYtAudioUrl(string id)
		{
			await Task.Yield();

			var utf8 = new UTF8Encoding(false);
			var wc = new WebClient();
			wc.Encoding = utf8;

			var dats = await wc.DownloadStringTaskAsync(new Uri(string.Format("https://youtube.com/get_video_info?video_id={0}&el=detailpage", id)));
			var dat = dats.Split('&')
				.Where(xs => !string.IsNullOrWhiteSpace(xs))
				.Select(xs => xs.Split('='))
				.ToDictionary(xsa => xsa[0], xsa => HttpUtility.UrlDecode(xsa[1]));

			if (dat.ContainsKey("reason"))
				throw new Exception(dat["reason"]);

			// You can use those to get some video info
			//var thumb = dat["thumbnail_url"];
			//var title = dat["title"];
			//var authr = dat["author"];

			var fmtss = dat["adaptive_fmts"];
			var fmts = fmtss.Split(',')
				.Where(xs => !string.IsNullOrWhiteSpace(xs))
				.Select(xs => xs.Split('&')
					.Where(xxs => !string.IsNullOrWhiteSpace(xxs))
					.Select(xxs => xxs.Split('='))
					.ToDictionary(xxsa => xxsa[0], xxsa => HttpUtility.UrlDecode(xxsa[1])))
				.Select(xd => new { url = xd["url"], type = xd["type"], bitrate = int.Parse(xd["bitrate"]), sig = xd.ContainsKey("s") ? xd["s"] : null })
				.OrderByDescending(xa => xa.bitrate);

			var fmt = fmts.FirstOrDefault(xa => xa.type.StartsWith("audio/mp4"));
			if (fmt == null)
				throw new InvalidOperationException("The audio stream did not contain suitable formats.");

			var url = fmt.url;

			if (fmt.sig != null)
			{
				var sig = fmt.sig;

				// decode
				var dpage = await wc.DownloadStringTaskAsync(string.Concat("https://www.youtube.com/watch?v=", id));

				var dreg = new Regex(@"""js"":""(.+?)""(\}|,)");
				var dm = dreg.Match(dpage);
				var djloc = string.Concat("https:", Regex.Unescape(dm.Groups[1].Value));

				var djs = await wc.DownloadStringTaskAsync(djloc);

				dreg = new Regex(@"\.set\(""signature"",(([a-zA-Z]+?))\(.+?\)\);");
				dm = dreg.Match(djs);
				var dfunc = dm.Groups[1].Value;

				dreg = new Regex(string.Concat(dfunc, @"=function\(([a-zA-Z]+?)\)\{(.+?)\};"));
				dm = dreg.Match(djs);

				var dargn = dm.Groups[1].Value;
				var dalg = dm.Groups[2].Value;
				var djsfunc = string.Concat("var unscramble = function(", dargn, ") { ", dalg, " };");

				var dalgps = dalg.Split(';');
				var dalgrs = new HashSet<string>();
				foreach (var dalgp in dalgps)
					if (!dalgp.StartsWith(dargn) && !dalgp.StartsWith(string.Concat(dargn, "=")) && !dalgp.StartsWith("return "))
						dalgrs.Add(dalgp.Split('.')[0]);

				dreg = new Regex(string.Concat("var ", dalgrs.First(), @"=\{(.+?)\};"), RegexOptions.Singleline);
				dm = dreg.Match(djs);

				dalg = dm.Groups[0].Value;

				djsfunc = string.Concat(dalg, "\n", djsfunc, "\nunscramble(sig);");

				var jseng = new Engine();
				sig = jseng.SetValue("sig", sig)
					.Execute(djsfunc)
					.GetCompletionValue()
					.ToObject()
					.ToString();

				url = string.Concat(url, "&signature=", sig);
			}

			return url;
		}
	}
}
