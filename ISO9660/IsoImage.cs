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

		[DllImport("shlwapi.dll", CharSet = CharSet.Unicode, PreserveSig = true)]
		static extern void SHCreateStreamOnFile(string pszFile, uint grfMode, out IStream ppstm);

		public IsoImage(string VolumeName) {
			ISO = new MsftFileSystemImage();
			ISO.ChooseImageDefaultsForMediaType(IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_DISK);
			ISO.FileSystemsToCreate = FsiFileSystems.FsiFileSystemISO9660 | FsiFileSystems.FsiFileSystemJoliet;
			ISO.VolumeName = VolumeName;
		}

		public void CreateFile(string FileName, Stream S) {
		}

		public void WriteToFile(string FileName) {
			IFileSystemImageResult Res = ISO.CreateResultImage();

			IStream ImgStream = (IStream)Res.ImageStream;
			STATSTG Stat;
			ImgStream.Stat(out Stat, 0x1);

			if (ImgStream != null) {
				IStream OutStream;
				SHCreateStreamOnFile(FileName, 0x1001, out OutStream);

				ImgStream.CopyTo(OutStream, Stat.cbSize, IntPtr.Zero, IntPtr.Zero);
				OutStream.Commit(0);
			}
		}
	}
}
