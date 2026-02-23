using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Create a new User by copying the specified User.
/// </summary>
public class UserResetMyPasswordT : UserIdBaseT, IPTSerializable
{
    public const int UNIQUE_ID = 724;

    #region IPTSerializable Members
    public UserResetMyPasswordT(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 1)
        {
            a_reader.Read(out m_currentPassword);
            a_reader.Read(out m_newPassword);
            m_bools = new BoolVector32(a_reader);
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);

        a_writer.Write(m_currentPassword);
        a_writer.Write(m_newPassword);
        m_bools.Serialize(a_writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public UserResetMyPasswordT() { }

    /// <summary>
    /// </summary>
    /// <param name="a_currentPassword">Must match the user's current password to validate this transmission.</param>
    /// <param name="a_newPassword">The password to set the user's password to.</param>
    public UserResetMyPasswordT(BaseId a_userId, string a_currentPassword, string a_newPassword, bool a_resetPwdOnNextLogin) : base(a_userId)
    {
        m_currentPassword = a_currentPassword;
        m_newPassword = a_newPassword;
        ResetPasswordOnNextLogin = a_resetPwdOnNextLogin;
        ResetAdminUser = false;
    }

    /// <summary>
    /// </summary>
    /// <param name="a_currentPassword">Must match the user's current password to validate this transmission.</param>
    /// <param name="a_newPassword">The password to set the user's password to.</param>
    public UserResetMyPasswordT(BaseId a_userId, string a_newPassword) : base(a_userId)
    {
        m_newPassword = a_newPassword;
        ResetAdminUser = true;
    }

    private readonly string m_currentPassword;

    /// <summary>
    /// Must match the user's current password to validate this transmission.
    /// </summary>
    public string CurrentPassword => m_currentPassword;

    private readonly string m_newPassword;

    /// <summary>
    /// The password to set the user's password to.
    /// </summary>
    public string NewPassword => m_newPassword;

    private BoolVector32 m_bools;
    private const int c_resetPasswordOnNextLoginIdx = 0;
    private const int c_resetAdminUserIdx = 1;

    /// <summary>
    /// The requirement for password reset on next login for user.
    /// </summary>
    public bool ResetPasswordOnNextLogin
    {
        get => m_bools[c_resetPasswordOnNextLoginIdx];
        set => m_bools[c_resetPasswordOnNextLoginIdx] = value;
    }

    /// <summary>
    /// The requirement for password reset on next login for user.
    /// </summary>
    public bool ResetAdminUser
    {
        get => m_bools[c_resetAdminUserIdx];
        set => m_bools[c_resetAdminUserIdx] = value;
    }
}