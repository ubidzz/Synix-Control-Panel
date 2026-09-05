// ============================================================================
// PROJECT: Synix Game Server Control Panel
// AUTHOR: Jason Turner (ubidzz)
// COPYRIGHT: © 2026 All Rights Reserved.
// ============================================================================

using System.Reflection;
using Xunit;

namespace Synix_Control_Panel.Tests;

public sealed class UserInterfaceResourceTests
{
	[Fact]
	public void EmbeddedFormResources_MatchTheirCurrentTypeNamespaces()
	{
		Assembly assembly = typeof(MainGUI).Assembly;
		string expectedDashboardResource =
			$"{typeof(MainGUI).FullName}.resources";
		string[] resourceNames = assembly.GetManifestResourceNames();

		Assert.Contains(expectedDashboardResource, resourceNames);

		string[] formResourceNames = resourceNames
			.Where(name => name.EndsWith(
				".resources",
				StringComparison.Ordinal))
			.Where(name => !string.Equals(
				name,
				"Synix_Control_Panel.Localization.Strings.resources",
				StringComparison.Ordinal))
			.Where(name => !string.Equals(
				name,
				"Synix_Control_Panel.Properties.Resources.resources",
				StringComparison.Ordinal))
			.ToArray();

		foreach (string resourceName in formResourceNames)
		{
			string typeName = resourceName[..^".resources".Length];
			Assert.NotNull(assembly.GetType(typeName));
		}
	}
}
