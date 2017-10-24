using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using IStream = System.Runtime.InteropServices.ComTypes.IStream;
using STATSTG = System.Runtime.InteropServices.ComTypes.STATSTG;
using IMAPI2FS;

namespace ISO9660 {
	public class IsoImage {
		MsftFileSystemImage ISO;

		[DllImport("shlwapi", CharSet = CharSet.Unicode, PreserveSig = true)]
		static extern void SHCreateStreamOnFile(string pszFile, uint grfMode, out IStream ppstm);

		[DllImport("shlwapi", CharSet = CharSet.Unicode, PreserveSig = true)]
		static extern IStream SHCreateMemStream(byte[] Data, uint Len);

		public IsoImage(string VolumeName) {
			ISO = new MsftFileSystemImage();
			ISO.ChooseImageDefaultsForMediaType(IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_DISK);
			ISO.FileSystemsToCreate = FsiFileSystems.FsiFileSystemISO9660 | FsiFileSystems.FsiFileSystemJoliet;
			ISO.VolumeName = VolumeName;
		}

		public void SetBootImage(Stream S) {
			using (MemoryStream MS = new MemoryStream()) {
				S.CopyTo(MS);
				SetBootImage(MS.ToArray());
			}
		}

		public void SetBootImage(byte[] Data) {
			FsiStream S = (FsiStream)SHCreateMemStream(Data, (uint)Data.Length);

			BootOptions BootImageOptions = new BootOptions();
			BootImageOptions.Manufacturer = "Microsoft";
			BootImageOptions.Emulation = EmulationType.EmulationNone;
			BootImageOptions.PlatformId = PlatformId.PlatformX86;
			BootImageOptions.AssignBootImage(S);
			ISO.BootImageOptions = BootImageOptions;
		}

		public void AddFile(string FileName, Stream S) {
			using (MemoryStream MS = new MemoryStream()) {
				S.CopyTo(MS);
				AddFile(FileName, MS.ToArray());
			}
		}

		public bool AddFile(string FileName, byte[] Data) {
			try {
				ISO.Root.AddFile(FileName, (FsiStream)SHCreateMemStream(Data, (uint)Data.Length));
			} catch (Exception) {
				return false;
			}
			return true;
		}

		public bool AddDirectory(string DirName) {
			try {
				ISO.Root.AddDirectory(DirName);
			} catch (Exception) {
				return false;
			}
			return true;
		}

		public void AddTree(string Source, bool IncludeBase = false) {
			ISO.Root.AddTree(Source, IncludeBase);
		}


		public void WriteToFile(string FileName, bool Overwrite = true) {
			IFileSystemImageResult Res = ISO.CreateResultImage();
			IStream ImgStream = (IStream)Res.ImageStream;

			if (ImgStream != null) {
				STATSTG Stat;
				ImgStream.Stat(out Stat, 0x1);

				if (File.Exists(FileName)) {
					if (Overwrite)
						File.Delete(FileName);
					else
						throw new Exception("File already exists: " + FileName);
				}

				IStream OutStream;
				SHCreateStreamOnFile(FileName, 0x1001, out OutStream);

				ImgStream.CopyTo(OutStream, Stat.cbSize, IntPtr.Zero, IntPtr.Zero);
				OutStream.Commit(0);
			}
		}
	}
}
