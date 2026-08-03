// ============================================================================
//  ZFTP — ConnectionProfile
//  A saved server: everything ZFTP needs to connect and where to mount it.
//  This is what we'll later save to disk so your servers persist between runs.
// ============================================================================

namespace ZFTP.Core;

public enum AuthMethod
{
    Password,
    PrivateKey,
}

public enum AccessMode
{
    ReadWrite,   // default — full read & write
    ReadOnly,    // browse/copy out only; writes are blocked
}

/// <summary>
/// WebDAV server flavour (Provider == WebDav). rclone's webdav backend needs this
/// to know the server's quirks - which quota/PROPFIND response shape it returns
/// and how it wants uploads chunked. Left as "Other" (rclone's generic DAV
/// handling), Nextcloud/ownCloud servers report the wrong (or no) free space and
/// can reject uploads that a real Nextcloud/ownCloud vendor setting would allow.
/// </summary>
public enum WebDavVendor
{
    Other,
    Nextcloud,
    OwnCloud,
    Sharepoint,
}

/// <summary>
/// Which kind of storage this drive connects to. SFTP uses ZFTP's own native
/// engine; everything else is mounted through the bundled rclone engine.
/// </summary>
public enum ProviderType
{
    Sftp,          // native engine
    Ftp,           // rclone
    Ftps,          // rclone (FTP over TLS)
    WebDav,        // rclone
    S3,            // rclone (Amazon S3 + compatible: Wasabi, B2, DO Spaces, ...)
    GoogleDrive,   // rclone (browser sign-in)
    Dropbox,       // rclone (browser sign-in)
    OneDrive,      // rclone (browser sign-in)
    Box,           // rclone (browser sign-in)
    Smb,           // rclone (Windows/Samba network share)
    B2,            // rclone (Backblaze B2)
    Azure,         // rclone (Microsoft Azure Blob Storage)
    Mega,          // rclone (Mega.nz)
    Proton,        // rclone (Proton Drive)
    Android,       // native engine (adb over USB)
    IPhone,        // native engine (Apple AFC over USB) - LIMITED: photos + file-sharing apps only

    // ---- appended later - Provider is persisted as its plain int value in
    // drives.json, so new entries must only ever be ADDED HERE AT THE END.
    // Reordering or inserting earlier would silently reassign every saved
    // profile after the insertion point to the wrong provider. ----
    PCloud,             // rclone (browser sign-in)
    Yandex,             // rclone (browser sign-in)
    PremiumizeMe,       // rclone (browser sign-in)
    PutIo,              // rclone (browser sign-in)
    HiDrive,            // rclone (browser sign-in)
    Jottacloud,         // rclone (browser sign-in / personal login token)
    GoogleCloudStorage, // rclone (browser sign-in; bucket-rooted like S3)
    Seafile,            // rclone (url/user/pass + optional library)
    Storj,              // rclone (existing access grant)
    Swift,              // rclone (OpenStack Swift / Rackspace / Memset / OVH; container-rooted)
    Koofr,              // rclone (Koofr / Digi Storage / Koofr-compatible)
    Http,               // rclone (read-only HTTP directory listing)
}

public sealed class ConnectionProfile
{
    /// <summary>Stable unique id so the app can track a profile across edits/saves.</summary>
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>Friendly name shown in the app AND as the drive label in Explorer.</summary>
    public string Name { get; set; } = "New Server";

    /// <summary>What kind of storage this is. SFTP = native; others go through rclone.</summary>
    public ProviderType Provider { get; set; } = ProviderType.Sftp;

    /// <summary>Accent colour for this drive in the list, as a hex string like "#2D7DD2".</summary>
    public string Color { get; set; } = "#2D7DD2";

    public string Host { get; set; } = "";
    public int Port { get; set; } = 22;
    public string Username { get; set; } = "";

    public AuthMethod Auth { get; set; } = AuthMethod.Password;

    // NOTE: for now the password lives here in memory. A later step moves it
    // into the Windows Credential Manager so it's never stored in plain text.
    public string Password { get; set; } = "";

    public string KeyPath { get; set; } = "";
    public string KeyPassphrase { get; set; } = "";

    /// <summary>SFTP server's host-key fingerprint (SHA-256), remembered on first
    /// connect. If it ever changes, the connection is refused (possible MITM).</summary>
    public string KnownHostKey { get; set; } = "";

    /// <summary>Remote folder to show as the drive root. "/" means "my home folder".</summary>
    public string RemoteRoot { get; set; } = "/";

    // ---- provider-specific (only used by the relevant Provider) ------------

    /// <summary>WebDAV server URL, e.g. https://dav.example.com/remote.php/webdav.</summary>
    public string Url { get; set; } = "";

    /// <summary>WebDAV server flavour (Provider == WebDav). Defaults to "Other" for
    /// backwards compatibility with profiles saved before this setting existed.</summary>
    public WebDavVendor WebDavVendor { get; set; } = WebDavVendor.Other;

    /// <summary>Optional personal OAuth app credentials (cloud drives). Blank = use
    /// rclone's built-in sign-in. Providing your own makes logins permanent.</summary>
    public string ClientId { get; set; } = "";
    public string ClientSecret { get; set; } = "";

    /// <summary>S3-compatible settings.</summary>
    public string S3AccessKey { get; set; } = "";
    public string S3Secret { get; set; } = "";
    public string S3Region { get; set; } = "";
    public string S3Endpoint { get; set; } = "";   // blank = Amazon AWS; set for Wasabi/B2/DO/etc.
    public string S3Bucket { get; set; } = "";

    /// <summary>SMB/CIFS share name (Provider == Smb), e.g. "Public". Combined with
    /// RemoteRoot for the mounted path. Uses Host/Port/Username/Password above.</summary>
    public string SmbShare { get; set; } = "";

    /// <summary>Optional NTLM domain/workgroup for the SMB login (Provider == Smb).</summary>
    public string SmbDomain { get; set; } = "";

    /// <summary>Backblaze B2 credentials (Provider == B2).</summary>
    public string B2AccountId { get; set; } = "";
    public string B2ApplicationKey { get; set; } = "";
    public string B2Bucket { get; set; } = "";

    /// <summary>Microsoft Azure Blob Storage credentials (Provider == Azure).</summary>
    public string AzureAccount { get; set; } = "";
    public string AzureKey { get; set; } = "";
    public string AzureContainer { get; set; } = "";

    /// <summary>Proton Drive (Provider == Proton). Uses Username/Password above.
    /// TwoFactorCode is only consumed once, at the first successful connection -
    /// rclone caches the resulting session itself, so a stale code afterwards is
    /// harmless. MailboxPassword is only needed for "two-password mode" accounts.</summary>
    public string ProtonTwoFactorCode { get; set; } = "";
    public string ProtonMailboxPassword { get; set; } = "";

    /// <summary>Seafile library/repo to mount (Provider == Seafile). Blank = every
    /// non-encrypted library the account can see. Uses Url/Username/Password above.</summary>
    public string SeafileLibrary { get; set; } = "";

    /// <summary>A pre-generated Storj access grant (Provider == Storj) - the simplest
    /// of Storj's auth options, so it's the only one exposed here.</summary>
    public string StorjAccessGrant { get; set; } = "";

    /// <summary>OpenStack Swift (Provider == Swift). Uses Username as "user", Password
    /// as "key" (API key/password), and Url as the Keystone auth URL.</summary>
    public string SwiftTenant { get; set; } = "";
    public string SwiftContainer { get; set; } = "";

    /// <summary>Koofr / Digi Storage / other Koofr-compatible service (Provider ==
    /// Koofr). "koofr" or "digistorage" need nothing else; "other" also needs
    /// Endpoint. Uses Username + Password (an app-specific password, not the
    /// account password - Koofr generates one under its own security settings).</summary>
    public string KoofrProvider { get; set; } = "koofr";
    public string KoofrEndpoint { get; set; } = "";

    /// <summary>Google Cloud Storage bucket to mount (Provider == GoogleCloudStorage).</summary>
    public string GcsBucket { get; set; } = "";

    /// <summary>ADB serial of the chosen Android device (Provider == Android). Blank
    /// means "use the only connected device" and is resolved at mount time.</summary>
    public string DeviceSerial { get; set; } = "";

    /// <summary>Drive letter to mount on, e.g. "Z". Windows only.</summary>
    public string DriveLetter { get; set; } = "Z";

    /// <summary>Directory to mount on (Linux/macOS only - there's no drive-letter
    /// concept there). Blank means "pick a default under ~/ZFTP/mounts".</summary>
    public string MountPath { get; set; } = "";

    /// <summary>Read &amp; write (default) or read-only.</summary>
    public AccessMode Access { get; set; } = AccessMode.ReadWrite;

    /// <summary>If false, this drive is shown but skipped by "Mount all" / auto-mount.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>Mount this drive automatically when ZFTP starts.</summary>
    public bool AutoMount { get; set; } = false;

    public ConnectionProfile Clone() => (ConnectionProfile)MemberwiseClone();

    /// <summary>Copy every editable field from another profile (keeps this object's identity).</summary>
    public void CopyFrom(ConnectionProfile o)
    {
        Name = o.Name; Host = o.Host; Port = o.Port; Username = o.Username;
        Auth = o.Auth; Password = o.Password; KeyPath = o.KeyPath; KeyPassphrase = o.KeyPassphrase;
        KnownHostKey = o.KnownHostKey;
        DeviceSerial = o.DeviceSerial;
        RemoteRoot = o.RemoteRoot; DriveLetter = o.DriveLetter; MountPath = o.MountPath; Enabled = o.Enabled; AutoMount = o.AutoMount;
        Access = o.Access; Provider = o.Provider; Color = o.Color;
        Url = o.Url; WebDavVendor = o.WebDavVendor; S3AccessKey = o.S3AccessKey; S3Secret = o.S3Secret;
        S3Region = o.S3Region; S3Endpoint = o.S3Endpoint; S3Bucket = o.S3Bucket;
        SmbShare = o.SmbShare; SmbDomain = o.SmbDomain;
        B2AccountId = o.B2AccountId; B2ApplicationKey = o.B2ApplicationKey; B2Bucket = o.B2Bucket;
        AzureAccount = o.AzureAccount; AzureKey = o.AzureKey; AzureContainer = o.AzureContainer;
        ProtonTwoFactorCode = o.ProtonTwoFactorCode; ProtonMailboxPassword = o.ProtonMailboxPassword;
        ClientId = o.ClientId; ClientSecret = o.ClientSecret;
        SeafileLibrary = o.SeafileLibrary; StorjAccessGrant = o.StorjAccessGrant;
        SwiftTenant = o.SwiftTenant; SwiftContainer = o.SwiftContainer;
        KoofrProvider = o.KoofrProvider; KoofrEndpoint = o.KoofrEndpoint;
        GcsBucket = o.GcsBucket;
    }
}
