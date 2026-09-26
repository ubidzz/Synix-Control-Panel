// ============================================================================
// PROJECT: Synix Game Server Control Panel
// AUTHOR: Jason Turner (ubidzz)
// COPYRIGHT: © 2026 All Rights Reserved.
//
// LEGAL NOTICE:
// This source code is proprietary and confidential.
// 1. Permission is granted for PERSONAL, NON-COMMERCIAL use only.
// 2. You may modify this code for your own use, but you may NOT redistribute,
//    rebrand, or sell this code or derivative works without written consent.
// 3. The "Synix" brand and logic remain the property of Jason Turner.
// ============================================================================
using System.Net;
using System.Security.Cryptography;
using System.Text.Json;

namespace Synix_Control_Panel.SynixApp.ServerHandler;

internal sealed record MinecraftRuntimeArtifact(Uri Url, string Hash, HashAlgorithmName Algorithm);

internal static class MinecraftPluginRuntime
{
	internal const string Paper = "Paper";
	internal const string Purpur = "Purpur";
	private static readonly HttpClient Client = CreateClient();
	internal static bool IsPluginRuntime(string? loader) =>
		string.Equals(loader, Paper, StringComparison.OrdinalIgnoreCase) ||
		string.Equals(loader, Purpur, StringComparison.OrdinalIgnoreCase);

	internal static async Task<IReadOnlyList<string>> GetBuildsAsync(string loader, string version,
		CancellationToken cancellationToken = default)
	{
		string url = loader == Paper
			? $"https://fill.papermc.io/v3/projects/paper/versions/{Uri.EscapeDataString(version)}/builds"
			: $"https://api.purpurmc.org/v2/purpur/{Uri.EscapeDataString(version)}";
		using JsonDocument? document = await GetJsonAsync(url, cancellationToken).ConfigureAwait(false);
		return document == null ? [] : ParseBuilds(loader, document.RootElement);
	}

	internal static IReadOnlyList<string> ParseBuilds(string loader, JsonElement root)
	{
		IEnumerable<string> builds = loader == Paper
			? root.EnumerateArray().Where(build => build.GetProperty("channel").GetString() == "STABLE")
				.Select(build => build.GetProperty("id").ToString())
			: root.GetProperty("builds").GetProperty("all").EnumerateArray().Select(build => build.ToString());
		return builds.Where(value => int.TryParse(value, out int number) && number > 0)
			.Distinct().OrderByDescending(int.Parse).ToArray();
	}

	internal static async Task<MinecraftRuntimeArtifact> GetArtifactAsync(string loader, string version,
		string build, CancellationToken cancellationToken = default)
	{
		if (!IsPluginRuntime(loader) || !int.TryParse(build, out int number) || number <= 0)
			throw new InvalidDataException(LocalizationManager.Get("MinecraftWorkspace.Error.Runtime"));
		string path = $"{Uri.EscapeDataString(version)}/{number}";
		string url = loader == Paper
			? $"https://fill.papermc.io/v3/projects/paper/versions/{Uri.EscapeDataString(version)}/builds/{number}"
			: $"https://api.purpurmc.org/v2/purpur/{path}";
		using JsonDocument? document = await GetJsonAsync(url, cancellationToken).ConfigureAwait(false);
		if (document == null) throw new InvalidDataException(LocalizationManager.Get("MinecraftWorkspace.Error.Runtime"));
		JsonElement root = document.RootElement;
		if (loader == Paper)
		{
			if (root.GetProperty("channel").GetString() != "STABLE")
				throw new InvalidDataException(LocalizationManager.Get("MinecraftWorkspace.Error.Runtime"));
			JsonElement download = root.GetProperty("downloads").GetProperty("server:default");
			Uri downloadUrl = new(download.GetProperty("url").GetString()!);
			if (downloadUrl.Scheme != "https" || downloadUrl.Host != "fill-data.papermc.io" ||
				!downloadUrl.IsDefaultPort || downloadUrl.UserInfo.Length > 0)
				throw new InvalidDataException(LocalizationManager.Get("MinecraftWorkspace.Error.Runtime"));
			return ValidateArtifact(downloadUrl, download.GetProperty("checksums").GetProperty("sha256").GetString()!, HashAlgorithmName.SHA256);
		}
		if (root.GetProperty("result").GetString() != "SUCCESS")
			throw new InvalidDataException(LocalizationManager.Get("MinecraftWorkspace.Error.Runtime"));
		// Purpur publishes MD5 for transfer integrity. HTTPS is the source-authentication boundary.
		return ValidateArtifact(new Uri(url + "/download"), root.GetProperty("md5").GetString()!, HashAlgorithmName.MD5);
	}

	private static MinecraftRuntimeArtifact ValidateArtifact(Uri url, string hash, HashAlgorithmName algorithm)
	{
		if (hash.Length != (algorithm == HashAlgorithmName.SHA256 ? 64 : 32) || !hash.All(Uri.IsHexDigit))
			throw new InvalidDataException(LocalizationManager.Get("MinecraftWorkspace.Error.Runtime"));
		return new(url, hash, algorithm);
	}

	private static async Task<JsonDocument?> GetJsonAsync(string url, CancellationToken token)
	{
		using HttpResponseMessage response = await Client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, token).ConfigureAwait(false);
		if (response.StatusCode == HttpStatusCode.NotFound) return null;
		response.EnsureSuccessStatusCode();
		await response.Content.LoadIntoBufferAsync(8 * 1024 * 1024, token).ConfigureAwait(false);
		return JsonDocument.Parse(await response.Content.ReadAsStringAsync(token).ConfigureAwait(false));
	}

	private static HttpClient CreateClient()
	{
		HttpClient client = new(new HttpClientHandler { AllowAutoRedirect = false }) { Timeout = TimeSpan.FromSeconds(30) };
		client.DefaultRequestHeaders.UserAgent.ParseAdd("SynixControlPanel/1.0 (+https://github.com/ubidzz/Synix-Control-Panel)");
		return client;
	}
}
