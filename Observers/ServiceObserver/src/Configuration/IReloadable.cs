namespace ServiceObserver.Configuration
{
	public interface IReloadable
	{

		void reload(ServiceConfiguration newConfig);

	}
}