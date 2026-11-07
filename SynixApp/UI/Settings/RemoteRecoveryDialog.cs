using System.Diagnostics;
using Synix_Control_Panel.SynixApp.Design;
using Synix_Control_Panel.SynixEngine;

namespace Synix_Control_Panel.SynixApp.UI.Settings;

internal sealed class RemoteRecoveryDialog : Form
{
    private readonly RemoteManagerService _service;
    private readonly CancellationTokenSource _closing = new();
    private readonly Label _status = new() { AutoSize = true, MaximumSize = new Size(570, 0) };
    private readonly TextBox _password = new() { ReadOnly = true, UseSystemPasswordChar = true, Dock = DockStyle.Fill };
    private readonly Button _create = new() { AutoSize = true, Enabled = false };
    private readonly Button _cancel = new() { AutoSize = true, Enabled = false };
    private readonly Button _refresh = new() { AutoSize = true };
    private readonly CheckBox _show = new() { AutoSize = true };
    private readonly System.Windows.Forms.Timer _timer = new() { Interval = 1000 };
    private RemoteRecoveryPassword? _issued;
    private bool _enabled, _busy;

    internal RemoteRecoveryDialog(RemoteManagerService service)
    {
        _service = service;
        LocalizationManager.BindText(this, "RemoteManager.RecoveryTitle");
        LocalizationManager.BindText(_create, "RemoteManager.RecoveryGenerate");
        LocalizationManager.BindText(_cancel, "RemoteManager.RecoveryCancel");
        LocalizationManager.BindText(_refresh, "ModManager.Button.Refresh");
        LocalizationManager.BindText(_show, "RemoteManager.RecoveryShow");
        LocalizationManager.BindAccessibleName(_password, "RemoteManager.RecoveryShow");
        Size = new Size(680, 610); MinimumSize = new Size(560, 520);
        StartPosition = FormStartPosition.CenterParent; BackColor = SettingsPalette.Window;
        ForeColor = SettingsPalette.PrimaryText; Font = new Font("Segoe UI", 10F);
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, Padding = new Padding(24), AutoScroll = true };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        void Add(Control control) { control.Margin = new Padding(0, 0, 0, 16); layout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); layout.Controls.Add(control, 0, layout.RowCount++); }
        var help = new Label { AutoSize = true, MaximumSize = new Size(570, 0) };
        LocalizationManager.BindText(help, "RemoteManager.RecoveryInstructions");
        Add(help); Add(_status); Add(_refresh); Add(_create); Add(_password); Add(_show); Add(_cancel);
        var open = new Button { AutoSize = true };
        LocalizationManager.BindText(open, "RemoteManager.OpenWebsite");
        open.Click += (_, _) => Process.Start(new ProcessStartInfo(RemoteManagerProtocol.Origin.AbsoluteUri) { UseShellExecute = true });
        Add(open); Controls.Add(layout);
        _show.CheckedChanged += (_, _) => _password.UseSystemPasswordChar = !_show.Checked;
        Deactivate += (_, _) => _show.Checked = false;
        _refresh.Click += async (_, _) => await Work(RefreshStatus);
        Shown += async (_, _) => await Work(RefreshStatus);
        _create.Click += async (_, _) => await Work(async () =>
        {
            var issued = await _service.CreateRecoveryPasswordAsync(_closing.Token);
            if (IsDisposed) return;
            _issued = issued; _show.Checked = false;
            LocalizationManager.SetDataText(_password, issued.TemporaryPassword);
            UpdateExpiry(); _timer.Start();
        });
        _cancel.Click += async (_, _) => await Work(async () =>
        {
            if (_issued == null) return;
            await _service.CancelRecoveryPasswordAsync(_issued.RecoveryId, _closing.Token);
            ClearPassword(); await RefreshStatus();
        });
        _timer.Tick += (_, _) => UpdateExpiry();
    }
    private async Task RefreshStatus()
    {
        var result = await _service.RecoveryStatusAsync(_closing.Token);
        if (IsDisposed) return;
        _enabled = result.Enabled;
        if (_issued != null) UpdateExpiry();
        else if (_enabled) LocalizationManager.BindText(_status, "RemoteManager.RecoveryReady", result.Username);
        else LocalizationManager.BindText(_status, "RemoteManager.WaitingApproval");
    }
    private void UpdateExpiry()
    {
        if (_issued == null) return;
        int seconds = (int)(_issued.ExpiresAt - DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        if (seconds <= 0 || _service.State is not { Enabled: true, PendingUnpair: false })
        {
            ClearPassword(); LocalizationManager.BindText(_status, "RemoteManager.RecoveryExpired"); RefreshButtons(); return;
        }
        LocalizationManager.BindText(_status, "RemoteManager.RecoveryExpires", _issued.Username, TimeSpan.FromSeconds(seconds).ToString(@"mm\:ss"));
    }
    private void ClearPassword()
    {
        _issued = null; _password.Clear(); _show.Checked = false; _timer.Stop();
    }
    private void RefreshButtons()
    {
        _create.Enabled = !_busy && _enabled && _issued == null;
        _cancel.Enabled = !_busy && _issued != null;
        _refresh.Enabled = !_busy;
        _show.Enabled = _issued != null;
    }
    private async Task Work(Func<Task> work)
    {
        if (_busy) return; _busy = true; RefreshButtons();
        try { await work(); }
        catch (OperationCanceledException) when (_closing.IsCancellationRequested) { }
        catch (Exception ex)
        {
            if (!IsDisposed) LocalizationManager.SetDataText(_status,
                ex is InvalidOperationException ? ex.Message : LocalizationManager.Get("RemoteManager.Unavailable"));
        }
        finally { _busy = false; if (!IsDisposed) RefreshButtons(); }
    }
    protected override void Dispose(bool disposing)
    {
        if (disposing) { _closing.Cancel(); ClearPassword(); _timer.Dispose(); _closing.Dispose(); }
        base.Dispose(disposing);
    }
}
