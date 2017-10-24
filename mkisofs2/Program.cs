using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using ISO9660;

namespace mkisofs2 {
	class Program {
		static void Error(string Msg) {
			Console.WriteLine("{0}", Msg);
			Environment.Exit(1);
		}

		static void Main(string[] args) {
			string Dir = "";
			string BootImg = null;

			if (args.Length == 1) {
				if (!Directory.Exists(args[0]))
					Error(string.Format("Not valid directory `{0}´", args[0]));

				Dir = args[0];
			} else if (args.Length == 2) {
				if (!Directory.Exists(args[0]))
					Error(string.Format("Not valid directory `{0}´", args[0]));

				Dir = args[0];

				if (args[1] != "-" && !File.Exists(args[1]))
					Error(string.Format("Not a valid boot image `{0}´", args[1]));

				BootImg = args[1];
			} else
				Error("mkisofs2 rootdir [bootimg]");

			if (BootImg != null) {
				if (BootImg == "-")
					Console.WriteLine("Boot image is internal grub2 image");
				else
					Console.WriteLine("Boot image is {0}", BootImg);
			}

			Dir = Path.GetFullPath(Dir);
			string Name = Path.GetFileNameWithoutExtension(Dir);
			Console.Write("Creating {0} ... ", Name + ".iso");

			IsoImage ISO = new IsoImage(Name);
			ISO.AddTree(Dir);

			if (BootImg != null) {
				if (BootImg == "-")
					ISO.SetBootImage(Properties.Resources.grub2);
				else
					ISO.SetBootImage(File.ReadAllBytes(BootImg));
			}

			ISO.WriteToFile(Name + ".iso");

			Console.WriteLine("OK");
		}
	}
}
