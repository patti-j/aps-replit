using Newtonsoft.Json;

namespace PT.Common;
#nullable enable
public class Result<Ok, Err>
{
    private Ok? m_ok;
    private Err? m_err;

    public bool IsOk
    {
        get;
        private set;
    }

    public bool IsErr
    {
        get => !IsOk;
    }

    public Result(Ok a_val)
    {
        m_ok = a_val;
        IsOk = true;
    }

    public Result(Err a_val)
    {
        m_err = a_val;
        IsOk = false;
    }

    public Ok Unwrap()
    {
        if (!IsOk)
        {
            throw new InvalidOperationException("Cannot Unwrap with no Ok");
        }

        return m_ok!;
    }

    public Err UnwrapErr()
    {
        if (!IsErr)
        {
            throw new InvalidOperationException("Cannot UnwrapErr with no Err");
        }

        return m_err!;
    }
    
    [JsonConstructor]
    private Result()
    {}
}
#nullable restore