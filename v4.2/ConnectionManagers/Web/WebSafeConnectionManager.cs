using System;
using System.Web;
using Zonkey.ConnectionManagers.Specialized;

namespace Zonkey.ConnectionManagers.Web
{
	/// <summary>
	/// A Thread-safe and Web-Safe connection manager
	/// </summary>
	public class WebSafeConnectionManager: BaseConnectionManager
	{
		[ThreadStatic] 
		private static ConnectionManagerContext _threadContext;

		private const string CTX_ITEM_NAME = "Zonkey.ConnectionManager.Context";

		protected override ConnectionManagerContext Context
		{
			get
			{
				if (HttpContext.Current == null)
					return (_threadContext ?? (_threadContext = new ConnectionManagerContext()));

				var ctx = HttpContext.Current.Items[CTX_ITEM_NAME] as ConnectionManagerContext;
				if (ctx == null)
				{
					ctx = new ConnectionManagerContext();
					HttpContext.Current.Items[CTX_ITEM_NAME] = ctx;
				}

				return ctx;					
			}
		}

		protected override void OnPrepareConnection()
		{
			// check if transaction needed
			if (HttpContext.Current != null)
			switch (HttpContext.Current.Request.Headers["X-Zonkey-Transaction"])
			{
				case "required":
				case "auto-rollback":
					GetTransaction();
					break;
			}
		}
	}
}
