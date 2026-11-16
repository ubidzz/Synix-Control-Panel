using System.Runtime.CompilerServices;

namespace Synix_Control_Panel.SynixApp.Localization;

/// <summary>Uses installed Windows script fonts where default font linking is incomplete.</summary>
internal static class LocalizedFont
{
	private static readonly ConditionalWeakTable<object, FontBinding> Bindings = new();

	private static (string Family, float Scale)? CurrentFont => LocalizationManager.CurrentLanguageCode switch
	{
		var code when code.StartsWith("tzm-Tfng", StringComparison.OrdinalIgnoreCase) => ("Ebrima", 1F),
		var code when code.StartsWith("ff-Adlm", StringComparison.OrdinalIgnoreCase) => ("Ebrima", 1F),
		var code when code.StartsWith("bo-", StringComparison.OrdinalIgnoreCase) || code == "bo"
			|| code.StartsWith("dz-", StringComparison.OrdinalIgnoreCase) || code == "dz" => ("Microsoft Himalaya", 1.5F),
		_ => null
	};

	internal static void Apply(Control control)
	{
		if (control.IsDisposed || control.Disposing) return;
		if (control.Parent is { } parent && Bindings.TryGetValue(parent, out FontBinding? parentFont)
			&& parentFont.Owns(control.Font)) return;
		var choice = CurrentFont;
		if (choice == null && !Bindings.TryGetValue(control, out _)) return;
		FontBinding binding = Bindings.GetValue(control, _ =>
		{
			FontBinding result = new(control.Font);
			control.FontChanged += (_, _) => Apply(control);
			control.Disposed += (_, _) => result.Dispose();
			return result;
		});
		binding.Apply(control.Font, font => control.Font = font, choice);
		if (control is DataGridView grid)
		{
			ApplyStyle(grid, grid.ColumnHeadersDefaultCellStyle, choice);
			ApplyStyle(grid, grid.RowHeadersDefaultCellStyle, choice);
			ApplyStyle(grid, grid.DefaultCellStyle, choice);
			ApplyStyle(grid, grid.RowsDefaultCellStyle, choice);
			ApplyStyle(grid, grid.AlternatingRowsDefaultCellStyle, choice);
			foreach (DataGridViewColumn column in grid.Columns)
			{
				if (column.HasDefaultCellStyle) ApplyStyle(grid, column.DefaultCellStyle, choice);
				if (column.HeaderCell.HasStyle) ApplyStyle(grid, column.HeaderCell.Style, choice);
			}
		}
	}

	private static void ApplyStyle(DataGridView owner, DataGridViewCellStyle style, (string Family, float Scale)? choice)
	{
		if (style.Font is not Font current || (choice == null && !Bindings.TryGetValue(style, out _))) return;
		FontBinding binding = Bindings.GetValue(style, _ =>
		{
			FontBinding result = new(current);
			owner.Disposed += (_, _) => result.Dispose();
			return result;
		});
		binding.Apply(current, font => style.Font = font, choice);
	}

	internal static void Apply(ToolStripItem item)
	{
		if (item.IsDisposed) return;
		var choice = CurrentFont;
		if (choice == null && !Bindings.TryGetValue(item, out _)) return;
		FontBinding binding = Bindings.GetValue(item, target =>
		{
			FontBinding result = new(((ToolStripItem)target).Font);
			((ToolStripItem)target).Disposed += (_, _) => result.Dispose();
			return result;
		});
		binding.Apply(item.Font, font => item.Font = font, choice);
	}

	private sealed class FontBinding(Font original) : IDisposable
	{
		private Font _original = original;
		private Font? _applied;
		private (string Family, float Scale)? _choice;
		private bool _writing;
		internal bool Owns(Font font) => _applied != null && ReferenceEquals(font, _applied);

		internal void Apply(Font current, Action<Font> write, (string Family, float Scale)? choice)
		{
			if (_writing) return;
			bool changed = !ReferenceEquals(current, _applied) && !ReferenceEquals(current, _original);
			if (changed) _original = current;
			if (!changed && _choice == choice && (choice == null || ReferenceEquals(current, _applied))) return;
			Font? previous = _applied;
			_applied = choice is { } selected
				? new Font(selected.Family, _original.Size * selected.Scale, _original.Style, _original.Unit)
				: null;
			_choice = choice;
			_writing = true;
			try { write(_applied ?? _original); }
			finally { _writing = false; previous?.Dispose(); }
		}

		public void Dispose() => _applied?.Dispose();
	}
}
