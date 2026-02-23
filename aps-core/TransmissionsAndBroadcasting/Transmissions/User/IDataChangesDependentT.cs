namespace PT.Transmissions.User;

/// <summary>
/// Used to denote a transmission should be processed with output to a <see cref="IScenarioDataChanges" />.
/// This is used to structure the transmission-receiving pipeline, so such transmissions can output to that data changes object and handle any events afterward.
/// Devs wishing to implement this interface for any given transmission class Foo should implement IDataChangesDependentT&lt;Foo&gt; (see e.g. <see cref="UserBaseT" />.) - this soft contract is needed
/// for the interface to work.
/// Methods that need to interact with the transmission in this capacity should accept this IDataChangesDependentT; for other methods, an instance of the interface can always be safely cast as T given
/// the above convention.
/// Dev note: This behavior is currently unique to transmissions impacting the UserManager. In future, we may consider broadening that scope, or (if not needed) narrowing this definition.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IDataChangesDependentT<out T> where T : PTTransmissionBase { }