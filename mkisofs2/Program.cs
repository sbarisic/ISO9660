using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
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
					Console.WriteLine("Boot image is internal boot_hybrid.img");
				else
					Console.WriteLine("Boot image is {0}", BootImg);
			}

			Dir = Path.GetFullPath(Dir);
			string Name = Path.GetFileNameWithoutExtension(Dir);
			Console.Write("Creating {0} ... ", Name + ".iso");

			IsoImage ISO = new IsoImage("ISOIMAGE");
			ISO.AddTree(Dir);

			if (BootImg != null) {
				if (BootImg == "-") {
					ISO.AddDirectory("boot");
					ISO.AddDirectory("boot/grub");
					ISO.AddDirectory("boot/grub/fonts");
					ISO.AddDirectory("boot/grub/i386-pc");
					ISO.AddDirectory("boot/grub/locale");
					ISO.AddDirectory("boot/grub/roms");

					ISO.AddFile("boot/grub/fonts/unicode.pf2", Properties.Resources.unicode);
					ISO.AddFile("boot/grub/locale/en_GB.mo", Properties.Resources.en_GB);
					ISO.AddFile("boot/grub/locale/en_AU.mo", Properties.Resources.en_AU);
					ISO.AddFile("boot/grub/locale/en_CA.mo", Properties.Resources.en_CA);
					ISO.AddFile("boot/grub/locale/hr.mo", Properties.Resources.hr);

					ISO.AddFile("boot.catalog", Properties.Resources.boot_catalog);

					MemoryStream i386pc = new MemoryStream(Properties.Resources.i386pc);
					using (ZipArchive Arch = new ZipArchive(i386pc, ZipArchiveMode.Read, false)) {
						foreach (var E in Arch.Entries)
							using (Stream ES = E.Open())
								ISO.AddFile("boot/grub/i386-pc/" + E.FullName, ES);
						
					}

					ISO.SetBootImage(Properties.Resources.boot_hybrid);
				} else
					ISO.SetBootImage(File.ReadAllBytes(BootImg));
			}


			ISO.WriteToFile(Name + ".iso");
			Console.WriteLine("OK");
		}
	}
}
