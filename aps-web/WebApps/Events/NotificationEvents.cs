namespace ReportsWebApp.Events
{
	public static class NotificationEvents
	{
		public delegate Task AsyncEventHandler();
		public static event AsyncEventHandler? SubscriptionsChanged;

		public static async Task OnSubscriptionChanged()
		{
			if (SubscriptionsChanged != null)
			{
				await SubscriptionsChanged();
			}
		}
	}
}
