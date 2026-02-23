/// 
/// DispatcherDefinitionManager contains a set of BalancedCompositeDispatcherDefinition classes. 
/// 
/// Dispather defintions and their inheritance relationships are shown below.
/// *=abstract
/// 
/// DispatcherDefinition --> BalancedCompositeDispatcherDefinition
/// *ReadyActivitiesDispatcher --> *ArrayListDispatcher --> *SortedListDispatcher -- > BalancedCompositeDispatcher
/// *ReadyActivitiesDispatcher --> StaticCompositeDispatcher
/// 
/// DispatcherDefinition --> MoveDispatcherDefinition
/// *ReadyActivitiesDispatcher --> *ArrayListDispatcher --> *SortedListDispatcher -- > MoveDispatcher
///
///
/// Both dispatchers make use of KeyAndActivity, where keys specific to the dispatchers and activities are stored within the sorted list. 
/// Test

