using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using ISO9660;

namespace mkisofs2 {
	class Program {
		static void Main(string[] args) {
			IsoImage ISO = new IsoImage("Test");
			ISO.WriteToFile("test.iso");

			Console.WriteLine("Done!");
			Console.ReadLine();
		}
	}
}
