using System;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using Scouty.Utility;

namespace Scouty.Azure
{
	public static class UserManager
	{
		static readonly Logger logger = new Logger (typeof(UserManager));

		const string REGISTER_API = "accounts/Register";
		const string LOGIN_API = "accounts/Login";
		const string GET_ACCOUNT_INFO_API = "accounts/GetMyAccount";
		const string CUSTOM_REGISTER_API = "accounts/CustomRegister";

		public static LoginInfo User { get; private set; }
		public static bool IsLoggedIn { get { return User != null; } }

		/// <summary>
		/// Login the specified username and password.
		/// </summary>
		/// <param name="username">Username.</param>
		/// <param name="password">Password.</param>
		public static async Task<LoginState> Login(string username, string password){
			if (string.IsNullOrWhiteSpace (username))
				throw new ArgumentNullException (nameof (username));

			if (string.IsNullOrWhiteSpace (password))
				throw new ArgumentNullException (nameof (password));

			// Fire off the request and get the login info
			try {
				var req = new LoginRequest(){
					Password = password,
					Username = username
				};

				var info = await AzureManager.InvokeApiAsync<LoginRequest, LoginInfo>(LOGIN_API, req);
				User = info;

				// Set Azure Client now
				var client = new MobileServiceUser(info.UserId){
					MobileServiceAuthenticationToken = info.MobileServiceAuthenticationToken
				};

				AzureManager.Client.CurrentUser = client;

				return LoginState.Success;
			} catch (MobileServiceInvalidOperationException e){
				if (e.Response.StatusCode == System.Net.HttpStatusCode.BadRequest) {
					logger.Error ("Invalid Username/Password");
					return LoginState.InvalidLogin;
				} else {
					logger.Error ("Something happened on the server: " + e.Response.StatusCode);
					return LoginState.ServerFailure;
				}
			}
		}

		/// <summary>
		/// Customs the register.
		/// </summary>
		/// <returns>The state of the register</returns>
		/// <param name="username">Username to use.</param>
		/// <param name="password">Password to use.</param>
		/// <param name="realName">Real name to use.</param>
		public static async Task<RegisterState> CustomRegister(string username, string password, string realName){
			if (string.IsNullOrWhiteSpace (username))
				throw new ArgumentNullException (nameof (username));
			if (string.IsNullOrWhiteSpace (password))
				throw new ArgumentNullException (nameof (password));
			if (string.IsNullOrWhiteSpace(realName))
				throw new ArgumentNullException(nameof(realName));

			// Fire off the request to register
			try {
				var req = new CustomRegistrationRequest(){
					Username = username,
					Password = password,
					RealName = realName
				};

				await AzureManager.InvokeApiAsync<CustomRegistrationRequest, string>(CUSTOM_REGISTER_API, req);

				return RegisterState.Success;
			} catch (MobileServiceInvalidOperationException e){
				if (e.Response.StatusCode == System.Net.HttpStatusCode.BadRequest) {
					logger.Error ("Username Exists/Invalid password");
					logger.Error (await e.Response.Content.ReadAsStringAsync ());
					return RegisterState.InvalidRegister;
				} else {
					logger.Error ("Something happened on the server: " + e.Response.StatusCode);
					return RegisterState.ServerFailure;
				}
			}
		}
	}

	public enum LoginState {
		Success,
		ServerFailure,
		InvalidLogin
	}

	public enum RegisterState {
		Success,
		ServerFailure,
		InvalidRegister
	}

	public class CustomRegistrationRequest {
		public string Username { get; set; }
		public string Password { get; set; }
		public string RealName { get; set; }
	}

	public class RegisterationRequest {
		public string Username { get; set; }
		public string RealName { get; set; }
	}

	public class LoginRequest {
		public string Username { get; set; }
		public string Password { get; set; }
	}

	public class ClientAccount {
		public string Id { get; set; }
		public string Username { get; set; }
		public string RealName { get; set; }
		public string TeamNumber { get; set; }
	}

	public class LoginInfo {
		public string UserId { get; set; }
		public string MobileServiceAuthenticationToken { get; set; }
		public ClientAccount AccountInfo { get; set; }
	}
}

