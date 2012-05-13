using System;
using axScript3;
using System.IO;
using System.Collections.Generic; 

namespace axScript3Console
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			string Script = File.ReadAllText("main.ax3");
			AxInterpreter ax = new AxInterpreter();
			ax.Run(Script);
			
#if PRINTDEBUG
			"Testing: ".Print();
			5.Print();
			List<object> testList = new List<object>();
			testList.Add("Hello");
			testList.Add("IS IT ME YOU'RE LOOKING FOR?");
			testList.Add(-45.2345);
			testList.Add(new string[] { "a", "b", "c", "d" });
			testList.Print();
#endif
			
#if VARDEBUG
			"Variables: ".Print();
			foreach(var a in ax.Variables) a.Value.Print();
#endif
		}
	}
}
