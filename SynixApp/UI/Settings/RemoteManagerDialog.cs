using System.Diagnostics;
using Synix_Control_Panel.SynixApp.Design;
using Synix_Control_Panel.SynixEngine;

namespace Synix_Control_Panel.SynixApp.UI.Settings;

internal sealed class RemoteManagerDialog : Form
{
    private readonly RemoteManagerService _service = RemoteManagerService.Shared;
    private readonly Label _status = new() { AutoSize = true, MaximumSize = new Size(540, 0) };
    private readonly TextBox _name = new() { MaxLength = 60, Dock = DockStyle.Fill };
    private readonly TextBox _code = new() { ReadOnly = true, Dock = DockStyle.Fill, Font = new Font("Consolas", 18F), TextAlign = HorizontalAlignment.Center };
    private readonly Label _claim = new() { AutoSize = true, MaximumSize = new Size(540, 0) };
    private readonly Button _create = new() { AutoSize = true };
    private readonly Button _approve = new() { AutoSize = true, Enabled = false };
    private readonly Button _unpair = new() { AutoSize = true };
    private readonly Button _recover = new() { AutoSize = true };
    private readonly System.Windows.Forms.Timer _timer = new() { Interval = 5000 };
    private string? _claimId, _claimAccount;
    private bool _checking, _busy;
    private DateTimeOffset _pairUntil;

    internal RemoteManagerDialog()
    {
        LocalizationManager.BindText(this, "RemoteManager.Title");
        LocalizationManager.BindText(_create, "RemoteManager.GenerateCode");
        LocalizationManager.BindText(_approve, "RemoteManager.Approve");
        LocalizationManager.BindText(_unpair, "RemoteManager.Unpair");
        LocalizationManager.BindText(_recover, "RemoteManager.RecoveryTitle");
        LocalizationManager.SetDataText(_name, LocalizationManager.Get("RemoteManager.DefaultComputer"));
        LocalizationManager.SetDataText(_code, string.Empty);
        Size = new Size(660, 720); MinimumSize = new Size(550, 580);
        StartPosition = FormStartPosition.CenterParent; BackColor = SettingsPalette.Window;
        ForeColor = SettingsPalette.PrimaryText; Font = new Font("Segoe UI", 10F);
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, Padding = new Padding(24), AutoScroll = true };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        void Add(Control control) { control.Margin = new Padding(0, 0, 0, 16); layout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); layout.Controls.Add(control, 0, layout.RowCount++); }
        var heading = new Label { AutoSize = true, Font = new Font("Segoe UI", 14F, FontStyle.Bold), MaximumSize = new Size(540, 0) };
        LocalizationManager.BindText(heading, "RemoteManager.Heading"); Add(heading);
        Add(_status);
        var nameLabel = new Label { AutoSize = true, MaximumSize = new Size(540, 0) };
        LocalizationManager.BindText(nameLabel, "RemoteManager.ComputerName"); Add(nameLabel); Add(_name); Add(_create); Add(_code);
        var open = new Button { AutoSize = true };
        LocalizationManager.BindText(open, "RemoteManager.OpenWebsite");
        open.Click += (_, _) => Process.Start(new ProcessStartInfo(RemoteManagerProtocol.Origin.AbsoluteUri) { UseShellExecute = true });
        Add(open); Add(_claim); Add(_approve); Add(_unpair);
        Add(_recover);
        _recover.Click += (_, _) => { using var dialog = new RemoteRecoveryDialog(_service); dialog.ShowDialog(this); };
        var instructions = new Label { AutoSize = true, MaximumSize = new Size(540, 0), ForeColor = SettingsPalette.SecondaryText };
        LocalizationManager.BindText(instructions, "RemoteManager.Instructions"); Add(instructions);
        Controls.Add(layout);
        _create.Click += async (_, _) => await Work(async () =>
        {
            if (string.IsNullOrWhiteSpace(_name.Text)) throw new InvalidOperationException(LocalizationManager.Get("RemoteManager.EnterName"));
            var reply = await _service.BeginPairingAsync(_name.Text);
            LocalizationManager.SetDataText(_code, reply.Code); _pairUntil = DateTimeOffset.UtcNow.AddSeconds(reply.ExpiresIn);
            _claimId = null; _claimAccount = null; LocalizationManager.BindText(_claim, "RemoteManager.WaitingClaim"); _timer.Start();
        });
        _approve.Click += async (_, _) => await Work(async () =>
        {
            if (_claimId == null || _claimAccount == null) return;
            await _service.ApproveAsync(_claimId, _claimAccount);
            _code.Clear(); LocalizationManager.BindText(_claim, "RemoteManager.ApprovedFormat", _claimAccount); _claimId = null; _timer.Start();
        });
        _unpair.Click += async (_, _) => await Work(async () =>
        {
            _timer.Stop(); _claimId = null; _code.Clear(); LocalizationManager.SetDataText(_claim, string.Empty);
            await _service.UnpairAsync();
        });
        _timer.Tick += async (_, _) =>
        {
            if (_checking || _busy) return;
            if (_service.State?.Enabled == true) { RefreshState(); return; }
            if (DateTimeOffset.UtcNow >= _pairUntil) { _timer.Stop(); _claimId = null; LocalizationManager.BindText(_claim, "RemoteManager.CodeExpired"); RefreshState(); return; }
            _checking = true;
            try
            {
                var reply = await _service.CheckPairingAsync();
                if (IsDisposed) return;
                _claimId = reply.ClaimId; _claimAccount = reply.Username;
                if (reply.Username == null) LocalizationManager.BindText(_claim, "RemoteManager.WaitingClaim");
                else LocalizationManager.BindText(_claim, "RemoteManager.ClaimFormat", reply.Username);
                RefreshState();
            }
            catch { if (!IsDisposed) LocalizationManager.BindText(_status, "RemoteManager.CheckFailed"); }
            finally { _checking = false; }
        };
        LocalizationManager.LanguageChanged += LanguageChanged;
        RefreshState();
        if (_service.State?.Enabled == true) _timer.Start();
    }
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            LocalizationManager.LanguageChanged -= LanguageChanged;
            _timer.Dispose();
        }
        base.Dispose(disposing);
    }
    private async Task Work(Func<Task> action)
    {
        if (_busy) return; _busy = true; RefreshState();
        try { await action(); }
        catch (Exception ex)
        {
            if (!IsDisposed)
            {
                if (ex is HttpRequestException or OperationCanceledException) LocalizationManager.BindText(_claim, "RemoteManager.Unavailable");
                else LocalizationManager.SetDataText(_claim, ex.Message);
            }
        }
        finally { _busy = false; if (!IsDisposed) RefreshState(); }
    }
    private void RefreshState()
    {
        var state = _service.State;
        LocalizationManager.SetDataText(_status, _service.ConnectionStatus + (state == null ? "" : Environment.NewLine +
            LocalizationManager.Get(state.Hardware ? "RemoteManager.IdentityTpm" : "RemoteManager.IdentitySoftware")));
        _create.Enabled = !_busy && state?.Enabled != true;
        _approve.Enabled = !_busy && _claimId != null && state?.Enabled != true;
        _unpair.Enabled = !_busy && state != null;
        _recover.Enabled = !_busy && state is { Enabled: true, PendingUnpair: false };
    }
    private void LanguageChanged(object? sender, EventArgs args) { if (!IsDisposed) RefreshState(); }
}
