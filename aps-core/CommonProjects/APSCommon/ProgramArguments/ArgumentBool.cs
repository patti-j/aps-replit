namespace PT.APSCommon.ProgramArguments;

public class ArgumentBool : Argument
{
    public ArgumentBool(string a_name, EValueAfterNameRequirement a_valueAfterNameRequirement) :
        base(a_name, a_valueAfterNameRequirement) { }

    public ArgumentBool(string a_name, EValueAfterNameRequirement a_valueAfterNameRequirement, params string[] a_aliasNames)
        : base(a_name, a_valueAfterNameRequirement, a_aliasNames) { }

    public ArgumentBool(string a_name, bool a_defaultValue, EValueAfterNameRequirement a_valueAfterNameRequirement, params string[] a_aliasNames)
        : base(a_name, a_defaultValue.ToString(), a_valueAfterNameRequirement, a_aliasNames) { }

    public virtual bool ValueBool { get; private set; }

    protected override void ValueSet(string a_value)
    {
        base.ValueSet(a_value);
        ValueBool = bool.Parse(a_value);
    }
}