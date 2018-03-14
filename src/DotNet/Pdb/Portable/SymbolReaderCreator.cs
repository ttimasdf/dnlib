﻿// dnlib: See LICENSE.txt for more info

using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using dnlib.DotNet.MD;
using dnlib.DotNet.Pdb.Symbols;
using dnlib.IO;
using dnlib.PE;

namespace dnlib.DotNet.Pdb.Portable {
	static class SymbolReaderCreator {
		public static SymbolReader TryCreate(DataReaderFactory pdbStream, bool isEmbeddedPortablePdb) {
			try {
				if (pdbStream != null && pdbStream.Length >= 4 && pdbStream.CreateReader().ReadUInt32() == 0x424A5342)
					return new PortablePdbReader(pdbStream, isEmbeddedPortablePdb ? PdbFileKind.EmbeddedPortablePDB : PdbFileKind.PortablePDB);
			}
			catch (IOException) {
			}
			catch {
				pdbStream?.Dispose();
				throw;
			}
			pdbStream?.Dispose();
			return null;
		}

		public static SymbolReader TryCreate(Metadata metadata) {
			if (metadata == null)
				return null;
			try {
				var peImage = metadata.PEImage;
				if (peImage == null)
					return null;
				var embeddedDir = TryGetEmbeddedDebugDirectory(peImage);
				if (embeddedDir == null)
					return null;
				var reader = peImage.CreateReader(embeddedDir.PointerToRawData, embeddedDir.SizeOfData);
				// "MPDB" = 0x4244504D
				if (reader.ReadUInt32() != 0x4244504D)
					return null;
				uint uncompressedSize = reader.ReadUInt32();
				// If this fails, see the (hopefully) updated spec:
				//		https://github.com/dotnet/corefx/blob/master/src/System.Reflection.Metadata/specs/PE-COFF.md#embedded-portable-pdb-debug-directory-entry-type-17
				bool newVersion = (uncompressedSize & 0x80000000) != 0;
				Debug.Assert(!newVersion);
				if (newVersion)
					return null;
				var decompressedBytes = new byte[uncompressedSize];
				using (var deflateStream = new DeflateStream(reader.AsStream(), CompressionMode.Decompress)) {
					int pos = 0;
					while (pos < decompressedBytes.Length) {
						int read = deflateStream.Read(decompressedBytes, pos, decompressedBytes.Length - pos);
						if (read == 0)
							break;
						pos += read;
					}
					if (pos != decompressedBytes.Length)
						return null;
					var stream = ByteArrayDataReaderFactory.Create(decompressedBytes, filename: null);
					return TryCreate(stream, true);
				}
			}
			catch (IOException) {
			}
			return null;
		}

		static ImageDebugDirectory TryGetEmbeddedDebugDirectory(IPEImage peImage) {
			foreach (var idd in peImage.ImageDebugDirectories) {
				if (idd.Type == ImageDebugType.EmbeddedPortablePdb)
					return idd;
			}
			return null;
		}
	}
}
