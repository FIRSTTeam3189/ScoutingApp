using System;
using Microsoft.WindowsAzure.MobileServices;
using Scouty.Utility;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Scouty.Azure
{
	public static class AzureManager
	{
		public static MobileServiceClient Client { get; private set; }
		private static readonly Logger logger = new Logger(typeof(AzureManager));

		#if !__ANDROID__
		public static void Init(string applicationUrl){
			CurrentPlatform.Init ();
			Client = new MobileServiceClient (applicationUrl);
		}
		#else
		public static void Init(string applicationUrl){
			Client = new MobileServiceClient(applicationUrl);
		}
		#endif

		/// <summary>
		/// Calls the InvokeApiAsync of the azure client
		/// </summary>
		/// <returns>The result of the api.</returns>
		/// <param name="api">The API to call.</param>
		/// <param name="input">Input of the api.</param>
		/// <typeparam name="TIn">The 1st type parameter.</typeparam>
		/// <typeparam name="T">The 2nd type parameter.</typeparam>
		public static async Task<T> InvokeApiAsync<TIn, T>(string api, TIn input){
			return await Client.InvokeApiAsync<TIn, T> (api, input);
		}

		/// <summary>
		/// Calls the InvokeApiAsync of the Azure Client
		/// </summary>
		/// <returns>The result of the api.</returns>
		/// <param name="api">API to call.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static async Task<T> InvokeApiAsync<T>(string api){
			return await Client.InvokeApiAsync<T> (api);
		}

		public static async Task<T> InvokeGetApiAsync<T>(string api, IDictionary<string, string> parameters){
			return await Client.InvokeApiAsync<T> (api, System.Net.Http.HttpMethod.Get, parameters);
		}
	}
}

