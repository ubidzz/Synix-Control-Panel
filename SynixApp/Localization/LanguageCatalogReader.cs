using System.Collections;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Resources;

namespace Synix_Control_Panel.SynixApp.Localization;

/// <summary>Reads a loose satellite's string data without loading its assembly.</summary>
internal static class LanguageCatalogReader
{
	internal static IReadOnlyDictionary<string, string>? ReadSatellite(string path, string culture)
	{
		try
		{
			using FileStream file = File.OpenRead(path);
			using PEReader image = new(file);
			if (!image.HasMetadata || image.PEHeaders.CorHeader is not { } header)
				return null;

			DirectoryEntry directory = header.ResourcesDirectory;
			if (directory.Size < sizeof(int) || directory.Size > 64 * 1024 * 1024)
				return null;
			MetadataReader metadata = image.GetMetadataReader();
			string expectedName = $"Synix_Control_Panel.Localization.Strings.{culture}.resources";
			foreach (ManifestResourceHandle handle in metadata.ManifestResources)
			{
				ManifestResource resource = metadata.GetManifestResource(handle);
				if (!resource.Implementation.IsNil || metadata.GetString(resource.Name) != expectedName)
					continue;
				if (resource.Offset < 0 || resource.Offset > directory.Size - sizeof(int))
					return null;
				var blob = image.GetSectionData(directory.RelativeVirtualAddress)
					.GetReader((int)resource.Offset, directory.Size - (int)resource.Offset);
				int length = blob.ReadInt32();
				if (length < 0 || length > blob.RemainingBytes)
					return null;
				using MemoryStream data = new(blob.ReadBytes(length), writable: false);
				using ResourceReader reader = new(data);
				Dictionary<string, string> entries = new(StringComparer.Ordinal);
				foreach (DictionaryEntry entry in reader)
				{
					if (entry.Key is string key && entry.Value is string value)
						entries.Add(key, value);
				}
				return entries;
			}
		}
		catch (Exception exception) when (exception is IOException or UnauthorizedAccessException
			or BadImageFormatException or ArgumentException or NotSupportedException)
		{
			// An unreadable or malformed catalog is never eligible for the picker.
		}
		return null;
	}
}
