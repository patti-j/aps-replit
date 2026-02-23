namespace PT.Common.Delegates;

//Void
public delegate void VoidDelegate();

public delegate void VoidDelegateObject(object a_sender);

public delegate void VoidDelegateBool(bool a_boolParameter);

public delegate void VoidDelegateBoolString(bool a_boolParameter, string a_stringParameter);

public delegate void VoidDelegateString(string a_stringParameter);

public delegate void VoidDelegateStringString(string a_stringParameter, string a_stringParameter2);

public delegate void VoidDelegateStringInt(string a_stringParameter, int a_intParameter);

public delegate void VoidDelegateStringDateTime(string a_stringParameter, DateTime a_dateTimeParameter);

public delegate void VoidDelegateException(Exception a_exception);

//Bool
public delegate bool BoolDelegate();